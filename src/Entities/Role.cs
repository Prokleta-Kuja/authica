using System;
using System.Collections.Generic;

namespace authica.Entities
{
    public class Role
    {
        Role()
        {
            Name = null!;
        }
        internal Role(string name)
        {
            Name = name;
            AliasId = Guid.NewGuid();
        }
        public int RoleId { get; set; }
        public Guid AliasId { get; set; }
        public string Name { get; set; }
        public DateTime? Disabled { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public virtual ICollection<AppRole> AppRoles { get; set; } = new HashSet<AppRole>();
    }
}