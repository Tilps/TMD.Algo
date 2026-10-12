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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Numerical integration and differential equation solving methods.
    /// </summary>
    public static class NumericalMethods
    {
        /// <summary>
        /// Solves a system of ordinary differential equations dy/dx = f(x, y) using the classical 4th-order Runge-Kutta method (RK4).
        /// </summary>
        /// <param name="f">Derivative function evaluating dy/dx given current x and state vector y.</param>
        /// <param name="x0">Initial x coordinate.</param>
        /// <param name="y0">Initial state vector at x0.</param>
        /// <param name="x1">Final x coordinate.</param>
        /// <param name="steps">Number of integration steps (must be positive).</param>
        /// <returns>The state vector y at x1.</returns>
        public static double[] RungeKutta4(Func<double, double[], double[]> f, double x0, double[] y0, double x1, int steps)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            if (y0 == null) throw new ArgumentNullException(nameof(y0));
            if (steps <= 0) throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be positive.");

            int dim = y0.Length;
            double h = (x1 - x0) / steps;
            double[] y = (double[])y0.Clone();
            double x = x0;

            double[] tempY = new double[dim];

            for (int s = 0; s < steps; s++)
            {
                double[] k1 = f(x, y);

                for (int i = 0; i < dim; i++)
                    tempY[i] = y[i] + 0.5 * h * k1[i];
                double[] k2 = f(x + 0.5 * h, tempY);

                for (int i = 0; i < dim; i++)
                    tempY[i] = y[i] + 0.5 * h * k2[i];
                double[] k3 = f(x + 0.5 * h, tempY);

                for (int i = 0; i < dim; i++)
                    tempY[i] = y[i] + h * k3[i];
                double[] k4 = f(x + h, tempY);

                for (int i = 0; i < dim; i++)
                    y[i] += (h / 6.0) * (k1[i] + 2.0 * k2[i] + 2.0 * k3[i] + k4[i]);

                x = x0 + (s + 1) * h;
            }

            return y;
        }

        /// <summary>
        /// Solves a single ordinary differential equation dy/dx = f(x, y) using the classical 4th-order Runge-Kutta method (RK4).
        /// </summary>
        /// <param name="f">Derivative function evaluating dy/dx given current x and scalar y.</param>
        /// <param name="x0">Initial x coordinate.</param>
        /// <param name="y0">Initial value y(x0).</param>
        /// <param name="x1">Final x coordinate.</param>
        /// <param name="steps">Number of integration steps (must be positive).</param>
        /// <returns>The value y at x1.</returns>
        public static double RungeKutta4(Func<double, double, double> f, double x0, double y0, double x1, int steps)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            if (steps <= 0) throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be positive.");

            double h = (x1 - x0) / steps;
            double y = y0;
            double x = x0;

            for (int s = 0; s < steps; s++)
            {
                double k1 = f(x, y);
                double k2 = f(x + 0.5 * h, y + 0.5 * h * k1);
                double k3 = f(x + 0.5 * h, y + 0.5 * h * k2);
                double k4 = f(x + h, y + h * k3);

                y += (h / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
                x = x0 + (s + 1) * h;
            }

            return y;
        }

        /// <summary>
        /// Computes the definite integral of f(x) from a to b using Simpson's 1/3 rule with n subintervals (rounded up to an even number).
        /// </summary>
        /// <param name="f">Integrand function.</param>
        /// <param name="a">Lower bound of integration.</param>
        /// <param name="b">Upper bound of integration.</param>
        /// <param name="intervals">Number of subintervals (at least 2; made even if odd).</param>
        /// <returns>Approximated integral.</returns>
        public static double Simpson(Func<double, double> f, double a, double b, int intervals = 1000)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            if (intervals < 2) intervals = 2;
            if (intervals % 2 != 0) intervals++;

            double h = (b - a) / intervals;
            double sum = f(a) + f(b);

            for (int i = 1; i < intervals; i++)
            {
                double x = a + i * h;
                sum += (i % 2 == 0 ? 2.0 : 4.0) * f(x);
            }

            return sum * (h / 3.0);
        }
    }
}

