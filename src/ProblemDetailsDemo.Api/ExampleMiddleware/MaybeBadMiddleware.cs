using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProblemDetailsDemo.Api.ExampleMiddleware
{
    public class MaybeBadMiddleware
    {
        private readonly RequestDelegate _next;

        public MaybeBadMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/middleware", out _, out var remaining))
            {
                if (remaining.StartsWithSegments("/error"))
                    throw new DBConcurrencyException("This is an exception thrown from middleware.");

                if (remaining.StartsWithSegments("/status", out _, out remaining))
                {
                    var statusCodeString = remaining.Value.Trim('/');

                    if (int.TryParse(statusCodeString, out var statusCode))
                    {
                        context.Response.StatusCode = statusCode;
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}