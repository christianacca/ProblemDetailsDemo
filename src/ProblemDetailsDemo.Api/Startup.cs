using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ProblemDetailsDemo.Api.ExampleMiddleware;

namespace ProblemDetailsDemo.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //  this is from Hellang.Middleware.ProblemDetails package
            services.AddProblemDetails(ConfigureProblemDetails);

            services.AddMvc();

            // this is optional but a good idea
            services.Configure<MvcJsonOptions>(options =>
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                // note: this is standard built-in functionality of asp.net core
                // note: in asp.net core 2.2 this will is what you will get by default (no need to add this behaviour as we're doing here)
                //       see: https://blogs.msdn.microsoft.com/webdev/2018/09/12/asp-net-core-2-2-0-preview2-now-available/#problem-details-support

                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        // note: optionally set this to a url that would allow the end user (?) to view more details about input validation error
                        Type = "about:blank",
                        Detail = "Please refer to the errors property for additional details."
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = {"application/problem+json", "application/problem+xml"}
                    };
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //  this is from Hellang.Middleware.ProblemDetails package
            app.UseProblemDetails();

            // demo a middleware throwing exceptions or setting StatusCode
            // try browsing to:
            // - middleware/error
            // - middleware/status/nnn (replacing nnn with a http status code eg 501)
            app.UseMiddleware<MaybeBadMiddleware>();

            app.UseMvc();
        }

        private void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            // This is the default behavior; only include exception details in a development environment.
            options.IncludeExceptionDetails = ctx => Environment.IsDevelopment();

            // This will map NotImplementedException to the 501 Not Implemented status code.
            options.Map<NotImplementedException>(ex =>
                new ExceptionProblemDetails(ex, StatusCodes.Status501NotImplemented)
                {
                    Instance = $"urn:mri:error:{Guid.NewGuid()}"
                });

            // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
            // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
            options.Map<Exception>(ex =>
                new ExceptionProblemDetails(ex, StatusCodes.Status500InternalServerError)
                {
                    Instance = $"urn:mri:error:{Guid.NewGuid()}"
                });
        }
    }
}