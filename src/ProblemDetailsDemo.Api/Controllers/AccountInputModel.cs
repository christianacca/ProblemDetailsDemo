using System.ComponentModel.DataAnnotations;

namespace ProblemDetailsDemo.Api.Controllers
{
    public class AccountInputModel
    {
        public int AccountNumber { get; set; }

        [Required]
        public string Reference { get; set; }
    }
}