using System.IO;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace authica
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            InitializeDirectories();
            await InitializeDb(args);
            await C.Configuration.LoadAsync();
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        static void InitializeDirectories()
        {
            var appdata = new DirectoryInfo(C.Paths.AppData);
            appdata.Create();
        }

        static async Task InitializeDb(string[] args)
        {
            var dbFile = new FileInfo(C.Paths.AppDataFor("app.db"));

            var opt = new DbContextOptionsBuilder<AppDbContext>();
            opt.UseSqlite(C.Paths.AppDbConnectionString);

            var db = new AppDbContext(opt.Options);
            if (db.Database.GetMigrations().Any())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();

            // Seed
            if (!db.Users.Any())
            {
                await db.InitializeDefaults(new PasswordHashingService());
            }
        }
    }
}