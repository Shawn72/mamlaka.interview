using System.Net;
using Microsoft.EntityFrameworkCore;

using Mamlaka.API.Exceptions;
using Mamlaka.API.DAL.DbContexts;

namespace Mamlaka.API.DAL.Entities.DataConfigurations;
public static class MigrationManager
{
    public static WebApplication MigrateDatabase(this WebApplication _webApplication)
    {
        using (IServiceScope scope = _webApplication.Services.CreateScope())
        {
            using (MySqlDbContext appContext = scope.ServiceProvider.GetRequiredService<MySqlDbContext>())
            {
                try
                {
                    appContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    throw new CustomException($"EF exception occured: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
                }
            }
        }
        return _webApplication;
    }
}
