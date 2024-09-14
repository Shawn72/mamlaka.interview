using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mamlaka.API.DAL.DbContexts;
    /// <summary>
    /// dbcontex configs for migrations
    /// </summary>
public class DbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
{
    /// <summary>
    /// Db creating context
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public MySqlDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<MySqlDbContext> optionsBuilder = new DbContextOptionsBuilder<MySqlDbContext>();

        string dbServer = "your-ip-address";
        string dbName = "your-db-name";
        string dbPort =  "your-db-port";
        string dbUserId = "your-db-userid";
        string dbPassword = "your-db-password";

        string DbConnection = $"Data Source={dbServer}; Database={dbName}; Port={dbPort}; User Id={dbUserId}; Password={dbPassword}";

        optionsBuilder.UseMySql(DbConnection, ServerVersion.AutoDetect(DbConnection),
                   options => options.EnableRetryOnFailure(
                           maxRetryCount: 10,
                           maxRetryDelay: TimeSpan.FromSeconds(30),
                           errorNumbersToAdd: null));

        return new MySqlDbContext(optionsBuilder.Options);
    }

    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddEntityFrameworkSqlServer();
        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAddCoreServices();
    }
}