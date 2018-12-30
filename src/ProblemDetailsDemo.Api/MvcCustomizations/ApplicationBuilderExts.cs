using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ProblemDetailsDemo.Api.MvcCustomizations
{
    public static class ApplicationBuilderExts
    {
        public static void UseIfElse(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> ifConfiguration, Action<IApplicationBuilder> elseConfiguration)
        {
            app.UseWhen(predicate, ifConfiguration);
            app.UseWhen(ctx => !predicate(ctx), elseConfiguration);
        }
    }
}