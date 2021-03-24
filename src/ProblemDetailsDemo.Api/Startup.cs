using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProblemDetailsDemo.Api.ExampleMiddleware;
using ProblemDetailsDemo.Api.MvcCustomizations;
using ProblemDetailsDemo.Api.ProblemDetailsConfig;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace ProblemDetailsDemo.Api
{
    public class Startup
    {
        private const string UIRoutePrefix = "ui";

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        private IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.ConfigureOptions<ProblemDetailsOptionsCustomSetup>();
            // Hellang.Middleware.ProblemDetails package
            services.AddProblemDetails();

            services
                .AddControllersWithViews(o =>
                {
                    // optional tweaks to built-in mvc non-success http responses
                    o.Conventions.Add(new NotFoundResultApiConvention());
                    o.Conventions.Add(new ProblemDetailsResultApiConvention());
                    // todo: switch to endpoint routing
                    o.EnableEndpointRouting = false;
                });

            services.AddSwaggerDocument(
                configure =>
                {
                    configure.PostProcess = (document) =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "ProblemDetailsDemo";
                        document.Info.Description = "ASP.NET Core 3.1 Problem Details demo";
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseIfElse(IsUIRequest, UIExceptionMiddleware, NonUIExceptionMiddleware);

            app.UseStaticFiles();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            // demo a middleware throwing exceptions or setting StatusCode
            // try browsing to:
            // - middleware/error
            // - middleware/status/nnn (replacing nnn with a http status code eg 501)
            app.UseMiddleware<MaybeBadMiddleware>();

            // todo: switch to endpoint routing
            app.UseMvc(routes =>
            {
                routes.MapRoute("home", "", new { controller = "Home", action = "Index"});
                routes.MapRoute("default", UIRoutePrefix + "/{controller}/{action=Index}/{id?}");
            });
        }

        private static void NonUIExceptionMiddleware(IApplicationBuilder app)
        {
            // Hellang.Middleware.ProblemDetails package
            app.UseProblemDetails();
        }

        private void UIExceptionMiddleware(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/ui/home/error");
            }

            app.UseStatusCodePages();
        }

        private static bool IsUIRequest(HttpContext httpContext)
        {
            var requestPath = httpContext.Request.Path;
            return requestPath == "/" || requestPath.StartsWithSegments($"/{UIRoutePrefix}", StringComparison.OrdinalIgnoreCase);
        }
    }
}