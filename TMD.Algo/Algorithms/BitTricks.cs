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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Provides a few methods useful for bit manipulation.
    /// </summary>
    public static class BitTricks
    {
        /// <summary>
        /// Given a mask of bits, skip to the next subset of that mask to trial.
        /// </summary>
        /// <param name="i">
        /// Current subset, for the first call this should be the empty set.
        /// </param>
        /// <param name="mask">
        /// Mask of bits representing a set.
        /// </param>
        /// <returns>
        /// The next subset.  If it returns mask, this is the final result.
        /// </returns>
        public static int NextSubset(int i, int mask)
        {
            return (i - mask) & mask;
        }

        /// <summary>
        /// Given a mask of bits, skip to the previous subset of that mask to trial.
        /// </summary>
        /// <param name="i">
        /// Current subset, if reverse iterating, it should be mask on the first call.
        /// </param>
        /// <param name="mask">
        /// Mask of bits representing a set.
        /// </param>
        /// <returns>
        /// The previous subset.  If it returns 0, this is the final result.
        /// </returns>
        public static int PrevSubset(int i, int mask)
        {
            return (i - 1) & mask;
        }

        /// <summary>
        /// Given two masks of bits, one representing the bits which can change and the other representing the required values where the changing mask is unset.
        /// This function generates the next value which matches this set, given a previous value.
        /// </summary>
        /// <param name="i">
        /// Current matching value.  First usage should probably be the constantMask &amp; ~changingMask.
        /// </param>
        /// <param name="changingMask">
        /// Mask which indicates which bits are to change.
        /// </param>
        /// <param name="constantMask">
        /// Mask which indiciates the values for the bits which don't change.
        /// </param>
        /// <returns>
        /// The next value matching the bit pattern.
        /// </returns>
        public static int NextBitPattern(int i, int changingMask, int constantMask)
        {
            return ((i - (changingMask + constantMask)) & changingMask) + constantMask;
        }


        /// <summary>
        /// Given a number, removes the least significant bit.
        /// </summary>
        /// <param name="i">
        /// Number to have its least significant bit removed.
        /// </param>
        /// <returns>
        /// The number with its least significant bit removed.
        /// </returns>
        public static int RemoveLeastSignificantSetBit(int i)
        {
            return i & (i - 1);
        }

        /// <summary>
        /// Given a number, removes all but the least significant bit.
        /// </summary>
        /// <param name="i">
        /// Number to be stripped of all but the least significant bit.
        /// </param>
        /// <returns>
        /// The number stripped of all but the least significant bit.
        /// </returns>
        public static int KeepLeastSignificantSetBit(int i)
        {
            return i & -i;
        }

        /// <summary>
        /// Sets all bits above the least significant set bit.
        /// </summary>
        /// <param name="i">
        /// Number to have all the high bits set.
        /// </param>
        /// <returns>
        /// The number, with all bits more significant than the least significant set bit, set.
        /// </returns>
        public static int BitExtendLeastSignificantSetBit(int i)
        {
            return i | -i;
        }

        /// <summary>
        /// Sets all bits above the least significant set bit, and unsets that bit.
        /// </summary>
        /// <param name="i">
        /// Number to be manipulated.
        /// </param>
        /// <returns>
        /// A number corresponding to the summary.
        /// </returns>
        public static int BitRemoveAndExtendLeastSignificantSetBit(int i)
        {
            return i ^ -i;
        }

        /// <summary>
        /// Sets all the least significant unset bits.
        /// </summary>
        /// <param name="i">
        /// Number to have its low bits saturated.
        /// </param>
        /// <returns>
        /// A number corresponding to the summary.
        /// </returns>
        public static int SaturateLeastSignificantBits(int i)
        {
            return i | (i - 1);
        }

        /// <summary>
        /// Performs the Keep Least significant set bit and saturate least significant bits functions in a single go.
        /// </summary>
        /// <param name="i">
        /// Number to be manipulated.
        /// </param>
        /// <returns>
        /// A number corresponding to the summary.
        /// </returns>
        public static int KeepLeastSignificanSetBitAndSaturateLeastSignificantUnsetBits(int i)
        {
            return i ^ (i - 1);
        }

        /// <summary>
        /// Clears all bits above the least significant set bit, clears that bit, saturates all bits below that.
        /// </summary>
        /// <param name="i">
        /// Number to be manipulated.
        /// </param>
        /// <returns>
        /// A number coresponding to the summary.
        /// </returns>
        public static int ClearAllExceptSaturateLeastSignificantUnsetBits(int i)
        {
            return ~i & (i - 1);
        }

        /// <summary>
        /// Removes the least significant run of bits.
        /// </summary>
        /// <param name="i">
        /// Number to be manipulated.
        /// </param>
        /// <returns>
        /// A number corresponding to the summary.
        /// </returns>
        public static int RemoveLeastSignificantRunOfSetBits(int i)
        {
            return ((i | (i - 1)) + 1) & i;
        }

        /// <summary>
        /// Gets the parent of the specified node in a sideways heap.
        /// </summary>
        /// <param name="i">
        /// Current node in the sideways heap.
        /// </param>
        /// <returns>
        /// The specified nodes parent.
        /// </returns>
        public static int SidewaysHeapParent(int i)
        {
            int k= KeepLeastSignificantSetBit(i);
            return (i - k) | (k << 1);
        }

        /// <summary>
        /// Gets the first child of the specified node in a sideways heap.
        /// </summary>
        /// <param name="i">
        /// Currentnode in the sideways heap.
        /// </param>
        /// <returns>
        /// The specified nodes first child.  Returns self if node is a leaf.
        /// </returns>
        public static int SidewaysHeapFirstChild(int i)
        {
            // possibly wrong for int.MinValue, but i should never be negative.
            return i - (KeepLeastSignificantSetBit(i) >> 1);
        }

        /// <summary>
        /// Gets the second child of the specified node in a sideways heap.
        /// </summary>
        /// <param name="i">
        /// Currentnode in the sideways heap.
        /// </param>
        /// <returns>
        /// The specified nodes second child.  Returns self if node is a leaf.
        /// </returns>
        public static int SidewaysHeapSecondChild(int i)
        {
            // possibly wrong for int.MinValue, but i should never be negative.
            return i + (KeepLeastSignificantSetBit(i) >> 1);
        }

        /// <summary>
        /// Gets the number of set bits in the specified number.
        /// </summary>
        /// <param name="i">
        /// Number to count the set bits in.
        /// </param>
        /// <returns>
        /// The number of set bits in the number.
        /// </returns>
        public static int PopulationCount(int i)
        {
            // TODO: work out why this works with signed right shifts, surely it shouldn't.
            int j = (i >> 1) & 0x55555555;
            i -= j;
            j = i >> 2;
            i = i & 0x33333333;
            j = j & 0x33333333;
            i += j;
            j = i;
            j >>= 4;
            j += i;
            j &= 0x0F0F0F0F;
            j *= 0x01010101;
            return j >> 24;
        }

        /// <summary>
        /// Gets the position of the least significant bit in a number.
        /// </summary>
        /// <param name="i">
        /// Number to get the least significant bit position.
        /// </param>
        /// <returns>
        /// The position of the least significant bit.
        /// If i is 0, returns -1;
        /// </returns>
        public static int LeastSignificantSetBitPos(int i)
        {
            if (i == 0)
                return -1;
            // TODO: consider a method without so many conditionals.
            int j = KeepLeastSignificantSetBit(i);
            int total = 0;
            if ((j & 0x55555555) == 0)
                total += 1;
            if ((j & 0x33333333) == 0)
                total += 2;
            if ((j & 0x0F0F0F0F) == 0)
                total += 4;
            if ((j & 0x00FF00FF) == 0)
                total += 8;
            if ((j & 0x0000FFFF) == 0)
                total += 16;

            return total;
        }

        /// <summary>
        /// Gets whether two numbers have the same most significant bit set.
        /// </summary>
        /// <param name="i">
        /// First number.
        /// </param>
        /// <param name="j">
        /// Second number.
        /// </param>
        /// <returns>
        /// True if the two numbers have the same most significant set bit.  Otherwise false.
        /// </returns>
        public static bool HaveSameMostSignificantSetBit(int i, int j)
        {
            return (uint)(i ^ j) <= (uint)(i & j);
        }
        /// <summary>
        /// Gets the position of the most significant bit in a number.
        /// </summary>
        /// <param name="i">
        /// Number to get the most significant bit position.
        /// </param>
        /// <returns>
        /// The position of the most significant bit.
        /// If i==0, this returns -1;
        /// </returns>
        public static int MostSignificantSetBitPos(int i)
        {
            if (i == 0)
                return -1;
            unchecked
            {
                // TODO: consider a method without so many conditionals.
                int total = 0;
                if (HaveSameMostSignificantSetBit(i, i & (int)0xAAAAAAAA))
                    total += 1;
                if (HaveSameMostSignificantSetBit(i, i & (int)0xCCCCCCCC))
                    total += 2;
                if (HaveSameMostSignificantSetBit(i, i & (int)0xF0F0F0F0))
                    total += 4;
                if (HaveSameMostSignificantSetBit(i, i & (int)0xFF00FF00))
                    total += 8;
                if (HaveSameMostSignificantSetBit(i, i & (int)0xFFFF0000))
                    total += 16;

                return total;
            }
        }

        /// <summary>
        /// Generates the next largest number which has the same number of bits set.
        /// </summary>
        /// <param name="i">
        /// Current number.
        /// </param>
        /// <returns>
        /// Another number with the same number of bits set.
        /// </returns>
        public static int NextCombination(int i)
        {
            int j = KeepLeastSignificantSetBit(i);
            int k = i + j;
            return k + (int)((uint)((k ^ i) / j) >> 2);
        }

        // TODO: This doesn't belong here, create a new class for its type of stuff.

        /// <summary>
        /// Swaps two variables values.
        /// </summary>
        /// <typeparam name="T">
        /// Type of value to swap.
        /// </typeparam>
        /// <param name="a">
        /// First variable.
        /// </param>
        /// <param name="b">
        /// Second variable.
        /// </param>
        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
    }
}
