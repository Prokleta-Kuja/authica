using System;
using System.Linq;
using System.Threading.Tasks;
using authica.Services;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace authica.Entities
{
    public partial class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
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
            Users.Add(adminUser);

            await SaveChangesAsync();
        }
    }
}