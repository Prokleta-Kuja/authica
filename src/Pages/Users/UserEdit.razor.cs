using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Models;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace authica.Pages.Users
{
    public partial class UserEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IPasswordHasher Hasher { get; set; } = null!;
        [Parameter] public Guid AliasId { get; set; }
        private UserCreateModel? _create;
        private UserEditModel? _edit;
        private User? _item;
        private HashSet<string> _names = new();
        private HashSet<string> _emails = new();
        private Dictionary<string, string>? _errors;
        private readonly IUsers _t = LocalizationFactory.Users();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var users = await Db.Users.ToListAsync();

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

            Db.Users.Add(_item);
            await Db.SaveChangesAsync();

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

            await Db.SaveChangesAsync();
            Nav.NavigateTo(C.Routes.Users);
        }
    }
}