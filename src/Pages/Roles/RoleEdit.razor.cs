using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Models;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Roles;

public partial class RoleEdit : IDisposable
{
    [Inject] public CurrentSession Session { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private AuthorizationStore AuthzStore { get; set; } = null!;
    [Parameter] public Guid AliasId { get; set; }
    private AppDbContext _db = null!;
    private RoleCreateModel? _create;
    private RoleEditModel? _edit;
    private Role? _item;
    private readonly HashSet<string> _names = new();
    private Guid? _selectedApp;
    private Guid? _selectedUser;
    private readonly Dictionary<Guid, Entities.App> _roleApps = new();
    private readonly Dictionary<Guid, User> _roleUsers = new();
    private Dictionary<Guid, Entities.App> _allApps = new();
    private Dictionary<Guid, User> _allUsers = new();
    private Dictionary<string, string>? _errors;
    private IRoles _t = LocalizationFactory.Roles();

    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
            Nav.NavigateTo(C.Routes.SignIn, true);
        else if (!Session.HasClaim(Claims.IsAdmin))
            Nav.NavigateTo(C.Routes.Forbidden);
        else
            _t = LocalizationFactory.Roles(Session.LocaleId);

        _db = await DbFactory.CreateDbContextAsync();

        base.OnInitialized();
    }
    public void Dispose() => _db?.Dispose();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var roles = await _db.Roles.ToListAsync();

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

        _allApps = await _db.Apps
            .Include(r => r.AppRoles)
            .OrderBy(r => r.Name)
            .ToDictionaryAsync(r => r.AliasId);

        if (_item != null)
            foreach (var app in _allApps)
                if (app.Value.AppRoles!.Any(ur => ur.RoleId == _item.RoleId))
                    _roleApps.Add(app.Key, app.Value);


        foreach (var app in _roleApps)
            _allApps.Remove(app.Key);

        _allUsers = await _db.Users
            .Include(r => r.UserRoles)
            .OrderBy(r => r.Email)
            .ToDictionaryAsync(r => r.AliasId);

        if (_item != null)
            foreach (var user in _allUsers)
                if (user.Value.UserRoles!.Any(ur => ur.RoleId == _item.RoleId))
                    _roleUsers.Add(user.Key, user.Value);

        foreach (var user in _roleUsers)
            _allUsers.Remove(user.Key);

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

        _db.Roles.Add(_item);
        await _db.SaveChangesAsync();

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

        await _db.SaveChangesAsync();

        AuthzStore.ClearAuthorizations();
    }
    void AddApp()
    {
        if (!_selectedApp.HasValue || !_allApps.TryGetValue(_selectedApp.Value, out var app))
            return;

        app.AppRoles!.Add(new AppRole { RoleId = _item!.RoleId });

        _roleApps.Add(app.AliasId, app);
        _allApps.Remove(app.AliasId);
        _selectedApp = null;

        StateHasChanged();
    }
    void RemoveApp(Guid roleAliasId)
    {
        if (!_roleApps.TryGetValue(roleAliasId, out var app))
            return;

        var appRole = app.AppRoles!.Single(ur => ur.RoleId == _item!.RoleId);
        app.AppRoles!.Remove(appRole);

        _allApps.Add(app.AliasId, app);
        _roleApps.Remove(app.AliasId);

        StateHasChanged();
    }
    void AddUser()
    {
        if (!_selectedUser.HasValue || !_allUsers.TryGetValue(_selectedUser.Value, out var user))
            return;

        user.UserRoles!.Add(new UserRole { RoleId = _item!.RoleId });

        _roleUsers.Add(user.AliasId, user);
        _allUsers.Remove(user.AliasId);
        _selectedUser = null;

        StateHasChanged();
    }
    void RemoveUser(Guid userAliasId)
    {
        if (!_roleUsers.TryGetValue(userAliasId, out var user))
            return;

        var userRole = user.UserRoles!.Single(ur => ur.RoleId == _item!.RoleId);
        user.UserRoles!.Remove(userRole);

        _allUsers.Add(user.AliasId, user);
        _roleUsers.Remove(user.AliasId);

        StateHasChanged();
    }
}