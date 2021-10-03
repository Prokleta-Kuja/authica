using System;
using System.Collections.Generic;

namespace authica.Entities
{
    public class App
    {
        App()
        {
            Name = null!;
            RedirectUri = null!;
        }
        internal App(string name, string redirectUri)
        {
            AliasId = Guid.NewGuid();
            Name = name;
            RedirectUri = redirectUri;
            Created = DateTime.UtcNow;
        }

        public int AppId { get; set; }
        public Guid AliasId { get; set; }
        public string Name { get; set; }
        public string RedirectUri { get; set; }
        public string? SecretHash { get; set; }
        public bool AllowAllUsers { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Disabled { get; set; }

        public ICollection<AppRole>? AppRoles { get; set; }
    }
}