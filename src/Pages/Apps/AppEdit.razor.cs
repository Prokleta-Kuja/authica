using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Extensions;
using authica.Models;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace authica.Pages.Apps
{
    public partial class AppEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IPasswordHasher Hasher { get; set; } = null!;
        [Parameter] public Guid AliasId { get; set; }
        private AppCreateModel? _create;
        private AppEditModel? _edit;
        private Entities.App? _item;
        private HashSet<string> _names = new();
        private Dictionary<string, string>? _errors;
        private readonly IApps _t = LocalizationFactory.Apps();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var apps = await Db.Apps.ToListAsync();

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

            Db.Apps.Add(_item);
            await Db.SaveChangesAsync();

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

            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
    }
}