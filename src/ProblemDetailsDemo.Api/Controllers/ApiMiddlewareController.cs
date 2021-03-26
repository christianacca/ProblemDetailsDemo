﻿using System;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsDemo.Api.ExampleMiddleware;

namespace ProblemDetailsDemo.Api.Controllers
{
  /// <summary>
  ///   Fake controller used to generate Swagger / OpenAPI documentation for <see cref="MaybeBadMiddleware" />
  ///   // todo: switch to endpoint routing and then see if this controller can be dropped
  /// </summary>
  [Route("api-middleware")]
  [ApiController]
  public class ApiMiddlewareController : ControllerBase
  {
    /// <summary>
    ///   Sets the Response.StatusCode to the value supplied
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Hellang.Middleware.ProblemDetails will return this status code as a ProblemDetails
    ///     response.
    ///   </para>
    ///   <para>
    ///     The ProblemDetails to returned can be configured using ProblemDetailsOptions.MapStatusCode
    ///   </para>
    ///   <para>
    ///     Source code for this endpoint: https://tinyurl.com/problems-middleware#L23-L32
    ///   </para>
    /// </remarks>
    /// <param name="statusCode">The http status code to return</param>
    [HttpGet("status/{statusCode}")]
    public IActionResult Status([FromRoute] int statusCode)
    {
      throw new NotImplementedException("Endpoing is implemented by middleware class 'MaybeBadMiddleware'");
    }

    /// <summary>
    ///   Throw a <see cref="DBConcurrencyException" />
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Hellang.Middleware.ProblemDetails will return a 409 status code
    ///     as a ProblemDetails response.
    ///   </para>
    ///   <para>
    ///     This mapping of exception to status code is configured using
    ///     ProblemDetailsOptions.Map method
    ///   </para>
    ///   <para>
    ///     Source code for this endpoint: https://tinyurl.com/problems-middleware#L20-L21
    ///   </para>
    /// </remarks>
    [HttpGet("error")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Error()
    {
      throw new NotImplementedException(
        $"Endpoint is implemented by middleware class '{nameof(MaybeBadMiddleware)}'");
    }
  }
}