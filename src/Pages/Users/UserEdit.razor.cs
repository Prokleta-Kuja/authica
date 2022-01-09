using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Models;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace authica.Pages.Users
{
    public partial class UserEdit : IDisposable
    {
        [Inject] public CurrentSession Session { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
        [Inject] private IPasswordHasher Hasher { get; set; } = null!;
        [Inject] private AuthorizationStore AuthzStore { get; set; } = null!;
        [Parameter] public Guid AliasId { get; set; }
        private AppDbContext _db = null!;
        private UserCreateModel? _create;
        private UserEditModel? _edit;
        private User? _item;
        private HashSet<string> _names = new();
        private HashSet<string> _emails = new();
        private Guid? _selectedRole;
        private Dictionary<Guid, Role> _userRoles = new();
        private Dictionary<Guid, Role> _allRoles = new();
        private Dictionary<string, string>? _errors;
        private IUsers _t = LocalizationFactory.Users();

        protected override async Task OnInitializedAsync()
        {
            if (!Session.IsAuthenticated)
                Nav.NavigateTo(C.Routes.SignIn, true);
            else if (!Session.HasClaim(Claims.IsAdmin))
                Nav.NavigateTo(C.Routes.Forbidden);
            else
                _t = LocalizationFactory.Users(Session.LocaleId);

            _db = await DbFactory.CreateDbContextAsync();

            base.OnInitialized();
        }
        public void Dispose() => _db?.Dispose();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var users = await _db.Users.ToListAsync();

            if (AliasId == Guid.Empty)
                _create = new();
            else
            {
                _item = users.FirstOrDefault(a => a.AliasId == AliasId);

                if (_item != null)
                    _edit = new(_item);
            }

            foreach (var user in users)
                if (user.AliasId != AliasId)
                {
                    _names.Add(user.UserName.ToLower());
                    _emails.Add(user.Email.ToLower());
                }

            _allRoles = await _db.Roles
                .Include(r => r.UserRoles)
                .OrderBy(r => r.Name)
                .ToDictionaryAsync(r => r.AliasId);

            if (_item != null)
                foreach (var role in _allRoles)
                    if (role.Value.UserRoles!.Any(ur => ur.UserId == _item.UserId))
                        _userRoles.Add(role.Key, role.Value);

            foreach (var role in _userRoles)
                _allRoles.Remove(role.Key);

            StateHasChanged();
        }
        void CancelClicked() => Nav.NavigateTo(C.Routes.Users);
        async Task SaveCreateClicked()
        {
            if (_create == null)
                return;

            _errors = _create.Validate(_t, _emails, _names);
            if (_errors != null)
                return;

            _item = new User(_create.Email!, _create.UserName!, _create.FirstName, _create.LastName);
            _item.TimeZone = _create.TimeZone;
            _item.Locale = _create.Locale;
            _item.IsAdmin = _create.IsAdmin;
            _item.Disabled = _create.Disabled ? DateTime.UtcNow : null;

            if (!string.IsNullOrWhiteSpace(_create.Password))
                _item.SetPassword(_create.Password, Hasher);

            _db.Users.Add(_item);
            await _db.SaveChangesAsync();

            AuthzStore.ClearAuthorizations();

            Nav.NavigateTo(C.Routes.UsersFor(_item.AliasId));
            _create = null;
            _edit = new(_item);
            StateHasChanged();
        }
        async Task SaveEditClicked()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t, _emails, _names);
            if (_errors != null)
                return;

            _item.Email = _edit.Email!;
            _item.UserName = _edit.UserName!;
            _item.FirstName = _edit.FirstName;
            _item.LastName = _edit.LastName;
            _item.TimeZone = _edit.TimeZone;
            _item.Locale = _edit.Locale;
            _item.IsAdmin = _edit.IsAdmin;

            if (!_item.Disabled.HasValue && _edit.Disabled)
                _item.Disabled = DateTime.UtcNow;
            else if (_item.Disabled.HasValue && !_edit.Disabled)
                _item.Disabled = null;

            if (!string.IsNullOrWhiteSpace(_edit.NewPassword))
                _item.SetPassword(_edit.NewPassword, Hasher);

            await _db.SaveChangesAsync();

            AuthzStore.ClearAuthorizations();
        }
        void AddRole()
        {
            if (!_selectedRole.HasValue || !_allRoles.TryGetValue(_selectedRole.Value, out var role))
                return;

            role.UserRoles!.Add(new UserRole { UserId = _item!.UserId });

            _userRoles.Add(role.AliasId, role);
            _allRoles.Remove(role.AliasId);
            _selectedRole = null;

            StateHasChanged();
        }
        void RemoveRole(Guid roleAliasId)
        {
            if (!_userRoles.TryGetValue(roleAliasId, out var role))
                return;

            var userRole = role.UserRoles!.Single(ur => ur.UserId == _item!.UserId);
            role.UserRoles!.Remove(userRole);

            _allRoles.Add(role.AliasId, role);
            _userRoles.Remove(role.AliasId);

            StateHasChanged();
        }
    }
}