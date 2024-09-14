using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Mamlaka.API.CommonObjects.Responses;

namespace Mamlaka.API.Swagger;
public class VersionErrorProvider : IErrorResponseProvider
{
    public IActionResult CreateResponse(ErrorResponseContext context)
    {

        ResponseObject<object> responseObject = new()
        {
            Status = new ResponseStatus
            {
                Code = $"{context.StatusCode}",
                Message = $"{context.ErrorCode} - {context.Message}"
            }
        };

        return new BadRequestObjectResult(responseObject);
    }
}
