using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Article
    {
        #region Properties
        [Key]
        [Required]
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        #endregion

        #region Relations
        public User Author { get; set; }
        #endregion
    }
}
