using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnockKnockServer.Domain;

namespace KnockKnockServer.Selection
{
    public interface IJokeSelector
    {
        KnockKnockJoke SelectJoke();
    }
}
