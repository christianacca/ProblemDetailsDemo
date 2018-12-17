using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProblemDetailsDemo.Api.ExampleMiddleware;
using ProblemDetailsDemo.Api.MvcCustomizations;
using ProblemDetailsDemo.Api.ProblemDetailsConfig;

namespace ProblemDetailsDemo.Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.ConfigureOptions<ProblemDetailsOptionsCustomSetup>();
            // Hellang.Middleware.ProblemDetails package
            services.AddProblemDetails();

            services
                .AddMvc(o =>
                {
                    // optional tweaks to built-in mvc non-success http responses
                    o.Conventions.Add(new NotFoundResultApiConvention());
                    o.Conventions.Add(new ProblemDetailsResultApiConvention());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            // Hellang.Middleware.ProblemDetails package
            app.UseProblemDetails();

            // demo a middleware throwing exceptions or setting StatusCode
            // try browsing to:
            // - middleware/error
            // - middleware/status/nnn (replacing nnn with a http status code eg 501)
            app.UseMiddleware<MaybeBadMiddleware>();

            app.UseMvc();
        }
    }
}