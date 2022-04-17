using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using authica.Services;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace authica.Entities;

public partial class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<AppRole> AppRoles { get; set; } = null!;
    public DbSet<App> Apps { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(e =>
        {
            e.HasKey(p => p.UserId);
            e.HasMany(p => p.UserRoles).WithOne(p => p.User!).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserRole>(e =>
        {
            e.HasKey(p => new { p.UserId, p.RoleId });
        });

        builder.Entity<Role>(e =>
        {
            e.HasKey(p => p.RoleId);
            e.HasMany(p => p.UserRoles).WithOne(p => p.Role!).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.AppRoles).WithOne(p => p.Role!).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AppRole>(e =>
        {
            e.HasKey(p => new { p.AppId, p.RoleId });
        });

        builder.Entity<App>(e =>
        {
            e.HasKey(p => p.AppId);
        });

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var dtProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in dtProperties)
                builder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new DateTimeToBinaryConverter());

            var decProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            foreach (var property in decProperties)
                builder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion<double>();
        }
    }
    public async ValueTask InitializeDefaults(IPasswordHasher hasher)
    {
        var adminUser = new User("admin@te.st", "admin", null, null) { IsAdmin = true };
        adminUser.SetPassword(C.Env.AdminPassword, hasher);
        adminUser.TimeZone = C.Env.TimeZone;
        adminUser.Locale = C.Env.Locale;
        Users.Add(adminUser);

        if (!Debugger.IsAttached)
        {
            await SaveChangesAsync();
            return;
        }

        var coworker1 = new User("co.worker1@te.st", "coworker1", "Co", "Worker1");
        coworker1.SetPassword(C.Env.AdminPassword, hasher);
        coworker1.TimeZone = C.Env.TimeZone;
        coworker1.Locale = C.Env.Locale;
        Users.Add(coworker1);

        var coworker2 = new User("co.worker2@te.st", "coworker2", "Co", "Worker2");
        coworker2.SetPassword(C.Env.AdminPassword, hasher);
        coworker2.TimeZone = C.Env.TimeZone;
        coworker2.Locale = C.Env.Locale;
        Users.Add(coworker2);

        var coworker3 = new User("co.worker3@te.st", "coworker3", "Co", "Worker3");
        coworker3.SetPassword(C.Env.AdminPassword, hasher);
        coworker3.TimeZone = C.Env.TimeZone;
        coworker3.Locale = C.Env.Locale;
        Users.Add(coworker3);

        var friend1 = new User("my-friend1@te.st", "friend1", "My", "Friend1");
        friend1.SetPassword(C.Env.AdminPassword, hasher);
        friend1.TimeZone = C.Env.TimeZone;
        friend1.Locale = C.Env.Locale;
        Users.Add(friend1);

        var friend2 = new User("my-friend2@te.st", "friend2", "My", "Friend2");
        friend2.SetPassword(C.Env.AdminPassword, hasher);
        friend2.TimeZone = C.Env.TimeZone;
        friend2.Locale = C.Env.Locale;
        Users.Add(friend2);

        var friend3 = new User("my-friend3@te.st", "friend3", "My", "Friend3");
        friend3.SetPassword(C.Env.AdminPassword, hasher);
        friend3.TimeZone = C.Env.TimeZone;
        friend3.Locale = C.Env.Locale;
        Users.Add(friend3);

        var family1 = new User("family.member1@te.st", "family1", "Family", "Member1");
        family1.SetPassword(C.Env.AdminPassword, hasher);
        family1.TimeZone = C.Env.TimeZone;
        family1.Locale = C.Env.Locale;
        Users.Add(family1);

        var family2 = new User("family.member2@te.st", "family2", "Family", "Member2");
        family2.SetPassword(C.Env.AdminPassword, hasher);
        family2.TimeZone = C.Env.TimeZone;
        family2.Locale = C.Env.Locale;
        Users.Add(family2);

        var family3 = new User("family.member3@te.st", "family3", "Family", "Member3");
        family3.SetPassword(C.Env.AdminPassword, hasher);
        family3.TimeZone = C.Env.TimeZone;
        family3.Locale = C.Env.Locale;
        Users.Add(family3);

        await SaveChangesAsync();

        var internalApp = new App("Internal App", "localhost:81");
        internalApp.SetPassword(C.Env.AdminPassword, hasher);
        internalApp.LdapEnabled = true;
        Apps.Add(internalApp);

        var externalApp = new App("External App", "localhost:82");
        externalApp.SetPassword(C.Env.AdminPassword, hasher);
        Apps.Add(externalApp);

        await SaveChangesAsync();

        var coworkers = new Role("CoWorkers");
        coworkers.UserRoles.Add(new() { User = coworker1 });
        coworkers.UserRoles.Add(new() { User = coworker2 });
        coworkers.UserRoles.Add(new() { User = coworker3 });
        coworkers.AppRoles.Add(new() { App = externalApp });
        Roles.Add(coworkers);

        var friends = new Role("Friends");
        friends.UserRoles.Add(new() { User = friend1 });
        friends.UserRoles.Add(new() { User = friend2 });
        friends.UserRoles.Add(new() { User = friend3 });
        friends.AppRoles.Add(new() { App = internalApp });
        friends.AppRoles.Add(new() { App = externalApp });
        Roles.Add(friends);

        var family = new Role("Family");
        family.UserRoles.Add(new() { User = family1 });
        family.UserRoles.Add(new() { User = family2 });
        family.UserRoles.Add(new() { User = family3 });
        family.AppRoles.Add(new() { App = internalApp });
        family.AppRoles.Add(new() { App = externalApp });
        Roles.Add(family);

        await SaveChangesAsync();
    }
}