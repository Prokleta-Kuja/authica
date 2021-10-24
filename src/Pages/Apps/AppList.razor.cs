using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Apps
{
    public partial class AppList : IDisposable
    {
        [Inject] public CurrentSession Session { get; set; } = null!;
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;

        private AppDbContext _db = null!;
        private List<Entities.App> _apps = new();
        private IApps _t = LocalizationFactory.Apps();
        private Formats _f = LocalizationFactory.Formats();

        protected override void OnInitialized()
        {
            if (Session.IsAuthenticated)
            {
                _t = LocalizationFactory.Apps(Session.LocaleId);
                _f = LocalizationFactory.Formats(Session.LocaleId, Session.TimeZoneId);
            }
            _db = DbFactory.CreateDbContext();

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
}