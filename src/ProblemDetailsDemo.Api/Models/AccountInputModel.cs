using System.ComponentModel.DataAnnotations;

namespace ProblemDetailsDemo.Api.Models
{
  public class AccountInputModel
  {
    [Required] public int? AccountNumber { get; set; }

    [Required] public string Reference { get; set; }
  }
}