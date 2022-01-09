using System;
using System.Collections.Generic;

namespace authica.Entities
{
    public class App
    {
        App()
        {
            Name = null!;
            AuthorityUri = null!;
        }
        internal App(string name, string authorityUri)
        {
            AliasId = Guid.NewGuid();
            Name = name;
            AuthorityUri = authorityUri;
            Created = DateTime.UtcNow;
        }
        public int AppId { get; set; }
        public Guid AliasId { get; set; }
        public string Name { get; set; }
        public string AuthorityUri { get; set; }
        public string? RedirectUri { get; set; }
        public string? SecretHash { get; set; }
        public bool AllowAllUsers { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Disabled { get; set; }

        public virtual ICollection<AppRole> AppRoles { get; set; } = new HashSet<AppRole>();
    }
}