using KnockKnockServer.Domain;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Options
{
    internal class JokesOptions
    {
        [MinLength(5)]
        [ValidateEnumeratedItems]
        public List<KnockKnockJoke> Items { get; set; } = new List<KnockKnockJoke>();
    }
}
