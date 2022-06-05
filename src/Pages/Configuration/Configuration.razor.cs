using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Models;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace authica.Pages.Configuration;

public partial class Configuration : IDisposable
{
    [Inject] public CurrentSession Session { get; set; } = null!;
    [Inject] private ILogger<Configuration> Logger { get; set; } = null!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private MailService Mail { get; set; } = null!;
    [Inject] private GeolocationDbDownloadService GeolocationDbDownloadService { get; set; } = null!;
    [Inject] private ToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    private AppDbContext _db = null!;
    private Settings? _item;
    private SettingsEditModel? _edit;
    private string? _newCountryCode;
    private bool _invalidCountryCode;
    private Dictionary<string, string>? _errors;
    private IConfiguration _t = LocalizationFactory.Configuration();
    private Formats _f = LocalizationFactory.Formats();
    private string? TestEmail;

    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
            Nav.NavigateTo(C.Routes.SignIn, true);
        else if (!Session.HasClaim(Claims.IsAdmin))
            Nav.NavigateTo(C.Routes.Forbidden);
        else
        {
            _t = LocalizationFactory.Configuration(Session.LocaleId);
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

        _item = await C.Configuration.LoadFromDiskAsync();
        _edit = new(_item);

        StateHasChanged();
    }

    string LastGeoDownload()
    {
        if (C.GeoLocationDbFile.Exists)
            return _f.Display(C.GeoLocationDbFile.CreationTimeUtc);
        else
            return _t.MaxMindNotDownloaded;

    }
    async Task Save()
    {
        if (_edit == null || _item == null)
            return;

        _errors = _edit.Validate(_t);
        if (_errors != null)
            return;

        var shouldRestart = _item.Issuer != _edit.Issuer
            || _item.Domain != _edit.Domain
            || _item.MaxSessionDuration != _edit.MaxSessionDuration;

        _item = _edit.Convert();

        await C.Configuration.SaveToDiskAsync(_item);

        if (shouldRestart)
            ToastService.ShowWarning(_t.ToastRestartMessage, _t.ToastRestartAction, Restart);
        else
        {
            await C.Configuration.LoadFromDiskAsync();
            ToastService.ShowSuccess(_t.ToastSaved);
        }
    }

    void Restart()
    {
        Nav.NavigateTo(C.Routes.Restart, true);
        Program.Shutdown(true);
    }

    async Task SendTestEmail()
    {
        if (_edit == null || !_edit.IsMailSetup || string.IsNullOrWhiteSpace(TestEmail))
            return;

        try
        {
            await Mail.SendTestEmailAsync(TestEmail, _edit);
            ToastService.ShowSuccess(_t.ToastMailOk);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Send test email error");
            ToastService.ShowError(_t.ToastMailNok);
        }
    }
    public void AddCountryCode()
    {
        _invalidCountryCode = false;
        if (string.IsNullOrWhiteSpace(_newCountryCode))
            return;

        try
        {
            var info = new RegionInfo(_newCountryCode);
            _edit!.AllowedCountryCodes.Add(_newCountryCode.ToUpper());
            _newCountryCode = null;
        }
        catch (ArgumentException)
        {
            _invalidCountryCode = true;
        }

        StateHasChanged();
    }
    public void RemoveCountryCode(string code) => _edit!.AllowedCountryCodes.Remove(code);
    public async Task DownloadDb()
    {
        if (string.IsNullOrWhiteSpace(_edit!.MaxMindLicenseKey))
        {
            ToastService.ShowError(_t.ToastNoLicenseKey);
            return;
        }
        var ok = await GeolocationDbDownloadService.DownloadDb(_edit.MaxMindLicenseKey);
        if (ok)
            ToastService.ShowSuccess(_t.ToastDownloadOk);
        else
            ToastService.ShowError(_t.ToastDownloadNok);

        StateHasChanged();
    }
}