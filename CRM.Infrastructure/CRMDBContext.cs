using BuildingBlock.Infrastracture.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure
{
    public class CRMDBContext : DbContext
    {
        public CRMDBContext(DbContextOptions<CRMDBContext> options)
: base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplySoftDeleteQueryFilter();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CRMDBContext).Assembly);
        }
    }
}