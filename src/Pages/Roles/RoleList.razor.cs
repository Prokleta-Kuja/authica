using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Roles
{
    public partial class RoleList
    {
        [Inject] private AppDbContext Db { get; set; } = null!;

        private List<Role> _roles = new();
        private readonly IRoles _t = LocalizationFactory.Roles();
        private readonly Formats _f = LocalizationFactory.Formats();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _roles = await Db.Roles.ToListAsync();
            StateHasChanged();
        }
    }
}