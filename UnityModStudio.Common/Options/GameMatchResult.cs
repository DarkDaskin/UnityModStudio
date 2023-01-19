using System;
using System.Collections.Generic;

namespace UnityModStudio.Common.Options
{
    public abstract class GameMatchResult
    {
        public virtual bool Success => false;

        private protected GameMatchResult()
        {
        }

        public static GameMatchResult Create(IReadOnlyList<Game> games, string ambiguousMessage) =>
            games.Count switch
            {
                0 => new NoMatch(),
                1 => new Match(games[0]),
                _ => new AmbiguousMatch(games, ambiguousMessage)
            };


        public sealed class NoMatch : GameMatchResult
        {
        }

        public sealed class Match : GameMatchResult
        {
            public Game Game { get; }

            public override bool Success => true;

            public Match(Game game)
            {
                Game = game;
            }
        }

        public sealed class AmbiguousMatch : GameMatchResult
        {
            public IReadOnlyCollection<Game> Games { get; }
            public string Message { get; }

            internal AmbiguousMatch(IReadOnlyCollection<Game> games, string message)
            {
                Games = games;
                Message = message;
            }
        }
    }
}