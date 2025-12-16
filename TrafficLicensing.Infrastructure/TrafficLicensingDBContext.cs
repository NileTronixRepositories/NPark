using BuildingBlock.Infrastracture.Extensions;
using Microsoft.EntityFrameworkCore;

namespace TrafficLicensing.Infrastructure
{
    public class TrafficLicensingDBContext : DbContext
    {
        public TrafficLicensingDBContext(DbContextOptions<TrafficLicensingDBContext> options)
: base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplySoftDeleteQueryFilter();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrafficLicensingDBContext).Assembly);
        }
    }
}