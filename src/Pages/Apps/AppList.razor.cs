using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Apps
{
    public partial class AppList
    {
        [Inject] private AppDbContext Db { get; set; } = null!;

        private List<Entities.App> _apps = new();
        private readonly IApps _t = LocalizationFactory.Apps();
        private readonly Formats _f = LocalizationFactory.Formats();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _apps = await Db.Apps.ToListAsync();
            StateHasChanged();
        }
    }
}