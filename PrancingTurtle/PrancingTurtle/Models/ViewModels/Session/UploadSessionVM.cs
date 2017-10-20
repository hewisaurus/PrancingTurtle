using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Session
{
    public class UploadSessionVM
    {
        [Required]
        [DisplayName("Uploader")]
        public int UploadCharacterId { get; set; }
        public int UploadedSessionId { get; set; }
        public List<AuthUserCharacter> Characters { get; set; }
        [Required]
        public DateTime SessionDate { get; set; }
        [Required]
        [DisplayName("Session Name")]
        public string Name { get; set; }
        [DisplayName("Make encounters public")]
        public bool Public { get; set; }

        public string UploadToken { get; set; }

        public string UploadTokenDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(UploadToken)) return "";
                return string.Format("{0}.zip", UploadToken);
            }
        }
    }
}