using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Options
{
    internal class ServerOptions
    {
        [Range(1, 65535)]
        public int Port { get; set; }

        [Range(1, 100)]
        public int MaxClients { get; set; }
    }
}
