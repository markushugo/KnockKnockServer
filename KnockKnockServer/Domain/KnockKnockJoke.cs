using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Domain
{
    public class KnockKnockJoke
    {
        [Required]
        public required string Who { get; set; }
        [Required]
        public required string Punchline { get; set; }
    }
}
