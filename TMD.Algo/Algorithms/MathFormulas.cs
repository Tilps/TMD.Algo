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
using System.Diagnostics.CodeAnalysis;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Math formulas which are not specifically related to anything.
    /// </summary>
    public static class MathFormulas
    {
        /// <summary>
        /// Gets the nth harmonic number.
        /// </summary>
        /// <param name="n">
        /// Index in to the harmonic series.
        /// </param>
        /// <returns>
        /// The nth harmonic number.
        /// </returns>
        public static double HarmonicNumber(int n)
        {
            if (n > 32)
            {
                return EulerGamma + Math.Log(n) + 0.5 / n - 1.0 / 12 / n / n + 1.0 / 120 / n / n / n / n - 1.0 / 252 / n / n / n / n / n / n;
            }
            else
            {
                double accumulator = 0.0;
                for (int i = 0; i < n; i++)
                {
                    accumulator += 1.0 / n;
                }
                return accumulator;
            }
        }

        /// <summary>
        /// Gets the difference between the harmonic number at end and the one before start.
        /// </summary>
        /// <param name="start">
        /// Start of the harmonic segment.
        /// </param>
        /// <param name="end">
        /// End of the harmonic segment.
        /// </param>
        /// <returns>
        /// The difference between the harmonic number at end and the one before start.
        /// </returns>
        public static double HarmonicSegment(int start, int end)
        {
            int before = start - 1;
            if (start > 32)
            {
                Fraction endF = new Fraction(end, 1);
                Fraction beforeF = new Fraction(before, 1);
                Fraction one = new Fraction(1, 1);
                double beforeD = before;
                double endD = end;
                double accumulator;
                if (end - before < before / 10)
                    accumulator = Log1p((double)(end - before) / (double)before);
                else
                    accumulator = Math.Log((double)end / (double)before);
                long product = (long)before * (long)end;
                if (product < (1L << 62))
                    accumulator += (double)(one / endF - one / beforeF) / 2.0;
                else
                    accumulator += (beforeD - endD) / end / before / 2.0;
                if (product < (1L << 31))
                    accumulator -= (double)(one / endF / endF - one / beforeF / beforeF) / 12.0;
                else
                    accumulator -= (beforeD * beforeD - endD * endD) / end / end / before / before / 12.0;
                if (product < (1 << 15))
                    accumulator += (double)(one / endF / endF / endF / endF - one / beforeF / beforeF / beforeF / beforeF) / 120.0;
                else
                    accumulator += (beforeD * beforeD * beforeD * beforeD - endD * endD * endD * endD) / end / end / end / end / before / before / before / before / 120.0;
                if (product < (1 << 10))
                    accumulator -= (double)(one / endF / endF / endF / endF / endF / endF - one / beforeF / beforeF / beforeF / beforeF / beforeF / beforeF) / 252.0;
                else
                    accumulator -= (beforeD * beforeD * beforeD * beforeD * beforeD * beforeD - endD * endD * endD * endD * endD * endD) / end / end / end / end / end / end / before / before / before / before / before / before / 252.0;
                return accumulator;
            }
            else
            {
                return HarmonicNumber(end) - HarmonicNumber(before);
            }
        }

        /// <summary>
        /// Calculates the logarithm of a number close to 1 based on its offset.
        /// </summary>
        /// <param name="offset">
        /// Offset from 1.
        /// </param>
        /// <returns>
        /// Returns log (1+offset)
        /// </returns>
        public static double Log1p(double offset)
        {
            if (offset > 0.1 || offset < -0.1)
                return Math.Log(1 + offset);
            double accumulator = 0.0;
            double offsetPower = -offset;
            for (int i = 1; i <= 30; i++)
            {
                accumulator -= offsetPower / i;
                offsetPower *= -offset;
            }
            return accumulator;
        }

        /// <summary>
        /// The euler mascheroni constant or euler gamma.
        /// </summary>
        public const double EulerGamma = 0.57721566490153286060651209008240243104215933593992;

        /// <summary>
        /// The golden ratio constant.
        /// </summary>
        /// <remarks>
        /// Ratio of consecutive fibonacci numbers converges to this value with exponentially decaying erf.
        /// </remarks>
        public static double GoldenRatio = (1.0 + Math.Sqrt(5)) / 2.0;
    }
}
