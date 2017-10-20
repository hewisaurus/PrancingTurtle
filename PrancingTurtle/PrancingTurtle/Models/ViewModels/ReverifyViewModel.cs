using System.ComponentModel.DataAnnotations;

namespace PrancingTurtle.Models.ViewModels
{
    public class ReverifyViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}