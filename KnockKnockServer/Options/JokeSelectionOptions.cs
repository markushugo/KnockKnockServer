using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Options
{
    internal class JokeSelectionOptions
    {
        [Required]
        public string Strategy { get; set; } = "";
    }
}
