using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Users;

public partial class UserList
{
    [Inject] public CurrentSession Session { get; set; } = null!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    private AppDbContext _db = null!;
    private List<User> _users = new();
    private IUsers _t = LocalizationFactory.Users();
    private Formats _f = LocalizationFactory.Formats();

    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
            Nav.NavigateTo(C.Routes.SignIn, true);
        else if (!Session.HasClaim(Claims.IsAdmin))
            Nav.NavigateTo(C.Routes.Forbidden);
        else
        {
            _t = LocalizationFactory.Users(Session.LocaleId);
            _f = LocalizationFactory.Formats(Session.LocaleId, Session.TimeZoneId);
        }

        _db = await DbFactory.CreateDbContextAsync();

        base.OnInitialized();
    }
    public void Dispose() => _db?.Dispose();
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _users = await _db.Users.ToListAsync();
        StateHasChanged();
    }
}