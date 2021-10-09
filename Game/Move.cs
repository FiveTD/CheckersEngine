using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Game
{
    public class Move : IComparable<Move>
    {
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public sbyte Piece { get; set; }
        public sbyte Jumped { get; set; }
        public bool Turn { get; set; }
        public bool Promoted { get; set; }
        public IEnumerable<Move> Chains { get; set; }
        public int Score { get; private set; }

        public Move()
        {
            Chains = new HashSet<Move>();
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;

            Move o = obj as Move;

            return FromX == o.FromX && FromY == o.FromY &&
                ToX == o.ToX && ToY == o.ToY;
        }

        public override int GetHashCode()
        {
            return FromX + FromY * 10 + ToX * 100 + ToY * 1000;
        }

        public int CalculateScore()
        {
            Score = 0;

            if (Promoted) Score++;
            if (Jumped != 0) Score += Math.Abs(Jumped);
            if (HasChains()) Score += 2;

            return Score;
        }

        public bool HasChains()
        {
            return !Chains.Empty();
        }

        public int CompareTo([AllowNull] Move other)
        {
            return CalculateScore() - other.CalculateScore();
        }
    }
}
