#region License
/*
Copyright (c) 2008, Gareth Pearce (www.themissingdocs.net)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the www.themissingdocs.net nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMD.Algo.Algorithms.Generic;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Methods for calculating optimal scenarios.
    /// </summary>
    public static class OptimalFinding
    {

        /// <summary>
        /// Finds the set of items which has minimal cost but reaches target value.
        /// </summary>
        /// <param name="target">
        /// Target value to reach.
        /// </param>
        /// <param name="costs">
        /// Costs of each item.
        /// </param>
        /// <param name="values">
        /// Values of each item.
        /// </param>
        /// <returns></returns>
        public static int[] OptimalCostForTarget(int target, int[] costs, int[] values)
        {
            // break in to half to allow us to double the number of things we can process.
            // we can easily generate all combinations to work out what is best, but if number of items exceeds ~26, that is going to be slow.
            // This hack lets us go to almost double that in theory.
            int firstHalf = costs.Length / 2 + 1;
            int rest = costs.Length - firstHalf;
            List<KeyValuePair<int, int>> firstCombinationTotals = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> secondCombinationTotals = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < (1 << (firstHalf + 1)); i++)
            {
                int totalCost = 0;
                int totalValue = 0;
                for (int j = 0; j < firstHalf; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        totalCost += costs[j];
                        totalValue += values[j];
                    }
                }
                firstCombinationTotals.Add(new KeyValuePair<int,int>(totalValue, totalCost));
                totalCost = 0;
                totalValue = 0;
                if (i < (1 << (rest + 1)))
                {
                    for (int j = 0; j < rest; j++)
                    {
                        if ((i & (1 << j)) != 0)
                        {
                            totalCost += costs[firstHalf+j];
                            totalValue += values[firstHalf+j];
                        }
                    }
                    secondCombinationTotals.Add(new KeyValuePair<int, int>(totalValue, totalCost));
                }
            }
            firstCombinationTotals.Sort(KvpComparer<int, int>.Default);
            secondCombinationTotals.Sort(KvpComparer<int, int>.Default);

            int other = secondCombinationTotals.Count - 1;
            int best = int.MaxValue;
            int localBest = int.MaxValue;
            List<int> result = new List<int>();
            int firstBestCombo = -1;
            int secondBestComboTemp = -1;
            int secondBestCombo = -1;
            for (int i = 0; i < firstCombinationTotals.Count; i++)
            {
                while (other >= 0 && secondCombinationTotals[other].Key + firstCombinationTotals[i].Key >= target)
                {
                    if (secondCombinationTotals[other].Value < localBest)
                    {
                        localBest = secondCombinationTotals[other].Value;
                        secondBestComboTemp = other;
                    }
                    other--;
                }
                if (localBest < int.MaxValue)
                {
                    int newTotal = firstCombinationTotals[i].Value + localBest;
                    if (newTotal < best)
                    {
                        best = newTotal;
                        firstBestCombo = i;
                        secondBestCombo = secondBestComboTemp;
                    }
                }
            }
            if (best < int.MaxValue)
            {
                for (int j = 0; j < firstHalf; j++)
                {
                    if ((firstBestCombo & (1 << j)) != 0)
                    {
                        result.Add(j);
                    }
                    if ((secondBestCombo & (1 << j)) != 0)
                    {
                        result.Add(j + firstHalf);
                    }
                }
                return result.ToArray();
            }
            else
                return null;
        }
    }
}
