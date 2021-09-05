using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class User
    {
        #region Properties
        [Key]
        [Required]
        public Guid ID { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        #endregion

        #region Relations
        public ICollection<Article> Articles { get; set; }
        #endregion
    }
}
