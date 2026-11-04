

namespace Software.Api.Infra;



public class MustHaveIfMatch : IEndpointFilter
{


    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // If-Match: this;or-that

        var ifMatch = context.HttpContext.Request.Headers.IfMatch.FirstOrDefault();

        if (ifMatch is null)
        {
            //context.HttpContext.Response.StatusCode = StatusCodes.Status428PreconditionRequired;
            return Results.StatusCode(428);
        }

         var result = await next(context);

        var x = 12;

        return result;
        // this would happen after the response.
    }
}

