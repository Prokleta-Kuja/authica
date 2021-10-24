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

namespace authica.Pages.Apps
{
    public partial class AppEdit : IDisposable
    {
        [Inject] public CurrentSession Session { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IPasswordHasher Hasher { get; set; } = null!;
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
        [Parameter] public Guid AliasId { get; set; }
        private AppDbContext _db = null!;
        private AppCreateModel? _create;
        private AppEditModel? _edit;
        private Entities.App? _item;
        private HashSet<string> _names = new();
        private Guid? _selectedRole;
        private Dictionary<Guid, Role> _appRoles = new();
        private Dictionary<Guid, Role> _allRoles = new();
        private Dictionary<string, string>? _errors;
        private IApps _t = LocalizationFactory.Apps();

        protected override void OnInitialized()
        {
            if (Session.IsAuthenticated)
                _t = LocalizationFactory.Apps(Session.LocaleId);
            _db = DbFactory.CreateDbContext();

            base.OnInitialized();
        }
        public void Dispose() => _db?.Dispose();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var apps = await _db.Apps.ToListAsync();

            if (AliasId == Guid.Empty)
                _create = new();
            else
            {
                _item = apps.FirstOrDefault(a => a.AliasId == AliasId);

                if (_item != null)
                    _edit = new(_item);
            }

            foreach (var app in apps)
                if (app.AliasId != AliasId)
                    _names.Add(app.Name.ToUpper());

            _allRoles = await _db.Roles
                .Include(r => r.AppRoles)
                .OrderBy(r => r.Name)
                .ToDictionaryAsync(r => r.AliasId);

            if (_item != null)
                foreach (var role in _allRoles)
                    if (role.Value.AppRoles!.Any(ar => ar.AppId == _item.AppId))
                        _appRoles.Add(role.Key, role.Value);

            foreach (var role in _appRoles)
                _allRoles.Remove(role.Key);

            StateHasChanged();
        }
        void CancelClicked() => Nav.NavigateTo(C.Routes.Apps);
        async Task SaveCreateClicked()
        {
            if (_create == null)
                return;

            _errors = _create.Validate(_t, _names);
            if (_errors != null)
                return;

            _item = new Entities.App(_create.Name!, _create.RedirectUri!);
            _item.AllowAllUsers = _create.AllowAllUsers;
            _item.Disabled = _create.Disabled ? DateTime.UtcNow : null;

            if (!string.IsNullOrWhiteSpace(_create.Secret))
                _item.SecretHash = Hasher.HashPassword(_create.Secret);

            _db.Apps.Add(_item);
            await _db.SaveChangesAsync();

            Nav.NavigateTo(C.Routes.AppsFor(_item.AliasId));
            _create = null;
            _edit = new(_item);
            StateHasChanged();
        }
        async Task SaveEditClicked()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t, _names);
            if (_errors != null)
                return;

            _item.Name = _edit.Name!;
            _item.RedirectUri = _edit.RedirectUri!;
            _item.AllowAllUsers = _edit.AllowAllUsers;

            if (!_item.Disabled.HasValue && _edit.Disabled)
                _item.Disabled = DateTime.UtcNow;
            else if (_item.Disabled.HasValue && !_edit.Disabled)
                _item.Disabled = null;

            if (!string.IsNullOrWhiteSpace(_edit.NewSecret))
                _item.SecretHash = Hasher.HashPassword(_edit.NewSecret);

            await _db.SaveChangesAsync();
        }
        void ToggleAllowAllUsers()
        {
            _edit!.AllowAllUsers = !_edit.AllowAllUsers;
            StateHasChanged();
        }
        void AddRole()
        {
            if (!_selectedRole.HasValue || !_allRoles.TryGetValue(_selectedRole.Value, out var role))
                return;

            role.AppRoles!.Add(new AppRole { AppId = _item!.AppId });

            _appRoles.Add(role.AliasId, role);
            _allRoles.Remove(role.AliasId);
            _selectedRole = null;

            StateHasChanged();
        }
        void RemoveRole(Guid roleAliasId)
        {
            if (!_appRoles.TryGetValue(roleAliasId, out var role))
                return;

            var appRole = role.AppRoles!.Single(ar => ar.AppId == _item!.AppId);
            role.AppRoles!.Remove(appRole);

            _allRoles.Add(role.AliasId, role);
            _appRoles.Remove(role.AliasId);

            StateHasChanged();
        }
    }
}