using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Moneteer.Landing.Repositories
{
    public class DataProtectionKeysContext : DbContext, IDataProtectionKeyContext
    {
        public DataProtectionKeysContext(DbContextOptions<DataProtectionKeysContext> options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DataProtectionKey>().ToTable("keys", "data_protection");
            builder.Entity<DataProtectionKey>().Property(p => p.Id).HasColumnName("id");
            builder.Entity<DataProtectionKey>().Property(p => p.FriendlyName).HasColumnName("friendly_name");
            builder.Entity<DataProtectionKey>().Property(p => p.Xml).HasColumnName("xml");
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}