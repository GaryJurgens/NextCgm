using Microsoft.EntityFrameworkCore;
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



    }
}
