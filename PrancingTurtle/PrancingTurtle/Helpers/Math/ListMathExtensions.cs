using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database.Models;

namespace PrancingTurtle.Helpers.Math
{
    public static class ListMathExtensions
    {
        public static long Mean(this List<long> list)
        {
            return list.Sum() / list.Count;
        }
        public static long StdDev(this List<long> list)
        {
            var mean = list.Sum() / list.Count;
            var sumOfSquares = list.Sum(i => (i - mean) * (i - mean));
            return (long) System.Math.Sqrt(sumOfSquares / list.Count);
        }
    }
}