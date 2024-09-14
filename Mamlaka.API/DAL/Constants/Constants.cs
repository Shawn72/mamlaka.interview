namespace Mamlaka.API.DAL.Constants;
public class Constants
{
    public string _defaultGuid;
    public string _defaultActor;
    public DateTime _sqlMinimumDate;
    public DateTime _sqlMaximumDate;
    private readonly IConfiguration _configuration;
    public Constants(IConfiguration configuration)
    {
        _configuration = configuration;
        _defaultGuid = _configuration["StaticStrings:DefaultGuid"];
        _defaultActor = _configuration["StaticStrings:DefaultActor"];
        _sqlMinimumDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
        _sqlMaximumDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;
    }

    public string SqlConnectionString(IConfiguration configuration)
    {
        string dbServer =  configuration["DatabaseSpecs:DataSource"];
        string dbName = configuration["DatabaseSpecs:Database"];
        string dbPort = configuration["DatabaseSpecs:Port"];
        string dbUserId = configuration["DatabaseSpecs:UserId"];
        string dbPassword = configuration["DatabaseSpecs:Password"];

       return string.Format(configuration["ConnectionStrings:DbConnectionString"], dbServer, dbName, dbPort, dbUserId, dbPassword);
    }
}
