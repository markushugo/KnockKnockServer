using KnockKnockServer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Selection
{
    internal class SequentialJokeSelector : IJokeSelector
    {
        private readonly List<KnockKnockJoke> _jokes;
        private int _currentIndex;
        public SequentialJokeSelector(List<KnockKnockJoke> jokes)
        {
            _jokes = jokes;
            _currentIndex = 0;
        }
        public KnockKnockJoke SelectJoke()
        {
            if (_jokes.Count == 0)
            {
                throw new InvalidOperationException("No jokes available.");
            }
            KnockKnockJoke selectedJoke = _jokes[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _jokes.Count; // Move to the next joke, wrap around if needed
            return selectedJoke;
        }
    }
}
