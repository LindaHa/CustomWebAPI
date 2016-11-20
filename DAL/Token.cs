using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWebAPI.DAL
{
    public class Token
    {
        [Required]
        public int UserID { get; set; }

        [Required][Key]
        public string Code { get; set; }

        [Required]
        public DateTime Expiration { get; set; }



    }
}
