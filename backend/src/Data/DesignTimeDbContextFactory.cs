using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace COMP9034.Backend.Data
{
    // Add: Design-time factory to enable EF migrations without runtime env vars
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            // Use a safe placeholder connection string for design-time only
            var designTimeConnection =
                "Host=localhost;Port=5432;Database=farmtimems_design;Username=postgres;Password=postgres;SSL Mode=Disable";
            optionsBuilder.UseNpgsql(designTimeConnection);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

