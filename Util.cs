using System.Globalization;


namespace ConsoleApp1
{
    public static class Util
    {


        public static double DoubleParser(string v) => double.Parse(v, CultureInfo.InvariantCulture);
        public static int IntParser(string v) => int.Parse(v, CultureInfo.InvariantCulture);



        public static bool IsLeft(double x, double y, 
            double p1x, double p1y, 
            double p2x, double p2y)
        {
            return ((p2x - p1x) * (y - p1y) - (x - p1x) * (p2y - p1y)) > 0;
        }


        public static double DistSq(double x1, double x2, double y1, double y2)
        {
            double diffx = x1 - x2;
            double diffy = y1 - y2;
            return diffx * diffx + diffy * diffy;
        }

        public static bool IsInTriangle(double x, double y, 
            double p1x, double p1y, 
            double p2x, double p2y, 
            double p3x, double p3y)
        {
            // generirao chat gpt
            // Compute the Barycentric coordinates
            double denominator = (p2y - p3y) * (p1x - p3x) + (p3x - p2x) * (p1y - p3y);
            double alpha = ((p2y - p3y) * (x - p3x) + (p3x - p2x) * (y - p3y)) / denominator;
            double beta = ((p3y - p1y) * (x - p3x) + (p1x - p3x) * (y - p3y)) / denominator;
            double gamma = 1.0 - alpha - beta;

            // Check if the point is inside the triangle (including points on the edges)
            return alpha >= 0 && alpha <= 1 && beta >= 0 && beta <= 1 && gamma >= 0 && gamma <= 1;
        }



        public static (double, double) Rotate(double[,] st, double x, double y)
        {
            double tx = st[0, 0] * x + st[0, 1] * y;
            double ty = st[1, 0] * x + st[1, 1] * y;
            return (tx, ty);
        }


        public static void StAvg(double[,] q1, double[,] q2, double[,] r)
        {
            r[0, 0] = q1[0, 0] + q2[0, 0];
            r[0, 1] = q1[0, 1] + q2[0, 1];
            r[1, 0] = q1[1, 0] + q2[1, 0];
            r[1, 1] = q1[1, 1] + q2[1, 1];

            double n1 = Math.Sqrt(r[0, 0] * r[0, 0] + r[0, 1] * r[0, 1]);
            double n2 = Math.Sqrt(r[1, 0] * r[1, 0] + r[1, 1] * r[1, 1]);

            r[0, 0] /= n1;
            r[0, 1] /= n1;
            r[1, 0] /= n2;
            r[1, 1] /= n2;
        }


    }
}
