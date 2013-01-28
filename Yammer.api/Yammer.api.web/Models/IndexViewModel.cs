using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yammer.api.web.Models
{
    
    public class IndexViewModel
    {
        [Required]
        [Display(Name = "Client ID")]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

       
    }
}