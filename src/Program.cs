using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace authica;

public class Program
{
    static readonly string s_adminResetFile = C.Paths.AppDataFor("ADMIN");
    static readonly string s_domainResetFile = C.Paths.AppDataFor("DOMAIN");
    static bool s_shouldStart = true;
    static IHost s_instance = null!;
    public static async Task Main(string[] args)
    {
        InitializeDirectories();
        await InitializeDb();
        await C.Configuration.LoadAsync();
        await ResetAdmin();
        await ResetDomain();

        while (s_shouldStart)
        {
            s_shouldStart = false;
            s_instance = CreateHostBuilder(args).Build();
            s_instance.Run();
        }
    }
    public static void Shutdown(bool restart = false)
    {
        s_shouldStart = restart;
        s_instance.StopAsync();
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

    static async Task InitializeDb()
    {
        using var db = GetDb();
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
    static async Task ResetAdmin()
    {
        if (!File.Exists(s_adminResetFile))
            return;

        var adminLines = await File.ReadAllLinesAsync(s_adminResetFile);
        if (adminLines.Length < 2)
            throw new Exception("Cannot set admin user and password, expected username on first line, and password in the second");

        var username = adminLines[0]; var password = adminLines[1];
        if (string.IsNullOrWhiteSpace(username))
            throw new Exception("Cannot set empty username");

        if (string.IsNullOrWhiteSpace(password))
            throw new Exception("Cannot set empty password");

        using var db = GetDb();
        var userNameNormalized = C.Normalize(username);
        var admin = await db.Users.SingleOrDefaultAsync(u => u.UserNameNormalized == userNameNormalized);
        var pwHasher = new PasswordHashingService();

        admin ??= new User($"{userNameNormalized}@example.com", username);
        admin.SetPassword(password, pwHasher);
        admin.IsAdmin = true;

        await db.SaveChangesAsync();
        File.Delete(s_adminResetFile);
        Console.WriteLine($"Admin user info for {username} reset");
    }
    static async Task ResetDomain()
    {
        if (!File.Exists(s_domainResetFile))
            return;

        var domain = await File.ReadAllTextAsync(s_domainResetFile);
        if (string.IsNullOrWhiteSpace(domain))
            throw new Exception("Cannot set blank domain");

        C.Configuration.Current.Domain = domain;
        await C.Configuration.SaveToDiskAsync();

        File.Delete(s_domainResetFile);
        Console.WriteLine($"Domain changed to {domain}");
    }
    static AppDbContext GetDb()
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>();
        opt.UseSqlite(C.Paths.AppDbConnectionString);

        return new AppDbContext(opt.Options);
    }
}