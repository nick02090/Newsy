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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        #endregion

        #region Relations
        public ICollection<Article> Articles { get; set; }
        #endregion


        public void HidePasswordRelatedData()
        {
            Password = null;
            PasswordHash = null;
            PasswordSalt = null;
        }
    }
}
