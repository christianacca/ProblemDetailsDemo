using System.Data;
using CcAcca.ApplicationInsights.ProblemDetails;
using CcAcca.LogDimensionCollection.AppInsights;
using CcAcca.LogDimensionCollection.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsDemo.Api.ExampleMiddleware;
using ProblemDetailsDemo.Api.MvcCustomizations;

// ReSharper disable VariableHidesOuterVariable

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

const string uiRoutePrefix = "ui";

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Normally set by DOTNET_ENVIRONMENT env var; we set it explicitly to Development for demo purposes so as
    // force ProblemDetails middleware to always add exception details the responses.
    // YOUR APP should remove this line so that ProblemDetails middleware only adds exception details to responses
    // when running locally where DOTNET_ENVIRONMENT env var is typically set to Development
    EnvironmentName = Environments.Development
});

ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();


void ConfigureServices(IServiceCollection services)
{
    services.AddHttpContextAccessor();

    // Configure Hellang.Middleware.ProblemDetails package and tune ASP.Net Core MVC to line up with it's conventions
    services
        .AddProblemDetails(o => {
            o.ValidationProblemStatusCode = StatusCodes.Status400BadRequest;
            o.MapToStatusCode<DBConcurrencyException>(StatusCodes.Status409Conflict);
            o.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        })
        .AddControllersWithViews(o => {
            // optional tweak to built-in mvc http responses:
            o.Conventions.Add(new NotFoundResultApiConvention());
        })
        .AddProblemDetailsConventions();

    // configuration for CcAcca.LogDimensionCollection.AspNetCore package...
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
                document.Info.Description = "ASP.NET Core 6.0 Problem Details demo";
            };
        });

    // configuration for CcAcca.ApplicationInsights.ProblemDetails package...
    services
        .AddApplicationInsightsTelemetry()
        // enrich request telemetry with ProblemDetails
        .AddProblemDetailTelemetryInitializer(o => {
            // for demo purposes we're commenting this line out so that 4xx responses show in the failures pane in
            // app insights azure service.
            // YOUR APP will want to uncomment this line to ensure 4xx responses are not considered failures
            // o.IsFailure = ProblemDetailsTelemetryOptions.ServerErrorIsFailure;
        })
        .AddMvcActionDimensionTelemetryInitializer();
}

void ConfigureMiddleware(IApplicationBuilder app)
{
    // apply different exception middleware pipeline depending on whether the request is for the UI or API
    app.UseIfElse(IsUIRequest, UIExceptionMiddleware, NonUIExceptionMiddleware);

    app.UseOpenApi();
    app.UseSwaggerUi3();

    app.UseStaticFiles();

    app.UseRouting();

    // demo a middleware throwing exceptions or setting StatusCode
    // try browsing to:
    // - middleware/error
    // - middleware/status/nnn (replacing nnn with a http status code eg 501)
    app.UseMiddleware<MaybeBadMiddleware>();

    app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
        endpoints.MapControllerRoute("home", "", new { controller = "Home", action = "Index" });
        endpoints.MapControllerRoute("default", uiRoutePrefix + "/{controller=Bad}/{action=Index}/{id?}");
    });
}

void NonUIExceptionMiddleware(IApplicationBuilder app)
{
    // Hellang.Middleware.ProblemDetails
    app.UseProblemDetails();
}

void UIExceptionMiddleware(IApplicationBuilder app)
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/ui/home/error");
    }

    app.UseStatusCodePages();
}

bool IsUIRequest(HttpContext httpContext)
{
    var requestPath = httpContext.Request.Path;
    return requestPath == "/" ||
           requestPath.StartsWithSegments($"/{uiRoutePrefix}", StringComparison.OrdinalIgnoreCase);
}