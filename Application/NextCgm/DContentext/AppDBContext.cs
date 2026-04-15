using Microsoft.EntityFrameworkCore;
using NextCgm.DataEntities;
using NextCgm.DataEntities.Containers;
using NextCgm.DataEntities.Locations;
using NextCgm.DataEntities.Nginx;
using NextCgm.DataEntities.User;

namespace NextCgm.DContentext
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> UserEntities { get; set; }
        public DbSet<CountryList> CountryLists { get; set; }

        public DbSet<ProvinceStateList> ProvinceStateLists { get; set; }

        public DbSet<TimeZoneData> TimeZoneData { get; set; }

        public DbSet<DockerContainers> DockerContainers { get; set; }

        public DbSet<DockerLogger> DockerLogger { get; set; }

        public DbSet<NginxContainer> NginxContainers { get; set; }

        public DbSet<NginxSyncLog> NginxSyncLogs { get; set; }

        public DbSet<NginxRoutingRule> NginxRoutingRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.UserSubDomain)
                .IsUnique();

            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.ApiKeyForNightScout)
                .IsUnique();
        }
    }
}