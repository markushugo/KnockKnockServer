using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Domain
{
    public class KnockKnockJoke
    {
        public required string Who { get; set; }
        public required string Punchline { get; set; }
    }
}
