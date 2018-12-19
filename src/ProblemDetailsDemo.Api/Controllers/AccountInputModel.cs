using System.ComponentModel.DataAnnotations;

namespace ProblemDetailsDemo.Api.Controllers
{
    public class AccountInputModel
    {
        [Required] public int? AccountNumber { get; set; }

        [Required] public string Reference { get; set; }
    }
}