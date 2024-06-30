﻿using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace LSGL.Core.Maths.Splines.Interpolation
{
    /// <summary>
    /// Left and right boundary conditions.
    /// </summary>
    public enum SplineBoundaryCondition
    {
        /// <summary>
        /// Natural Boundary (Zero second derivative).
        /// </summary>
        Natural = 0,

        /// <summary>
        /// Parabolically Terminated boundary.
        /// </summary>
        ParabolicallyTerminated,

        /// <summary>
        /// Fixed first derivative at the boundary.
        /// </summary>
        FirstDerivative,

        /// <summary>
        /// Fixed second derivative at the boundary.
        /// </summary>
        SecondDerivative
    }

    /// <summary>
    /// Cubic Spline Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class CubicSpline : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _c0;
        readonly double[] _c1;
        readonly double[] _c2;
        readonly double[] _c3;
        readonly double[] _y;
        readonly Lazy<double[]> _indefiniteIntegral;

        /// <param name="x">sample points (N+1), sorted ascending</param>
        /// <param name="c0">Zero order spline coefficients (N)</param>
        /// <param name="c1">First order spline coefficients (N)</param>
        /// <param name="c2">second order spline coefficients (N)</param>
        /// <param name="c3">third order spline coefficients (N)</param>
        public CubicSpline(double[] x, double[] c0, double[] c1, double[] c2, double[] c3, double[] y)
        {
            if (x.Length != c0.Length + 1 || x.Length != c1.Length + 1 || x.Length != c2.Length + 1 || x.Length != c3.Length + 1)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            _x = x;
            _c0 = c0;
            _c1 = c1;
            _c2 = c2;
            _c3 = c3;
            _indefiniteIntegral = new Lazy<double[]>(ComputeIndefiniteIntegral);
            _y = y;
        }

        /// <summary>
        /// Create a Hermite cubic spline interpolation from a set of (x,y) value pairs and their slope (first derivative), sorted ascendingly by x.
        /// </summary>
        public static CubicSpline InterpolateHermiteSorted(double[] x, double[] y, double[] firstDerivatives)
        {
            if (x.Length != y.Length || x.Length != firstDerivatives.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            var c0 = new double[x.Length - 1];
            var c1 = new double[x.Length - 1];
            var c2 = new double[x.Length - 1];
            var c3 = new double[x.Length - 1];
            for (int i = 0; i < c1.Length; i++)
            {
                double w = x[i + 1] - x[i];
                double w2 = w * w;
                c0[i] = y[i];
                c1[i] = firstDerivatives[i];
                c2[i] = (3 * (y[i + 1] - y[i]) / w - 2 * firstDerivatives[i] - firstDerivatives[i + 1]) / w;
                c3[i] = (2 * (y[i] - y[i + 1]) / w + firstDerivatives[i] + firstDerivatives[i + 1]) / w2;
            }

            return new CubicSpline(x, c0, c1, c2, c3, y);
        }

        /// <summary>
        /// Create a Hermite cubic spline interpolation from an unsorted set of (x,y) value pairs and their slope (first derivative).
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolateHermiteInplace(double[] x, double[] y, double[] firstDerivatives)
        {
            if (x.Length != y.Length || x.Length != firstDerivatives.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            Sorting.Sort(x, y, firstDerivatives);
            return InterpolateHermiteSorted(x, y, firstDerivatives);
        }

        /// <summary>
        /// Create a Hermite cubic spline interpolation from an unsorted set of (x,y) value pairs and their slope (first derivative).
        /// </summary>
        public static CubicSpline InterpolateHermite(IEnumerable<double> x, IEnumerable<double> y, IEnumerable<double> firstDerivatives)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateHermiteInplace(x.ToArray(), y.ToArray(), firstDerivatives.ToArray());
        }

        /// <summary>
        /// Create a cubic spline interpolation from a set of (x,y) value pairs, sorted ascendingly by x,
        /// and custom boundary/termination conditions.
        /// </summary>
        public static CubicSpline InterpolateBoundariesSorted(double[] x, double[] y,
            SplineBoundaryCondition leftBoundaryCondition, double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            int n = x.Length;

            // normalize special cases
            if ((n == 2)
                && (leftBoundaryCondition == SplineBoundaryCondition.ParabolicallyTerminated)
                && (rightBoundaryCondition == SplineBoundaryCondition.ParabolicallyTerminated))
            {
                leftBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                leftBoundary = 0d;
                rightBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                rightBoundary = 0d;
            }

            if (leftBoundaryCondition == SplineBoundaryCondition.Natural)
            {
                leftBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                leftBoundary = 0d;
            }

            if (rightBoundaryCondition == SplineBoundaryCondition.Natural)
            {
                rightBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                rightBoundary = 0d;
            }

            var a1 = new double[n];
            var a2 = new double[n];
            var a3 = new double[n];
            var b = new double[n];

            // Left Boundary
            switch (leftBoundaryCondition)
            {
                case SplineBoundaryCondition.ParabolicallyTerminated:
                    a1[0] = 0;
                    a2[0] = 1;
                    a3[0] = 1;
                    b[0] = 2 * (y[1] - y[0]) / (x[1] - x[0]);
                    break;
                case SplineBoundaryCondition.FirstDerivative:
                    a1[0] = 0;
                    a2[0] = 1;
                    a3[0] = 0;
                    b[0] = leftBoundary;
                    break;
                case SplineBoundaryCondition.SecondDerivative:
                    a1[0] = 0;
                    a2[0] = 2;
                    a3[0] = 1;
                    b[0] = (3 * ((y[1] - y[0]) / (x[1] - x[0]))) - (0.5 * leftBoundary * (x[1] - x[0]));
                    break;
                default:
                    throw new NotSupportedException("Invalid Left Boundary Condition.");
            }

            // Central Conditions
            for (int i = 1; i < x.Length - 1; i++)
            {
                a1[i] = x[i + 1] - x[i];
                a2[i] = 2 * (x[i + 1] - x[i - 1]);
                a3[i] = x[i] - x[i - 1];
                b[i] = (3 * (y[i] - y[i - 1]) / (x[i] - x[i - 1]) * (x[i + 1] - x[i])) + (3 * (y[i + 1] - y[i]) / (x[i + 1] - x[i]) * (x[i] - x[i - 1]));
            }

            // Right Boundary
            switch (rightBoundaryCondition)
            {
                case SplineBoundaryCondition.ParabolicallyTerminated:
                    a1[n - 1] = 1;
                    a2[n - 1] = 1;
                    a3[n - 1] = 0;
                    b[n - 1] = 2 * (y[n - 1] - y[n - 2]) / (x[n - 1] - x[n - 2]);
                    break;
                case SplineBoundaryCondition.FirstDerivative:
                    a1[n - 1] = 0;
                    a2[n - 1] = 1;
                    a3[n - 1] = 0;
                    b[n - 1] = rightBoundary;
                    break;
                case SplineBoundaryCondition.SecondDerivative:
                    a1[n - 1] = 1;
                    a2[n - 1] = 2;
                    a3[n - 1] = 0;
                    b[n - 1] = (3 * (y[n - 1] - y[n - 2]) / (x[n - 1] - x[n - 2])) + (0.5 * rightBoundary * (x[n - 1] - x[n - 2]));
                    break;
                default:
                    throw new NotSupportedException("Invalid Right Boundary Condition.");
            }

            // Build Spline
            double[] dd = SolveTridiagonal(a1, a2, a3, b);
            return InterpolateHermiteSorted(x, y, dd);
        }

        /// <summary>
        /// Create a cubic spline interpolation from an unsorted set of (x,y) value pairs and custom boundary/termination conditions.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolateBoundariesInplace(double[] x, double[] y,
            SplineBoundaryCondition leftBoundaryCondition, double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            return InterpolateBoundariesSorted(x, y, leftBoundaryCondition, leftBoundary, rightBoundaryCondition, rightBoundary);
        }

        /// <summary>
        /// Create a cubic spline interpolation from an unsorted set of (x,y) value pairs and custom boundary/termination conditions.
        /// </summary>
        public static CubicSpline InterpolateBoundaries(IEnumerable<double> x, IEnumerable<double> y,
            SplineBoundaryCondition leftBoundaryCondition, double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateBoundariesInplace(x.ToArray(), y.ToArray(), leftBoundaryCondition, leftBoundary, rightBoundaryCondition, rightBoundary);
        }

        /// <summary>
        /// Create a natural cubic spline interpolation from a set of (x,y) value pairs
        /// and zero second derivatives at the two boundaries, sorted ascendingly by x.
        /// </summary>
        public static CubicSpline InterpolateNaturalSorted(double[] x, double[] y)
        {
            return InterpolateBoundariesSorted(x, y, SplineBoundaryCondition.SecondDerivative, 0.0, SplineBoundaryCondition.SecondDerivative, 0.0);
        }

        /// <summary>
        /// Create a natural cubic spline interpolation from an unsorted set of (x,y) value pairs
        /// and zero second derivatives at the two boundaries.
        /// </summary>
        public static CubicSpline InterpolateNatural(IEnumerable<double> x, IEnumerable<double> y)
        {
            return InterpolateBoundaries(x, y, SplineBoundaryCondition.SecondDerivative, 0.0, SplineBoundaryCondition.SecondDerivative, 0.0);
        }

        /// <summary>
        /// Tridiagonal Solve Helper.
        /// </summary>
        /// <param name="a">The a-vector[n].</param>
        /// <param name="b">The b-vector[n], will be modified by this function.</param>
        /// <param name="c">The c-vector[n].</param>
        /// <param name="d">The d-vector[n], will be modified by this function.</param>
        /// <returns>The x-vector[n]</returns>
        static double[] SolveTridiagonal(double[] a, double[] b, double[] c, double[] d)
        {
            for (int k = 1; k < a.Length; k++)
            {
                double t = a[k] / b[k - 1];
                b[k] = b[k] - (t * c[k - 1]);
                d[k] = d[k] - (t * d[k - 1]);
            }

            var x = new double[a.Length];
            x[x.Length - 1] = d[d.Length - 1] / b[b.Length - 1];
            for (int k = x.Length - 2; k >= 0; k--)
            {
                x[k] = (d[k] - (c[k] * x[k + 1])) / b[k];
            }

            return x;
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        bool IInterpolation.SupportsDifferentiation => true;

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        bool IInterpolation.SupportsIntegration => true;

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">PointGL t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return _c0[k] + x * (_c1[k] + x * (_c2[k] + x * _c3[k]));
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">PointGL t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return _c1[k] + x * (2 * _c2[k] + x * 3 * _c3[k]);
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">PointGL t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return 2 * _c2[k] + x * 6 * _c3[k];
        }

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">PointGL t to integrate at.</param>
        public double Integrate(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return _indefiniteIntegral.Value[k] + x * (_c0[k] + x * (_c1[k] / 2 + x * (_c2[k] / 3 + x * _c3[k] / 4)));
        }

        /// <summary>
        /// Definite integral between points a and b.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        public double Integrate(double a, double b) => Integrate(b) - Integrate(a);

        double[] ComputeIndefiniteIntegral()
        {
            var integral = new double[_c1.Length];
            for (int i = 0; i < integral.Length - 1; i++)
            {
                double w = _x[i + 1] - _x[i];
                integral[i + 1] = integral[i] + w * (_c0[i] + w * (_c1[i] / 2 + w * (_c2[i] / 3 + w * _c3[i] / 4)));
            }

            return integral;
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t,
        /// or the left index of the closest segment for extrapolation.
        /// </summary>
        int LeftSegmentIndex(double t)
        {
            int index = Array.BinarySearch(_x, t);
            if (index < 0)
            {
                index = ~index - 1;
            }

            return Math.Min(Math.Max(index, 0), _x.Length - 2);
        }
    }
}