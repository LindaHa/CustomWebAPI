using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWebAPI.DAL
{
    /// <summary>
    /// The entity to represent our access token
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets or sets the user identifier, the token belongs to this user.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// Gets or sets the code - the token identifier.
        /// </summary>
        /// <value>
        /// The code - the token identifier.
        /// </value>
        [Required][Key]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the token.
        /// </summary>
        /// <value>
        /// The expiration date of the token.
        /// </value>
        [Required]
        public DateTime Expiration { get; set; }
    }
}
