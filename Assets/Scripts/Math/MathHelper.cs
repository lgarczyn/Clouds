#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;

namespace UnityEngine
{

  /// <summary>
  /// Math helpers.
  /// </summary>
  public static class MathHelper
  {
    #region Const

    /// <summary>
    /// Degrees to radian constant.
    /// </summary>
    public const double Deg2Rad = Math.PI / 180.0;

    /// <summary>
    /// Radians to degrees constant.
    /// </summary>
    public const double Rad2Deg = 180.0 / Math.PI;
    #endregion

    #region Clamping
    /// <summary>
    /// Clamps a value between a minimum and a maximum value.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(double value, double min, double max)
    {
      return (value >= min ? (value <= max ? value : max) : min);
    }

    /// <summary>
    /// Clamps a value between 0 and 1.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp01(double value)
    {
      return (value >= 0 ? (value <= 1 ? value : 1) : 0);
    }
    #endregion

    #region Compare


    /// <summary>
    /// Compares two double precision values and returns true if they are similar.
    /// </summary>
    /// <param name="epsilon">
    /// The smallest acceptable difference
    /// </param>
    public static bool Approximately(this double a, double b, double epsilon = Double.Epsilon)
    {
      // If a or b is zero, compare that the other is less or equal to epsilon.
      // If neither a or b are 0, then find an epsilon that is good for
      // comparing numbers at the maximum magnitude of a and b.
      // Floating points have about 7 significant digits, so
      // 1.000001f can be represented while 1.0000001f is rounded to zero,
      // thus we could use an epsilon of 0.000001f for comparing values close to 1.
      // We multiply this epsilon by the biggest magnitude of a and b.

      // TODO: add back larger Epsilon instead of first Double.epsilon
      // Removed because hard to find equivalent of 0.000001f for double
      // TODO: check soundness of whole mess
      return Math.Abs(b - a) < Math.Max(epsilon * Math.Max(Math.Abs(a), Math.Abs(b)), epsilon * 8);
    }


    #endregion
  }
}