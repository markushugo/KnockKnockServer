using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Domain
{
    public enum ConversationState
    {
        WaitingForWhosThere,
        WaitingForWho,
        Finished
    }
}
