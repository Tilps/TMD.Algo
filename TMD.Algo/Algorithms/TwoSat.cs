#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Collections.Generic;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Solves 2-Satisfiability (2-SAT) boolean formulas over indexed boolean variables
    /// in O(V + E) time using an iterative Tarjan strongly connected components algorithm.
    /// </summary>
    public class TwoSat
    {
        private readonly List<int>[] implicationGraph;

        /// <summary>
        /// Gets the number of boolean variables in the 2-SAT instance.
        /// </summary>
        public int VariableCount { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoSat"/> class with the specified number of variables.
        /// </summary>
        /// <param name="variableCount">Total number of boolean variables indexed 0 through <c>variableCount - 1</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="variableCount"/> is negative.</exception>
        public TwoSat(int variableCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(variableCount);
            VariableCount = variableCount;
            implicationGraph = new List<int>[2 * variableCount];
            for (int i = 0; i < implicationGraph.Length; i++)
                implicationGraph[i] = [];
        }

        private int ValidateLiteral(int varIndex, bool isTrue, string paramName)
        {
            if ((uint)varIndex >= (uint)VariableCount)
                throw new ArgumentOutOfRangeException(paramName, varIndex, $"Variable index must be in [0, {VariableCount - 1}].");
            return (varIndex << 1) | (isTrue ? 0 : 1);
        }

        /// <summary>
        /// Adds the 2-SAT disjunction clause <c>(x_a == isTrueA) OR (x_b == isTrueB)</c>.
        /// </summary>
        /// <param name="varA">Index of the first variable.</param>
        /// <param name="isTrueA">Required truth value for the first literal to satisfy the clause.</param>
        /// <param name="varB">Index of the second variable.</param>
        /// <param name="isTrueB">Required truth value for the second literal to satisfy the clause.</param>
        public void AddClause(int varA, bool isTrueA, int varB, bool isTrueB)
        {
            int litA = ValidateLiteral(varA, isTrueA, nameof(varA));
            int litB = ValidateLiteral(varB, isTrueB, nameof(varB));
            implicationGraph[litA ^ 1].Add(litB);
            if (litA != litB)
                implicationGraph[litB ^ 1].Add(litA);
        }

        /// <summary>
        /// Adds the implication <c>(x_a == isTrueA) =&gt; (x_b == isTrueB)</c>.
        /// </summary>
        /// <param name="varA">Index of the antecedent variable.</param>
        /// <param name="isTrueA">Truth value triggering the implication.</param>
        /// <param name="varB">Index of the consequent variable.</param>
        /// <param name="isTrueB">Implied truth value of the consequent variable.</param>
        public void AddImplication(int varA, bool isTrueA, int varB, bool isTrueB)
        {
            AddClause(varA, !isTrueA, varB, isTrueB);
        }

        /// <summary>
        /// Forces variable <paramref name="varIndex"/> to take <paramref name="value"/>.
        /// </summary>
        /// <param name="varIndex">Index of the variable to fix.</param>
        /// <param name="value">Forced truth value.</param>
        public void SetValue(int varIndex, bool value)
        {
            int lit = ValidateLiteral(varIndex, value, nameof(varIndex));
            implicationGraph[lit ^ 1].Add(lit);
        }

        /// <summary>
        /// Adds the constraint <c>x_a XOR x_b == true</c> (the two variables must take opposite values).
        /// </summary>
        /// <param name="varA">Index of the first variable.</param>
        /// <param name="varB">Index of the second variable.</param>
        public void AddXor(int varA, int varB)
        {
            AddClause(varA, true, varB, true);
            AddClause(varA, false, varB, false);
        }

        /// <summary>
        /// Adds the constraint <c>x_a == x_b</c> (the two variables must take equal values).
        /// </summary>
        /// <param name="varA">Index of the first variable.</param>
        /// <param name="varB">Index of the second variable.</param>
        public void AddEqual(int varA, int varB)
        {
            AddClause(varA, true, varB, false);
            AddClause(varA, false, varB, true);
        }

        /// <summary>
        /// Determines whether the formula is satisfiable and, if so, produces a valid truth assignment.
        /// Uses iterative Tarjan's SCC so arbitrarily deep implication chains cannot overflow the call stack.
        /// </summary>
        /// <param name="assignment">
        /// When this method returns <c>true</c>, contains a boolean array of length <see cref="VariableCount"/>
        /// satisfying all added constraints; otherwise an empty array.
        /// </param>
        /// <returns><c>true</c> if a satisfying assignment exists; <c>false</c> otherwise.</returns>
        public bool TrySolve(out bool[] assignment)
        {
            int nodeCount = implicationGraph.Length;
            if (VariableCount == 0)
            {
                assignment = [];
                return true;
            }

            int[] tin = new int[nodeCount];
            int[] low = new int[nodeCount];
            int[] comp = new int[nodeCount];
            Array.Fill(tin, -1);
            Array.Fill(comp, -1);

            bool[] onStack = new bool[nodeCount];
            int[] sccStack = new int[nodeCount];
            int sccTop = 0;

            int[] dfsNode = new int[nodeCount];
            int[] dfsEdge = new int[nodeCount];
            int timer = 0;
            int compCount = 0;

            for (int start = 0; start < nodeCount; start++)
            {
                if (tin[start] != -1)
                    continue;

                int depth = 0;
                dfsNode[0] = start;
                dfsEdge[0] = 0;
                tin[start] = low[start] = timer++;
                sccStack[sccTop++] = start;
                onStack[start] = true;

                while (depth >= 0)
                {
                    int u = dfsNode[depth];
                    int edgeIdx = dfsEdge[depth];
                    List<int> adj = implicationGraph[u];

                    if (edgeIdx < adj.Count)
                    {
                        dfsEdge[depth] = edgeIdx + 1;
                        int v = adj[edgeIdx];
                        if (tin[v] == -1)
                        {
                            tin[v] = low[v] = timer++;
                            sccStack[sccTop++] = v;
                            onStack[v] = true;
                            depth++;
                            dfsNode[depth] = v;
                            dfsEdge[depth] = 0;
                        }
                        else if (onStack[v] && tin[v] < low[u])
                        {
                            low[u] = tin[v];
                        }
                    }
                    else
                    {
                        if (low[u] == tin[u])
                        {
                            while (true)
                            {
                                int w = sccStack[--sccTop];
                                onStack[w] = false;
                                comp[w] = compCount;
                                if (w == u)
                                    break;
                            }
                            compCount++;
                        }

                        depth--;
                        if (depth >= 0)
                        {
                            int parent = dfsNode[depth];
                            if (low[u] < low[parent])
                                low[parent] = low[u];
                        }
                    }
                }
            }

            bool[] res = new bool[VariableCount];
            for (int v = 0; v < VariableCount; v++)
            {
                int trueComp = comp[v << 1];
                int falseComp = comp[(v << 1) | 1];
                if (trueComp == falseComp)
                {
                    assignment = [];
                    return false;
                }
                // Tarjan emits sink SCCs first, so trueComp < falseComp means true is downstream/forced first.
                res[v] = trueComp < falseComp;
            }

            assignment = res;
            return true;
        }
    }
}
