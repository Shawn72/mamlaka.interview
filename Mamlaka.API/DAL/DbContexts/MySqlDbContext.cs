using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Mamlaka.API.DAL.Entities;
using Mamlaka.API.DAL.Entities.Transactions;
using Mamlaka.API.DAL.Entities.DataConfigurations;

namespace Mamlaka.API.DAL.DbContexts;
public class MySqlDbContext : IdentityDbContext<User>
{
    public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = true;
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //initial user roles set up in the database
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration()); //comment after first migration
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// entity object definitions
    /// </summary>
    public DbSet<User> AspNetUsers { get; set; }

    public DbSet<Transaction> Transactions { get; set; }


}
