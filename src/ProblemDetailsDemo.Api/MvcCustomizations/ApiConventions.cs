using Microsoft.AspNetCore.Mvc;

namespace ProblemDetailsDemo.Api.MvcCustomizations
{
    public static class ApiConventions
    {
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
//        [ProducesDefaultResponseType]
        public static void ValidatedGet()
        {
        }
    }
}