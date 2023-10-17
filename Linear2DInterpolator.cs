using static ConsoleApp1.Util;


namespace ConsoleApp1
{
    public class Linear2DInterpolator : Interpolator2d
    {

        private const double EPS = 1e-8;

        private readonly double[,] points;
        private readonly alglib.kdtree kdt;
        private readonly double[] coefs;
        private readonly int[] convex_hull;
        private readonly double[,] eqns;
        private readonly int[,] neighbors;
        private readonly List<int[]> vts;
        private readonly int[,] simplices;

        private readonly double xmin;
        private readonly double xmax;
        private readonly double ymin;
        private readonly double ymax;


        private readonly double[] qbuf = new double[2];
        private double[,] rbuf = new double[2, 1];
        private double[] distbuf = new double[1];
        private int[] tagbuf = new int[1];
        


        public Linear2DInterpolator(string dir, string dataFile)
        {

            // ucitaj tocke koristene za interpolaciju
            points = Loader.LoadData2D(dataFile, DoubleParser, Constants.DELIMITER);
            
            // ucitaj K-D stablo
            kdt = Loader.LoadKDTree(Path.Join(dir, "kdtree.bin"));

            // ucitaj regresijske koeficijente
            coefs = Loader.LoadData1D(Path.Join(dir, "coefs.txt"), DoubleParser, Constants.DELIMITER);

            // ucitaj konveksknu ljusku
            convex_hull = Loader.LoadData1D(Path.Join(dir, "convex_hull.txt"), IntParser, Constants.DELIMITER);

            // ucitaj jednadzbe ravnine za svaki trokut
            eqns = Loader.LoadData2D(Path.Join(dir, "eqns.txt"), DoubleParser, Constants.DELIMITER);

            // ucitaj matricu sa susjedima svih trokut
            neighbors = Loader.LoadData2D(Path.Join(dir, "neighbors.txt"), IntParser, Constants.DELIMITER);

            // ucitaj listu vrh -> svi trokuti koji imaju taj vrh
            vts = Loader.LoadVts(Path.Join(dir, "vertex_to_simplices.txt"), IntParser, Constants.DELIMITER);

            // ucitaj sve trokutove (simplexi)
            simplices = Loader.LoadData2D(Path.Join(dir, "simplices.txt"), IntParser, Constants.DELIMITER);



            // nadji min i max u obje dimenzije
            xmin = double.PositiveInfinity;
            xmax = double.NegativeInfinity;
            ymin = double.PositiveInfinity;
            ymax = double.NegativeInfinity;
            for (int i=0; i<points.GetLength(0); i++)
            {
                xmin = Math.Min(xmin, points[i, 0]);
                xmax = Math.Max(xmax, points[i, 0]);
                ymin = Math.Min(ymin, points[i, 1]);
                ymax = Math.Max(ymax, points[i, 1]);
            }
        }

        private double Extrapolate(double x, double y)
        {
            // regresijskom plohom odredi vrijednost
            return coefs[0] + x * coefs[1] + y * coefs[2] + x * x * coefs[3] + x * y * coefs[4] + y * y * coefs[5];
        }


        private bool CheckInConvexHull(double x, double y)
        {
            // uvijek ide u smjeru suprotnom od kazaljke na satu

            for (int i=0; i<convex_hull.Length; i++)
            {
                double p1x = points[convex_hull[i], 0];
                double p1y = points[convex_hull[i], 1];
                double p2x = points[convex_hull[(i + 1) % convex_hull.Length], 0];
                double p2y = points[convex_hull[(i + 1) % convex_hull.Length], 1];

                if (!IsLeft(x, y, p1x, p1y, p2x, p2y)) return false;
            }
            return true;
        }

        

        private double SumSqDistances(double x, double y, int simplex)
        {
            int v1 = simplices[simplex, 0];
            int v2 = simplices[simplex, 1];
            int v3 = simplices[simplex, 2];

            double p1x = points[v1, 0];
            double p1y = points[v1, 1];
            double p2x = points[v2, 0];
            double p2y = points[v2, 1];
            double p3x = points[v3, 0];
            double p3y = points[v3, 1];

            return DistSq(x, p1x, y, p1y) + DistSq(x, p2x, y, p2y) + DistSq(x, p3x, y, p3y);
        }


        public double Interpolate(double x, double y)
        {
            if (x < xmin || x > xmax || y < ymin || y > ymax)
            {
                return Extrapolate(x, y);
            }

            // provjeri je li u konveksnoj ljusci
            if (!CheckInConvexHull(x, y))
            {
                return Extrapolate(x, y);
            }


            // odredi najbliži vrh
            qbuf[0] = x;
            qbuf[1] = y;
            alglib.kdtreequeryknn(kdt, qbuf, 1);
            alglib.kdtreequeryresultsx(kdt, ref rbuf);
            alglib.kdtreequeryresultstags(kdt, ref tagbuf);
            alglib.kdtreequeryresultsdistances(kdt, ref distbuf);

            // ako je udaljenost od najbližeg vrha manja od epsilon onda kao vrijednost uzmi vrijednost tog vrha
            int closestVertex = tagbuf[0];
            if (distbuf[0] <= EPS)
            {
                return points[closestVertex, 2];
            }


            // pronađi inicijalni trokut
            // prvo pronađi sve trokutove koji sadrže vrh closestVertex
            int[] simplicesVertex = vts[closestVertex];

            // kao inicijalni odabir uzmi onaj trokut čija suma udaljenosti od vrhova je najmanja
            int initialSimplex = simplicesVertex.MinBy(s => SumSqDistances(x,y,s));


            // započni pretragu u širinu (BFS) dok ne nađeš trokut u kojem se nalazi točka x,y
            Queue<int> q = new();
            q.Enqueue(initialSimplex);
            HashSet<int> visited = new();

            while (q.Count > 0)
            {
                int simplex = q.Dequeue();

                

                int v1 = simplices[simplex, 0];
                int v2 = simplices[simplex, 1];
                int v3 = simplices[simplex, 2];

                double p1x = points[v1, 0];
                double p1y = points[v1, 1];
                double p2x = points[v2, 0];
                double p2y = points[v2, 1];
                double p3x = points[v3, 0];
                double p3y = points[v3, 1];

                if (IsInTriangle(x, y, p1x, p1y, p2x, p2y, p3x, p3y))
                {
                    double interpolated = eqns[simplex, 0] + eqns[simplex, 1] * x + eqns[simplex, 2] * y;
                    return interpolated;
                }

                visited.Add(simplex);

                for (int j=0; j<3; j++)
                {
                    int neighborSimplex = neighbors[simplex, j];
                    if (neighborSimplex == -1 || visited.Contains(neighborSimplex)) continue;

                    q.Enqueue(neighborSimplex);

                }

            }



            // do ovog ne bi trebalo doći
            Console.WriteLine($"Warning: inside convex hull but couldn't find a proper simplex for point ({x}, {y})");
            return Extrapolate(x, y);
        }
    }
}
