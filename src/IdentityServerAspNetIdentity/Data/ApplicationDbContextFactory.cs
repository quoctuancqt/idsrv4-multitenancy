using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServerAspNetIdentity.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Server=localhost; User Id=postgres; Database=AspNetIdentity; Port=5432; Password=postgres;");

            var dummyTenant = new TenantInfo();
            return new ApplicationDbContext(dummyTenant, optionsBuilder.Options);
        }
    }
}