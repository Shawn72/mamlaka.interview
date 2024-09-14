using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Mamlaka.API.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{

    readonly IApiVersionDescriptionProvider provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        // add a swagger document for each discovered API version
        // note: you might choose to skip or document deprecated API versions differently
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        StringBuilder text = new("Mamlaka REST APIs. These api performs back end services for library management system");

        OpenApiInfo info = new()
        {
            Title = "Mamlaka RESTful APIs",
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact
            {
                Name = "Shawn Mbuvi",
                Email = "shawnmbuvi@gmail.com",
                Url = new Uri("https://github.com/Shawn72/")
            }
        };
        info.Description = text.ToString();

        if (description.IsDeprecated)
        {
            text.Append("This API version has been deprecated.");
        }
        return info;
    }

}
