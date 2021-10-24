using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Users
{
    public partial class UserList
    {
        [Inject] public CurrentSession Session { get; set; } = null!;
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;

        private AppDbContext _db = null!;
        private List<User> _users = new();
        private IUsers _t = LocalizationFactory.Users();
        private Formats _f = LocalizationFactory.Formats();

        protected override void OnInitialized()
        {
            if (Session.IsAuthenticated)
            {
                _t = LocalizationFactory.Users(Session.LocaleId);
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

            _users = await _db.Users.ToListAsync();
            StateHasChanged();
        }
    }
}