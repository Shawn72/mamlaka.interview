using System.Net;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Redis.OM;
using AutoMapper;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

using Mamlaka.API.Swagger;
using Mamlaka.API.Filters;
using Mamlaka.API.DAL.Enums;
using Mamlaka.API.Exceptions;
using Mamlaka.API.Attributes;
using Mamlaka.API.Interfaces;
using Mamlaka.API.Repositories;
using Mamlaka.API.DAL.Entities;
using Mamlaka.API.DAL.Constants;
using Mamlaka.API.DAL.DbContexts;
using Mamlaka.API.Services.HttpService;
using Mamlaka.API.Services.HostedService;
using Mamlaka.API.DAL.Entities.Transactions;
using Mamlaka.API.DAL.Entities.DataConfigurations;
using IHttpClientFactory = Mamlaka.API.Services.HttpService.IHttpClientFactory;
using Mamlaka.API.Services.GatewayService;

namespace Mamlaka.API.Configs;
public class Startup
{
    /// <summary>
    /// service configuration
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        Constants _constants = new Constants(configuration);

        string connectionString = _constants.SqlConnectionString(configuration);

        services.AddDbContextPool<MySqlDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    options => options.EnableRetryOnFailure(
                            maxRetryCount: 50,
                            maxRetryDelay: TimeSpan.FromSeconds(20),
                            errorNumbersToAdd: null));
        });


        services.AddIdentity();
        services.AddVersioning();
        services.AddCustomSwagger();
        services.AddCustomControllers();
        services.AddAuthentication(configuration);
        services.AddHostedService<HostedService>();
        services.AddSingleton<PaypalGatewayService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRedisRepository, RedisRepository>();
        services.AddScoped<IHttpClientFactory, HttpClientFactory>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionPagination, PagedTransactionRepository<Transaction>>();
        services.AddSingleton(new RedisConnectionProvider(configuration.GetConnectionString("Redis_Connection_String")));

        MapperConfiguration mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new AutoMapperProfile());
        });

        IMapper mapper = mappingConfig.CreateMapper();
        services.AddSingleton(mapper);
    }

    /// <summary>
    /// app configuration
    /// </summary>
    /// <param name="app"></param>
    public static void ConfigureApp(WebApplication app)
    {
        //for NginX reverse proxy
        ForwardedHeadersOptions fordwardedHeaderOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        fordwardedHeaderOptions.KnownNetworks.Clear();
        fordwardedHeaderOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(fordwardedHeaderOptions);

        app.UseCustomSwagger();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseStaticFiles();

        app.UseCors("CorsPolicy");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MigrateDatabase(); //uncomment when doing first db migration

        app.UseMiddleware<ExceptionLoggingMiddleware>();

        _ = app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                context.Response.ContentType = "text/plain";

                // Host info
                var name = Dns.GetHostName(); // get container id
                var ip = Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

                Console.WriteLine($"Mamlaka API: {Environment.MachineName} \t {name}\t {ip}");
                await context.Response.WriteAsync($"Mamlaka Hostname: {Environment.MachineName}{Environment.NewLine}");

                await context.Response.WriteAsync(Environment.NewLine);

                // Connection: RemoteIp
                await context.Response.WriteAsync($"Remote IP: {context.Connection.RemoteIpAddress}");
            });
        });
    }
}
/// <summary>
/// extension methods
/// </summary>
public static class ConfigurationExtensionMethods
{
    /// <summary>
    /// identity configs
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 0;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1d);
            options.Lockout.MaxFailedAccessAttempts = 6;
        }).AddEntityFrameworkStores<MySqlDbContext>().AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// authentication configs
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        //Add Token Validation Parameters
        TokenValidationParameters tokenParameters = new()
        {
            //what to validate
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Security:Issuer"],
            ValidAudience = configuration["Security:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Security:Key"])),
            ClockSkew = new TimeSpan(0)
        };

        //Add JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenParameters;
        });

        services.AddAuthorization(auth =>
        {
            auth.AddPolicy(nameof(AuthPolicy.Bearer), new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
            .RequireAuthenticatedUser().Build());

            auth.AddPolicy(
                nameof(AuthPolicy.GlobalRights),
                policy => policy.RequireRole(
                    nameof(Roles.SuperAdmin),
                    nameof(Roles.Admin),
                    nameof(Roles.Customer)
              ));

            auth.AddPolicy(
                nameof(AuthPolicy.SuperRights),
                policy => policy.RequireRole(nameof(Roles.SuperAdmin)));

            //add other policies down here as you wish...
        });

        return services;
    }

    /// <summary>
    /// custom controller configs
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add(typeof(ModelStateFilter));
            options.Filters.Add(typeof(ExceptionFilter));
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        //Configure CORS: to solve CORS errors on Front-End e.g. React Js , Angular etc.
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });

        return services;
    }

    /// <summary>
    /// api swagger versioning
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // specify the default API Version as 1.0
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // if the client hasn't specified the API version in the request, use the default API version number 
            options.AssumeDefaultVersionWhenUnspecified = true;

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            options.ReportApiVersions = true;

            options.ApiVersionReader = new UrlSegmentApiVersionReader();

            options.ErrorResponses = new VersionErrorProvider();
        });

        services.AddVersionedApiExplorer(options =>
        {
            // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            options.GroupNameFormat = "'v'VVV";

            // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
            // can also be used to control the format of the API version in route templates
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// custom swagger configs
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            // add a custom operation filter which sets default values
            options.OperationFilter<SwaggerDefaultValues>();

            //integrate xml comments
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            options.OrderActionsBy(description =>
            {
                ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)description.ActionDescriptor;
                SwaggerOrderAttribute? attribute = (SwaggerOrderAttribute?)controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute(typeof(SwaggerOrderAttribute));
                return string.IsNullOrEmpty(attribute?.Order?.Trim()) ? description.GroupName : attribute.Order.Trim();
            });

            //define how the API is secured by defining one or more security schemes.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter in the value field: <b>Bearer {your JWT token}</b>"
            });

            //Operation security scheme based on Authorize attribute using OperationFilter()
            options.OperationFilter<SwaggerAuthOperationFilter>();
        });

        return services;
    }

    /// <summary>
    /// custom swagger 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseCustomSwagger(this WebApplication app)
    {
        // Enable middleware to serve generated Swagger as a JSON endpoint.  
        app.UseSwagger();

        IApiVersionDescriptionProvider apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        //Enable middleware to serve swagger - ui(HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
        app.UseSwaggerUI(options =>
        {
            // build a swagger endpoint for each discovered API version
            foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"BCHelperApi: {description.GroupName.ToUpperInvariant()}");
            }
            options.DocExpansion(DocExpansion.None);
        });

        return app;
    }

    /// <summary>
    /// json strings extension methods
    /// </summary>
    public class EmptyStringToNullConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            return value == string.Empty ? null : value?.Trim();
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}

