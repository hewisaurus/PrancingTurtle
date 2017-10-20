using System.ComponentModel.DataAnnotations;

namespace PrancingTurtle.Models.Misc
{
    public class ValidateExistingSession
    {
        [Required]
        [Display(Name="Session ID")]
        public int SessionId { get; set; }
    }
}