using System;
using System.Data;
using CcAcca.ApplicationInsights.ProblemDetails;
using CcAcca.LogDimensionCollection.AppInsights;
using CcAcca.LogDimensionCollection.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProblemDetailsDemo.Api.ExampleMiddleware;
using ProblemDetailsDemo.Api.MvcCustomizations;

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

      // Configure ProblemDetails middleware and tune ASP.Net Core MVC to line up with it's conventions
      services
        .AddProblemDetails(ConfigureProblemDetails)
        .AddControllersWithViews(o => {
          // optional tweak to built-in mvc http responses:
          o.Conventions.Add(new NotFoundResultApiConvention());
        })
        .AddProblemDetailsConventions();
      
      services
        .AddMvcActionDimensionCollection()
        .AddMvcResultItemsCountDimensionSelector()
        .AddMvcActionArgDimensionSelector()
        .ConfigureActionArgDimensionSelector(options => {
          // note: use with extreme caution and probably never for production workloads!
          options.AutoCollect = true;
        });

      services.AddSwaggerDocument(
        configure => {
          configure.PostProcess = document => {
            document.Info.Version = "v1";
            document.Info.Title = "ProblemDetailsDemo";
            document.Info.Description = "ASP.NET Core 3.1 Problem Details demo";
          };
        });

      services
        .AddApplicationInsightsTelemetry()
        // enrich request telemetry with ProblemDetails
        .AddProblemDetailTelemetryInitializer(o => {
          // only status codes >= 500 treat as a failure
          o.IsFailure = ProblemDetailsTelemetryOptions.ServerErrorIsFailure;
        })
        .AddMvcActionDimensionTelemetryInitializer();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
      app.UseIfElse(IsUIRequest, UIExceptionMiddleware, NonUIExceptionMiddleware);

      app.UseStaticFiles();

      app.UseRouting();

      app.UseOpenApi();
      app.UseSwaggerUi3();

      // demo a middleware throwing exceptions or setting StatusCode
      // try browsing to:
      // - middleware/error
      // - middleware/status/nnn (replacing nnn with a http status code eg 501)
      app.UseMiddleware<MaybeBadMiddleware>();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
        endpoints.MapControllerRoute("home", "", new {controller = "Home", action = "Index"});
        endpoints.MapControllerRoute("default", UIRoutePrefix + "/{controller=Bad}/{action=Index}/{id?}");
      });
    }

    private void ConfigureProblemDetails(ProblemDetailsOptions o)
    {
      o.ValidationProblemStatusCode = StatusCodes.Status400BadRequest;
      o.MapToStatusCode<DBConcurrencyException>(StatusCodes.Status409Conflict);
      o.MapToStatusCode<DBConcurrencyException>(StatusCodes.Status501NotImplemented);
    }

    private static void NonUIExceptionMiddleware(IApplicationBuilder app)
    {
      // Hellang.Middleware.ProblemDetails
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
      return requestPath == "/" ||
             requestPath.StartsWithSegments($"/{UIRoutePrefix}", StringComparison.OrdinalIgnoreCase);
    }
  }
}