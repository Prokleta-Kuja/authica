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
using Microsoft.Extensions.Logging;

namespace authica.Pages.MyProfile;

public partial class MyProfile : IDisposable
{
    [Inject] private CurrentSession Session { get; set; } = null!;
    [Inject] private ILogger<MyProfile> Logger { get; set; } = null!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private IPasswordHasher Hasher { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    private AppDbContext _db = null!;
    private User? _item;
    private MyProfileEditModel? _edit;
    private List<ApplicationTicket> _tickets = new();
    private Dictionary<string, string>? _errors;
    private IMyProfile _t = LocalizationFactory.MyProfile();
    private Formats _f = LocalizationFactory.Formats();
    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
            Nav.NavigateTo(C.Routes.SignIn, true);
        else
        {
            _t = LocalizationFactory.MyProfile(Session.LocaleId);
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

        _tickets = InMemoryTicketStore.GetUsersTickets(Session.UserAliasId);

        _item = await _db.Users.Where(u => u.AliasId == Session.UserAliasId).FirstOrDefaultAsync();
        if (_item != null)
            _edit = new(_item);

        StateHasChanged();
    }
    void RemoveSession(Guid sessionId)
    {
        InMemoryTicketStore.RemoveSession(sessionId.ToString());
        _tickets = InMemoryTicketStore.GetUsersTickets(Session.UserAliasId);
        StateHasChanged();
    }
    void CancelClicked() => Nav.NavigateTo(C.Routes.Root);
    async Task SaveEditClicked()
    {
        if (_edit == null || _item == null)
            return;

        var usernames = await _db.Users.AsNoTracking()
            .Where(u => u.UserId != _item.UserId)
            .Select(u => u.UserName)
            .ToListAsync();

        _errors = _edit.Validate(_t, usernames.ToHashSet(), C.Configuration.Current.MinPasswordLength, C.Configuration.Current.MaxPasswordLength);
        if (_errors != null)
            return;

        if (!string.IsNullOrWhiteSpace(_edit.OldPassword) && !string.IsNullOrWhiteSpace(_edit.NewPassword))
        {
            if (!_item.VerifyPassword(_edit.OldPassword, Hasher))
            {
                _errors = new() { { nameof(_edit.OldPassword), _t.ValidationInvalid } };
                return;
            }

            _item.SetPassword(_edit.NewPassword, Hasher);
        }

        _item.UserName = _edit.UserName!;
        _item.FirstName = _edit.FirstName;
        _item.LastName = _edit.LastName;
        _item.TimeZone = _edit.TimeZone;
        _item.Locale = _edit.Locale;

        await _db.SaveChangesAsync();

        Nav.NavigateTo(C.Routes.Root);
    }
}