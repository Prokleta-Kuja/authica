using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Entities;
using authica.Models;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace authica.Pages.Configuration
{
    public partial class Configuration : IDisposable
    {
        [Inject] private ILogger<Configuration> Logger { get; set; } = null!;
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
        [Inject] private MailService Mail { get; set; } = null!;
        private AppDbContext _db = null!;
        private Settings? _item;
        private SettingsEditModel? _edit;
        private Dictionary<string, string>? _errors;
        private readonly IConfiguration _t = LocalizationFactory.Configuration();
        private string? TestEmail;

        protected override void OnInitialized() { _db = DbFactory.CreateDbContext(); base.OnInitialized(); }
        public void Dispose() => _db?.Dispose();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _item = await C.Configuration.LoadFromDiskAsync();
            _edit = new(_item);

            StateHasChanged();
        }

        async Task Save()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t);
            if (_errors != null)
                return;

            _item = _edit.Convert();

            await C.Configuration.SaveToDiskAsync(_item);
        }

        async Task SendTestEmail()
        {
            if (_edit == null || !_edit.IsMailSetup || string.IsNullOrWhiteSpace(TestEmail))
                return;

            try
            {
                await Mail.SendTestEmailAsync(TestEmail, _edit);
                // TODO: Notification
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Send test email error");
                // TODO: notification
            }
        }
    }
}