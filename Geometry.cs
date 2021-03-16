using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Crusade
{
    public static class Geometry
    {                 

        /// <returns> Value of the determinant :
        ///    <para>| p1.X p1.Y 1 |</para>
        ///    <para>| p2.X p2.Y 1 |</para>
        ///    <para>| p3.X p3.Y 1 |</para>
        /// </returns>
        static double Det3P(Point p1, Point p2, Point p3) => (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);

        /// <returns> The absolute value of Det3P(<paramref name="p1"/>,<paramref name="p2"/>,<paramref name="p3"/>)</returns>
        static double AbsDet3P(Point p1, Point p2, Point p3) => Math.Abs(Det3P(p1, p2, p3));       

        /// <summary>
        /// A class helper for operating with segments.       
        /// </summary>
        public class Segment
        {            
            public Point P1, P2;
            public Segment(Point p1, Point p2)
            {
                P1 = p1;
                P2 = p2;                
            }

            public bool Intersects(Segment s) =>
                Det3P(P1, P2, s.P1) * Det3P(P1, P2, s.P2) <= 0 &&
                Det3P(s.P1, s.P2, P1) * Det3P(s.P1, s.P2, P2) <= 0;
            public bool IntersectsNoExtremities(Segment s) =>
                Det3P(P1, P2, s.P1) * Det3P(P1, P2, s.P2) < 0 &&
                Det3P(s.P1, s.P2, P1) * Det3P(s.P1, s.P2, P2) < 0;
        }                                      
    }
}
