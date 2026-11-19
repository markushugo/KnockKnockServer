using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Domain
{
    
    public class KnockKnockConversation
    {
        
        public ConversationState State { get; private set; } = ConversationState.WaitingForWhosThere;

        private readonly KnockKnockJoke _joke;

        public KnockKnockConversation(KnockKnockJoke joke)
        {
            _joke = joke;
        }

        public string ProcessResponse(string response)
        {
            switch (State)
            {
                case ConversationState.WaitingForWhosThere:
                    if (IsWhoQuestion(response))
                    {
                        State = ConversationState.WaitingForWho;
                        return _joke.Who;
                    }
                    else
                    {
                        return "Please respond with 'Who's there?'";
                    }

                case ConversationState.WaitingForWho:
                    if (_joke != null && IsWhoAnswer(response, _joke.Who))
                    {
                        State = ConversationState.Finished;
                        return _joke.Punchline;
                    }
                    else
                    {
                        return $"Please respond with '{_joke?.Who} who?'";
                    }
                case ConversationState.Finished:
                    return "The conversation is finished. Please start a new one.";
                default:
                    throw new InvalidOperationException("Invalid conversation state.");
            }
        }



        /// <summary>Kontrollerer de accepterede varianter af "Who's there?".</summary>
        private static bool IsWhoQuestion(string response) =>
        response.Equals("who's there?", StringComparison.OrdinalIgnoreCase) ||
        response.Equals("whos there?", StringComparison.OrdinalIgnoreCase) ||
        response.Equals("who's there", StringComparison.OrdinalIgnoreCase) ||
        response.Equals("whos there", StringComparison.OrdinalIgnoreCase);

        /// <summary>Kontrollerer om klienten svarer med jokernes navn efterfulgt af "who".</summary>
        private static bool IsWhoAnswer(string response, string who) =>
        response.Equals($"{who} who?", StringComparison.OrdinalIgnoreCase) ||
        response.Equals($"{who} who", StringComparison.OrdinalIgnoreCase);
    }
}
