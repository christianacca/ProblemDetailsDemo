using System.Data;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ProblemDetailsDemo.Api.ProblemDetailsConfig
{
    public class ProblemDetailsOptionsCustomSetup : IConfigureOptions<ProblemDetailsOptions>
    {
        public ProblemDetailsOptionsCustomSetup(IOptions<ApiBehaviorOptions> apiOptions)
        {
          ApiOptions = apiOptions.Value;
        }

        private ApiBehaviorOptions ApiOptions { get; }

        public void Configure(ProblemDetailsOptions options)
        {
            options.MapStatusCode = MapStatusCode;

            // This will map DBConcurrencyException to the 409 Conflict status code.
            options.MapToStatusCode<DBConcurrencyException>(StatusCodes.Status409Conflict);

            // This will map NotImplementedException to the 501 Not Implemented status code.
            options.MapToStatusCode<DBConcurrencyException>(StatusCodes.Status501NotImplemented);
        }

        private ProblemDetails MapStatusCode(HttpContext context)
        {
            if (!ApiOptions.SuppressMapClientErrors &&
                ApiOptions.ClientErrorMapping.TryGetValue(context.Response.StatusCode, out var errorData))
            {
                // prefer the built-in MVC client error mapping in asp.net core
                return new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = errorData.Title,
                    Type = errorData.Link
                };
            }
            else
            {
                // use Hellang.Middleware.ProblemDetails mapping
                return StatusCodeProblemDetails.Create(context.Response.StatusCode);
            }
        }
    }
}