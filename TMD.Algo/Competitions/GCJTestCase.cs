#region License
/*
Copyright (c) 2014, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion
using TMD.Algo.Text;

namespace TMD.Algo.Competitions
{
    /// <summary>
    /// Provides methods for reading values contained in a GCJ test case.
    /// The test case must be fully retrieved, since there is no delimiter marking test case start/end.
    /// </summary>
    public class GCJTestCase
    {
        internal GCJTestCase(int index, string[] lines)
        {
            this.index = index;
            this.lines = lines;
        }

        /// <summary>
        /// Gets a value from a line.
        /// </summary>
        /// <param name="value">
        /// Value stored in the line.
        /// </param>
        /// <typeparam name="T">
        /// Type of value to retrieve.
        /// </typeparam>
        public void Get<T>(out T value)
        {
            NextLine().Parse(out value);
        }

        /// <summary>
        /// Gets two values from a line assuming space separation.
        /// </summary>
        /// <param name="value1">
        /// First value stored in the line.
        /// </param>
        /// <param name="value2">
        /// Second value stored in the line.
        /// </param>
        /// <typeparam name="T1">
        /// First type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Second type of value to retrieve.
        /// </typeparam>
        public void Get<T1, T2>(out T1 value1, out T2 value2)
        {
            NextLine().Parse(out value1, out value2);
        }

        /// <summary>
        /// Gets three values from a line assuming space separation.
        /// </summary>
        /// <param name="value1">
        /// First value stored in the line.
        /// </param>
        /// <param name="value2">
        /// Second value stored in the line.
        /// </param>
        /// <param name="value3">
        /// Third value stored in the line.
        /// </param>
        /// <typeparam name="T1">
        /// First type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Second type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Third type of value to retrieve.
        /// </typeparam>
        public void Get<T1, T2, T3>(out T1 value1, out T2 value2, out T3 value3)
        {
            NextLine().Parse(out value1, out value2, out value3);
        }

        /// <summary>
        /// Gets four values from a line assuming space separation.
        /// </summary>
        /// <param name="value1">
        /// First value stored in the line.
        /// </param>
        /// <param name="value2">
        /// Second value stored in the line.
        /// </param>
        /// <param name="value3">
        /// Third value stored in the line.
        /// </param>
        /// <param name="value4">
        /// Fourth value stored in the line.
        /// </param>
        /// <typeparam name="T1">
        /// First type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Second type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Third type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T4">
        /// Fourth type of value to retrieve.
        /// </typeparam>
        public void Get<T1, T2, T3, T4>(out T1 value1, out T2 value2, out T3 value3, out T4 value4)
        {
            NextLine().Parse(out value1, out value2, out value3, out value4);
        }

        /// <summary>
        /// Gets five values from a line assuming space separation.
        /// </summary>
        /// <param name="value1">
        /// First value stored in the line.
        /// </param>
        /// <param name="value2">
        /// Second value stored in the line.
        /// </param>
        /// <param name="value3">
        /// Third value stored in the line.
        /// </param>
        /// <param name="value4">
        /// Fourth value stored in the line.
        /// </param>
        /// <param name="value5">
        /// Fifth value stored in the line.
        /// </param>
        /// <typeparam name="T1">
        /// First type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Second type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Third type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T4">
        /// Fourth type of value to retrieve.
        /// </typeparam>
        /// <typeparam name="T5">
        /// Fifth type of value to retrieve.
        /// </typeparam>
        public void Get<T1, T2, T3, T4, T5>(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5)
        {
            NextLine().Parse(out value1, out value2, out value3, out value4, out value5);
        }

        /// <summary>
        /// Gets six values from a line assuming space separation.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="value4"></param>
        /// <param name="value5"></param>
        /// <param name="value6"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        public void Get<T1, T2, T3, T4, T5, T6>(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6)
        {
            NextLine().Parse(out value1, out value2, out value3, out value4, out value5, out value6);
        }

        /// <summary>
        /// Gets seven values from a line assuming space separation.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="value4"></param>
        /// <param name="value5"></param>
        /// <param name="value6"></param>
        /// <param name="value7"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        public void Get<T1, T2, T3, T4, T5, T6, T7>(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6, out T7 value7)
        {
            NextLine().Parse(out value1, out value2, out value3, out value4, out value5, out value6, out value7);
        }

        /// <summary>
        /// Gets eight values from a line assuming space separation.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="value4"></param>
        /// <param name="value5"></param>
        /// <param name="value6"></param>
        /// <param name="value7"></param>
        /// <param name="value8"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        public void Get<T1, T2, T3, T4, T5, T6, T7, T8>(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6, out T7 value7, out T8 value8)
        {
            NextLine().Parse(out value1, out value2, out value3, out value4, out value5, out value6, out value7, out value8);
        }

        /// <summary>
        /// Gets an array of values from a line assuming space separation.
        /// </summary>
        /// <param name="values">
        /// Receives the values stored in the line.
        /// </param>
        /// <typeparam name="T">
        /// Type of values stored in the line.
        /// </typeparam>
        public void Get<T>(out T[] values)
        {
            NextLine().Parse(out values);
        }

        /// <summary>
        /// Gets an array of values which are stored over multiple lines.
        /// </summary>
        /// <param name="count">
        /// Number of lines containing values.
        /// </param>
        /// <param name="values">
        /// Values stored in the lines.
        /// </param>
        /// <typeparam name="T">
        /// Type of values stored in the lines.
        /// </typeparam>
        public void GetLines<T>(int count, out T[] values)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                T value;
                Get(out value);
                result[i] = value;
            }
            values = result;
        }

        /// <summary>
        /// Gets two arrays of values which are stored over multiple lines in separate columns
        /// </summary>
        /// <param name="count">
        /// Number of lines containing values.
        /// </param>
        /// <param name="values1">
        /// Values stored in the first entry in each line.
        /// </param>
        /// <param name="values2">
        /// Values stored in the second entry in each line.
        /// </param>
        /// <typeparam name="T1">
        /// First type of values stored in the lines.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Second type of values stored in the lines.
        /// </typeparam>
        public void GetLines<T1, T2>(int count, out T1[] values1, out T2[] values2)
        {
            T1[] result1 = new T1[count];
            T2[] result2 = new T2[count];
            for (int i = 0; i < count; i++)
            {
                T1 value1;
                T2 value2;
                Get(out value1, out value2);
                result1[i] = value1;
                result2[i] = value2;
            }
            values1 = result1;
            values2 = result2;
        }

        /// <summary>
        /// Gets a (potentially jagged) matrix from a specific number of lines.
        /// </summary>
        /// <param name="count">
        /// Number of lines containing values.
        /// </param>
        /// <param name="values">
        /// Values stored in the lines.
        /// </param>
        /// <typeparam name="T">
        /// Type of values stored in the lines.
        /// </typeparam>
        public void GetMatrix<T>(int count, out T[][] values)
        {
            T[][] result = new T[count][];
            for (int i = 0; i < count; i++)
            {
                T[] row;
                Get(out row);
                result[i] = row;
            }
            values = result;
        }

        private string NextLine()
        {
            string result = lines[index];
            index++;
            return result;
        }

        private int index;
        private string[] lines;
    }
}