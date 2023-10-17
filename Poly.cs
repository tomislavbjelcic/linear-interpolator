


using MB.Algodat;
using System.Runtime.CompilerServices;

namespace ConsoleApp1
{
    public class Poly
    {

        private const double EPS = 1e-12;


        private readonly double[,] points;

        private readonly int coord;
        private readonly int n;
        private readonly double xmin;
        private readonly double xmax;
        private readonly double ymin;
        private readonly double ymax;

        private readonly Dictionary<int, bool> sides = new();
        private readonly RangeTree<double, int> intervalTree = new();




        public Poly(double[,] points, int coord)
        {
            this.points = points;
            this.coord = coord;
            n = points.GetLength(0);


            double xmax = double.NegativeInfinity;
            double xmin = double.PositiveInfinity;
            double ymax = double.NegativeInfinity;
            double ymin = double.PositiveInfinity;
            for (int i = 0; i < n; i++)
            {
                xmax = Math.Max(xmax, points[i, 0]);
                xmin = Math.Min(xmin, points[i, 0]);
                ymax = Math.Max(ymax, points[i, 1]);
                ymin = Math.Min(ymin, points[i, 1]);
            }
            this.xmin = xmin;
            this.xmax = xmax;
            this.ymin = ymin;
            this.ymax = ymax;



            for (int i=0; i<n; i++)
            {
                double v1 = points[i, coord];
                double v2 = points[(i+1)%n, coord];

                if (v1 == v2)
                {
                    double v1o = points[i, 1 - coord];
                    double v2o = points[(i+1)%n, 1 - coord];
                    sides.Add(i, v1o > v2o);
                    continue;
                }

                double vmin = Math.Min(v1, v2);
                double vmax = Math.Max(v1, v2);
                intervalTree.Add(vmin, vmax, i);
            }

            intervalTree.Rebuild();
        }




        public bool Contains(double x, double y)
        {

            if (x < xmin || x > xmax || y < ymin || y > ymax)
                return false;


            double qv = coord == 0 ? x : y;
            double qv_other = coord == 0 ? y : x;
            var query_result = intervalTree[qv];

            HashSet<int> skip = new();

            bool inside = false;

            foreach (var qr in query_result)
            {
                int idx = qr.Value;
                if (skip.Contains(idx))
                    continue;

                double v1 = points[idx, coord];
                double v2 = points[(idx + 1) % n, coord];
                double vmin = Math.Min(v1, v2);
                double vmax = Math.Max(v1, v2);
                double v1o = points[idx, 1-coord];
                double v2o = points[(idx + 1) % n, 1-coord];

                if (qv_other > Math.Max(v1o, v2o))
                    // zraka prema desno, neće sjeći segment
                    continue;


                if (qv < vmin || qv > vmax)
                    continue;


                if (qv == v1 || qv == v2)
                {
                    // rub intervala
                    // pronadji susjedni segment
                    double p1x = points[idx, 0];
                    double p1y = points[idx, 1];
                    double p2x = points[(idx + 1) % n, 0];
                    double p2y = points[(idx + 1) % n, 1];
                    
                    bool forward = (qv == v2);
                    int neighbor_point_idx;
                    int neighbor_segment_idx;
                    if (forward)
                    {
                        // susjedni segment je na sljedecem indeksu
                        neighbor_point_idx = (idx + 2) % n;
                        neighbor_segment_idx = (idx + 1) % n;

                        // zamijeni p1 i p2
                        (p2x, p1x) = (p1x, p2x);
                        (p2y, p1y) = (p1y, p2y);
                    } else
                    {
                        neighbor_point_idx = (idx + n - 1) % n;
                        neighbor_segment_idx = neighbor_point_idx;
                    }

                    skip.Add(neighbor_segment_idx);
                    if (qv_other > (coord == 0 ? p1y : p1x)) continue;

                    double p3x = points[neighbor_point_idx, 0];
                    double p3y = points[neighbor_point_idx, 1];
                    double vax = p2x - p1x;
                    double vay = p2y - p1y;
                    double vbx = p3x - p1x;
                    double vby = p3y - p1y;
                    double vqx = x - p1x;
                    double vqy = y - p1y;
                    if ((vqx * vqx + vqy * vqy) < EPS * EPS) return true;


                    double cpa = vax * vqy - vay * vqx;
                    double cpb = vbx * vqy - vby * vqx;
                    bool change;
                    if (cpa==0.0 || cpb==0.0) {
                        double voc = cpa == 0.0 ? (coord == 0 ? vbx : vby) : (coord == 0 ? vax : vay);
                        change = (voc > 0) == sides[neighbor_segment_idx];
                    } else
                    {
                        change = (cpa * cpb) <= 0;
                    }


                    if (change)
                    {
                        inside = !inside;
                    }
                    continue;

                }




                double intersection = v1o + (v2o - v1o) * (qv - v1) / (v2 - v1);
                if (Math.Abs(intersection - qv_other) < EPS) return true;

                if (intersection >= qv_other)
                {
                    inside = !inside;
                }
            }
            
            
            return inside;
        }



    }
}
