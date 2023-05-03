using ASP_JWT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASP_JWT.Data
{
    public class DataContext:IdentityDbContext<IdentityUser>
    {
        public DataContext(DbContextOptions<DataContext> options) :base(options)
        {
         
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        DbSet<Customer> Customers { get; set; }

    }
}
