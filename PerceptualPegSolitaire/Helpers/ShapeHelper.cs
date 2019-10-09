//ShapeHelper.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-14
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PerceptualPegSolitaire.Helpers
{
    class ShapeHelper
    {
        #region Methods

        public static bool IsPointInsideRect(Point p, Rect rect)
        {
            if (p.X >= rect.Left && p.X <= rect.Right)
            {
                if (p.Y >= rect.Top && p.Y <= rect.Bottom)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPointInsideRect(double x, double y, Rect rect)
        {
            return IsPointInsideRect(new Point(x, y), rect);
        }

        public static Point RotatePoint(Point p, Point origin, double angle)
        {
            Point rotated = new Point();
            rotated.X = Math.Cos(angle) * (p.X - origin.X) - Math.Sin(angle) * (p.Y - origin.Y) + origin.X;
            rotated.Y = Math.Sin(angle) * (p.X - origin.X) + Math.Cos(angle) * (p.Y - origin.Y) + origin.Y;
            return rotated;
        }

        #endregion
    }
}
