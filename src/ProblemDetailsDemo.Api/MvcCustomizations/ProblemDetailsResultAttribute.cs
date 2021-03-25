﻿using System;
using System.Linq;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProblemDetailsDemo.Api.ProblemDetailsConfig;

namespace ProblemDetailsDemo.Api.MvcCustomizations
{
    /// <summary>
    ///     Ensure <see cref="BadRequestResult" /> explicitly returned by a controller action
    ///     has the same shape as automatic HTTP 400 responses produced by the framework
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ProblemDetailsResultAttribute : Attribute, IAlwaysRunResultFilter
    {
        private static readonly string BadRequestType = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!(context.Result is BadRequestObjectResult badRequest)) return;

            if (badRequest.Value is SerializableError errors)
            {
                // make controller actions that do this:
                //   `return BadRequest(ModelState);`
                // as though they did this instead:
                //   `return BadRequest(new ValidationProblemDetails(ModelState));`

                var problemDetails = ToValidationProblemDetails(errors);
                SetBadResponseProblemDetails(context, problemDetails);
            }

            if (badRequest.Value is string detail)
            {
              // make controller actions that do this:
              //   `return BadRequest("Something bad");`
              // as though they did this instead:
              //   ```
              //   var badResponse = new StatusCodeProblemDetails(400) { Detail = "Something bad" };
              //   throw new ProblemDetailsException(badResponse);
              //   ```

              var problemDetails = new StatusCodeProblemDetails(400)
              {
                Type = BadRequestType,
                Detail = detail
              };
              SetBadResponseProblemDetails(context, problemDetails);
            }
        }

        private static void SetBadResponseProblemDetails(ResultExecutingContext context, ProblemDetails problemDetails)
        {
          // keep consistent with asp.net core 2.2+ conventions that adds a tracing value
          ProblemDetailsHelper.SetTraceId(problemDetails, context.HttpContext);
          context.Result = new BadRequestObjectResult(problemDetails);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        private static ValidationProblemDetails ToValidationProblemDetails(SerializableError serializableError)
        {
            var validationErrors = serializableError
                .Where(x => x.Value is string[])
                .ToDictionary(x => x.Key, x => x.Value as string[]);
            return new ValidationProblemDetails(validationErrors)
            {
              Type = BadRequestType
            };
        }
    }
}