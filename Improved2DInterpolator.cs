using static ConsoleApp1.Util;

namespace ConsoleApp1
{
    public class Improved2DInterpolator : Interpolator2d
    {


        private readonly double[] ods;
        private readonly Poly convexHull;
        private readonly double[] coefs;
        private readonly double[,] coefsBetween;

        private readonly ODInfo[] odinfos;


        private readonly double[] scoresBuf;
        private readonly double[,] rBuf = new double[2, 2];


        public Improved2DInterpolator(string dir)
        {

            // ucitaj skup svih poznatih OD-ova
            ods = Loader.LoadData1D(Path.Join(dir, "ods.txt"), DoubleParser, Constants.DELIMITER);


            // ucitaj tocke koje su konveksna ljuska cijelog skupa
            double[,] convexHullPoints = Loader.LoadData2D(Path.Join(dir, "convex_hull.txt"), DoubleParser, Constants.DELIMITER);
            convexHull = new(convexHullPoints, 0);


            // ucitaj regresijske koeficijente cijelog skupa
            coefs = Loader.LoadData1D(Path.Join(dir, "coefs.txt"), DoubleParser, Constants.DELIMITER);


            // ucitaj regresijske koeficijente između platoa OD-ova
            coefsBetween = Loader.LoadData2D(Path.Join(dir, "coefs_between.txt"), DoubleParser, Constants.DELIMITER);



            odinfos = new ODInfo[ods.Length];
            for (int i=0; i<odinfos.Length; i++)
            {
                odinfos[i] = new(Path.Join(dir, $"od_{i}"));
            }



            scoresBuf = new double[ods.Length];

        }


        private double Extrapolate(double x, double y)
        {
            // regresijskom plohom odredi vrijednost
            return coefs[0] + x * coefs[1] + y * coefs[2] + x * x * coefs[3] + x * y * coefs[4] + y * y * coefs[5];
        }


        private void CalculateScoresBuf(double x, double y)
        {
            for (int i=0; i<scoresBuf.Length; i++)
            {
                scoresBuf[i] = odinfos[i].Score(x, y);
            }
        }


        public double Interpolate(double x, double y)
        {
            // provjeri je li u konveksnoj ljusci
            if (!convexHull.Contains(x, y))
            {
                return Extrapolate(x, y);
            }

            CalculateScoresBuf(x, y);
            if (scoresBuf[0] <= 0)
            {
                // provjeri jel u ljusci
                if (odinfos[0].Contains(x, y))
                {
                    return ods[0];
                }
            }

            for (int i=0; i<ods.Length-1; i++)
            {

                if (scoresBuf[i] >= 0 && scoresBuf[i+1] <= 0)
                {
                    bool c1 = odinfos[i].Contains(x, y);
                    bool c2 = odinfos[i + 1].Contains(x, y);
                    if (c1 && c2) return (ods[i] + ods[i + 1]) / 2;
                    if (c1) return ods[i];
                    if (c2) return ods[i + 1];

                    // onda je izmedju
                    StAvg(odinfos[i].St, odinfos[i+1].St, rBuf);
                    (double tx, double ty) = Rotate(rBuf, x, y);
                    double val = coefsBetween[i, 0]
                        + tx * coefsBetween[i, 1]
                        + ty * coefsBetween[i, 2]
                        + tx * tx * coefsBetween[i, 3];


                    return val;
                }

            }

            // mozda je zadnji
            if (odinfos[odinfos.Length-1].Contains(x, y))
            {
                return ods[odinfos.Length-1];
            }


            // onda je negdje na cudnoj poziciji
            return Extrapolate(x, y);
        }
    }
}
