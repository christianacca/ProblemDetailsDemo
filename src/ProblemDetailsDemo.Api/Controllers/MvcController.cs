using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProblemDetailsDemo.Api.Controllers
{
    [Route("mvc")]
    [ApiController]
    public class MvcController : ControllerBase
    {
        [HttpGet("status/{statusCode}")]
        public IActionResult Status([FromRoute] int statusCode)
        {
            return StatusCode(statusCode);
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            throw new NotImplementedException("This is an exception thrown from an MVC controller.");
        }

        [HttpGet("error-invalid-operation")]
        public IActionResult InvalidOperation()
        {
            throw new InvalidOperationException("BANG");
        }

        [HttpGet("error/details")]
        public IActionResult ErrorDetails()
        {
            ModelState.AddModelError("someProperty", "This property failed validation.");

            var validation = new ValidationProblemDetails(ModelState)
            {
                Status = StatusCodes.Status422UnprocessableEntity
            };

            throw new ProblemDetailsException(validation);
        }

        [HttpGet("validation-result")]
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

            // note: unlike other responses returning a BadRequest results in a content type that is NOT application/problem+json
            // this is not ideal. To deal with this
            // * wait for asp.net core 2.2 which should fix the issue
            // * throw ProblemDetailsException(problem) instead

            return BadRequest(problem);

        }

        [HttpGet("implicit-input-validation")]
        public IActionResult ImplicitInputValidation([FromQuery] AccountInputModel model)
        {
            // try the following url: mvc/implicit-input-validation
            // note: with the above url you won't even hit this breakpoint - the framework will not even call our Action method

            return Ok(model);
        }
    }
}