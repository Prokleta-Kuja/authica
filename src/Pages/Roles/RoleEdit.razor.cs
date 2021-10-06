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

namespace authica.Pages.Roles
{
    public partial class RoleEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IPasswordHasher Hasher { get; set; } = null!;
        [Parameter] public Guid AliasId { get; set; }
        private RoleCreateModel? _create;
        private RoleEditModel? _edit;
        private Role? _item;
        private HashSet<string> _names = new();
        private Dictionary<string, string>? _errors;
        private readonly IRoles _t = LocalizationFactory.Roles();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var roles = await Db.Roles.ToListAsync();

            if (AliasId == Guid.Empty)
                _create = new();
            else
            {
                _item = roles.FirstOrDefault(a => a.AliasId == AliasId);

                if (_item != null)
                    _edit = new(_item);
            }

            foreach (var role in roles)
                if (role.AliasId != AliasId)
                    _names.Add(role.Name.ToUpper());

            StateHasChanged();
        }
        void CancelClicked() => Nav.NavigateTo(C.Routes.Roles);
        async Task SaveCreateClicked()
        {
            if (_create == null)
                return;

            _errors = _create.Validate(_t, _names);
            if (_errors != null)
                return;

            _item = new(_create.Name!);
            _item.Disabled = _create.Disabled ? DateTime.UtcNow : null;

            Db.Roles.Add(_item);
            await Db.SaveChangesAsync();

            Nav.NavigateTo(C.Routes.RolesFor(_item.AliasId));
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

            if (!_item.Disabled.HasValue && _edit.Disabled)
                _item.Disabled = DateTime.UtcNow;
            else if (_item.Disabled.HasValue && !_edit.Disabled)
                _item.Disabled = null;

            await Db.SaveChangesAsync();
        }
    }
}