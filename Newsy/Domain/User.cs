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
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        #endregion

        #region Relations
        public ICollection<Article> Articles { get; set; }
        #endregion
    }
}
