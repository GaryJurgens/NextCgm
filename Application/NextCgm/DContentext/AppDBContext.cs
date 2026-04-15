using Microsoft.EntityFrameworkCore;
using NextCgm.DataEntities;
using NextCgm.DataEntities.Containers;
using NextCgm.DataEntities.Locations;
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

        public DbSet<TimeZones> TimeZones { get; set; }

        public DbSet<DockerContainers> DockerContainers { get; set; }

        public DbSet<DockerLogger> DockerLogger { get; set; }

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