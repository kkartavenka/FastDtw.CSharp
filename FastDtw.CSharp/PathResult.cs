using System;
using System.Collections.Generic;

namespace FastDtw.CSharp
{
    public class PathResult
    {
        internal PathResult(double score, List<Tuple<int, int>> path)
        {
            Score = score;
            Path = path;
        }

        public double Score { get; }
        public List<Tuple<int, int>> Path { get; }
    }
}