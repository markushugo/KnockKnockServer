using KnockKnockServer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockKnockServer.Selection
{
    internal class RandomJokeSelector : IJokeSelector
    {
        private readonly List<KnockKnockJoke> _jokes;
        private readonly Random _random;
        public RandomJokeSelector(List<KnockKnockJoke> jokes)
        {
            _jokes = jokes;
            _random = new Random();
        }
        public KnockKnockJoke SelectJoke()
        {
            int index = _random.Next(_jokes.Count);
            return _jokes[index];
        }
    }
}
