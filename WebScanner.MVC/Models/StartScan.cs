using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebScanner.MVC.Models
{
    public class StartScan
    {
        [Required(ErrorMessage = "Enter start url")]
        [RegularExpression(@"(?<url>https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*))",
            ErrorMessage = "Invalid url")]
        [StringLength(2048, ErrorMessage = "Max length is 2048")]
        public string StartUrl { get; set; }

        [Required(ErrorMessage = "Enter maximum number of threads")]
        [Range(1, 10, ErrorMessage = "Enter maximum number of threads between 1 and 10")]
        public int MaxCountThreads { get; set; }

        [Required(ErrorMessage = "Enter search text")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Search text length must be between 3 and 20")]
        public string SearchText { get; set; }
     
        [Required(ErrorMessage = "Enter maximum number urls for handling")]
        [Range(1, int.MaxValue, ErrorMessage = "Value must be positive and in integer diapason")]
        public int MaxCountUrls { get; set; }
    }
}