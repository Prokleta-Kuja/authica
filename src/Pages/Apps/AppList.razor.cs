using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Apps;

public partial class AppList : IDisposable
{
    [Inject] public CurrentSession Session { get; set; } = null!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    private AppDbContext _db = null!;
    private List<Entities.App> _apps = new();
    private IApps _t = LocalizationFactory.Apps();
    private Formats _f = LocalizationFactory.Formats();

    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
            Nav.NavigateTo(C.Routes.SignIn, true);
        else if (!Session.HasClaim(Claims.IsAdmin))
            Nav.NavigateTo(C.Routes.Forbidden);
        else
        {
            _t = LocalizationFactory.Apps(Session.LocaleId);
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

        _apps = await _db.Apps.ToListAsync();
        StateHasChanged();
    }
}