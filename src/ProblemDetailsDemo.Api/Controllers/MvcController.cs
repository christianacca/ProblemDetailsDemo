using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsDemo.Api.MvcCustomizations;

namespace ProblemDetailsDemo.Api.Controllers
{
    [Route("mvc")]
    [ApiController]
    public class MvcController : ControllerBase
    {
        /// <summary>
        /// Return the status code supplied using a <see cref="StatusCodeResult"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hellang.Middleware.ProblemDetails will return this status code as a ProblemDetails
        /// response.
        /// </para>
        /// <para>
        /// The ProblemDetails to returned can be configured using ProblemDetailsOptions.MapStatusCode
        /// </para>
        /// </remarks>
        /// <param name="statusCode">The http status code to return</param>
        [HttpGet("status/{statusCode}")]
        public IActionResult Status([FromRoute] int statusCode)
        {
            return StatusCode(statusCode);
        }

        /// <summary>
        /// Throw a <see cref="NotImplementedException"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hellang.Middleware.ProblemDetails will return a 501 status code
        /// as a ProblemDetails response.
        /// </para>
        /// <para>
        /// This mapping of exception to status code is configured using
        /// ProblemDetailsOptions.Map method
        /// </para>
        /// </remarks>
        [HttpGet("error")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status501NotImplemented)]
        public IActionResult Error()
        {
            throw new NotImplementedException("This is an exception thrown from an MVC controller.");
        }

        /// <summary>
        /// Throw a <see cref="InvalidOperationException"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hellang.Middleware.ProblemDetails will return a 500 status code
        /// as a ProblemDetails response.
        /// </para>
        /// <para>
        /// This "catch-all" mapping of exception to status code is configured using
        /// ProblemDetailsOptions.Map method
        /// </para>
        /// </remarks>
        [HttpGet("error-invalid-operation")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult InvalidOperation()
        {
            throw new InvalidOperationException("BANG");
        }

        /// <summary>
        /// Throw a <see cref="ProblemDetailsException"/> with status code 422 along
        /// with one ModelState error
        /// </summary>
        /// <remarks>
        /// Hellang.Middleware.ProblemDetails will convert the ProblemDetailsException
        /// into a ProblemDetails response
        /// </remarks>
        [HttpGet("error/details")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ErrorDetails()
        {
            ModelState.AddModelError("someProperty", "This property failed validation.");

            var validation = new ValidationProblemDetails(ModelState)
            {
                Status = StatusCodes.Status422UnprocessableEntity
            };

            throw new ProblemDetailsException(validation);
        }

        /// <summary>
        /// Return an custom <see cref="ProblemDetails"/> using a <see cref="BadRequestObjectResult"/>
        /// </summary>
        [HttpGet("validation-result")]
        [ProducesResponseType(typeof(OutOfCreditProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult Result()
        {
            var problem = new OutOfCreditProblemDetails
            {
                Type = "https://example.com/probs/out-of-credit",
                Title = "You do not have enough credit.",
                Detail = "Your current balance is 30, but that costs 50.",
                Instance = "/account/12345/msgs/abc",
                Balance = 30.0m,
                Accounts = {"/account/12345", "/account/67890"},
                Status = StatusCodes.Status400BadRequest
            };

            return BadRequest(problem);
        }

        /// <summary>
        /// Example of automatic ASP.Net Core Http model validation
        /// </summary>
        /// <remarks>
        /// As of ASP.Net Core 2.1, model validation is performed before the action is executed.
        /// As of ASP.Net Core 2.2, any invalid model will produce a ProblemDetails
        /// response with a list of ModelState errors
        /// </remarks>
        /// <param name="model">A model to validate</param>
        [HttpGet("implicit-input-validation")]
        [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.ValidatedGet))]
        public ActionResult<AccountInputModel> ImplicitInputValidation([FromQuery] AccountInputModel model)
        {
            // try the following url: mvc/implicit-input-validation
            // note: with the above url you won't even hit this breakpoint - the framework will not even call our Action method

            return model;
        }

        /// <summary>
        /// Example of manual Http model validation, returning a <see cref="BadRequestObjectResult"/>
        /// with validation errors
        /// </summary>
        /// <remarks>
        /// Asp.Net Core 2.2 needs a little help to ensure the BadRequestObjectResult
        /// containing ModelState errors is returned as a ProblemDetails response.
        /// A custom ResultsFilter is used for this purpose (see ProblemDetailsResultAttribute)
        /// </remarks>
        /// <param name="accountNumber">value that will be manually validated</param>
        [HttpGet("explicit-input-validation")]
        [ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.ValidatedGet))]
        public ActionResult<AccountInputModel> ExplicitInputValidation(int? accountNumber)
        {
            if (!accountNumber.HasValue)
            {
                ModelState.AddModelError(nameof(accountNumber), $"{nameof(accountNumber)} required");
            }

            if (!ModelState.IsValid)
            {
                // this response is converted to use ProblemDetails via ProblemDetailsResultAttribute
                return BadRequest(ModelState);
            }

            return new AccountInputModel
            {
                AccountNumber = accountNumber,
                Reference = "Blah"
            };
        }

        /// <summary>
        /// Return a null
        /// </summary>
        /// <remarks>
        /// By default, Asp.Net Core 2.2 would produce a 204 No content result.
        /// We change this behaviour using a custom ResultsFilter to return a 404
        /// Not Found result. (see NotFoundResultAttribute).
        /// The built-in ClientErrorResultFilter then converts this into a
        /// ProblemDetails response
        /// </remarks>
        [HttpGet("missing-resource")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountInputModel> MissingResourse()
        {
            // result pipeline:
            // null
            // -> ObjectResult(null)
            // -> NotFoundResult [via NotFoundResultAttribute]
            // -> ObjectResult(ProblemDetails) [via ClientErrorResultFilter]
            return null;
        }
    }
}