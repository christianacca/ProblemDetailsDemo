# ProblemDetailsDemo

Example ASP.Net Core Web API that conforms to the [Problem Details spec](https://tools.ietf.org/html/rfc7807)

Uses [Hellang.Middleware.ProblemDetails](https://www.nuget.org/packages/Hellang.Middleware.ProblemDetails) to implement this spec.

## Explore examples

1. `cd src\ProblemDetailsDemo.Api`
2. `dotnet run`
3. Browse to MVC controller endpoints
	* http://localhost:5000/status/406
	* http://localhost:5000/mvc/error
	* http://localhost:5000/mvc/validation-result
	* see `MvcController` class for more example endpoints
4. Browse to Middleware endpoints:
	* http://localhost:5000/middleware/status/406
	* http://localhost:5000/middleware/error

For more information about how to use this nuget library see: https://codeopinion.com/http-api-problem-details-in-asp-net-core/