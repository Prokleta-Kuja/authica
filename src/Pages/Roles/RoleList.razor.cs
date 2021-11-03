using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Roles
{
    public partial class RoleList
    {
        [Inject] public CurrentSession Session { get; set; } = null!;
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private AppDbContext _db = null!;
        private List<Role> _roles = new();
        private IRoles _t = LocalizationFactory.Roles();
        private Formats _f = LocalizationFactory.Formats();

        protected override async Task OnInitializedAsync()
        {
            if (Session.IsAuthenticated && Session.HasClaim(Claims.IsAdmin))
            {
                _t = LocalizationFactory.Roles(Session.LocaleId);
                _f = LocalizationFactory.Formats(Session.LocaleId, Session.TimeZoneId);
            }
            else
                Nav.NavigateTo(C.Routes.Forbidden);

            _db = await DbFactory.CreateDbContextAsync();

            base.OnInitialized();
        }
        public void Dispose() => _db?.Dispose();
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _roles = await _db.Roles.ToListAsync();
            StateHasChanged();
        }
    }
}