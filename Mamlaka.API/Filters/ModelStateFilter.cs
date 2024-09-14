using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mamlaka.API.CommonObjects.Responses;

namespace Mamlaka.API.Filters;
public class ModelStateFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            ResponseObject<object> responseObject = new()
            {
                Status = new ResponseStatus
                {
                    Code = $"{(int)HttpStatusCode.BadRequest}",
                    Message = string.Join(" | ", context.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage))
                }
            };
            context.Result = new BadRequestObjectResult(responseObject);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }
}
