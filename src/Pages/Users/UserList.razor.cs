using System.Collections.Generic;
using System.Threading.Tasks;
using authica.Entities;
using authica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Users
{
    public partial class UserList
    {
        [Inject] private AppDbContext Db { get; set; } = null!;

        private List<User> _users = new();
        private readonly IUsers _t = LocalizationFactory.Users();
        private readonly Formats _f = LocalizationFactory.Formats();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _users = await Db.Users.ToListAsync();
            StateHasChanged();
        }
    }
}