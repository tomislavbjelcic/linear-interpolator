using static ConsoleApp1.Util;

namespace ConsoleApp1
{
    public class ODInfo
    {


        private readonly double[,] st;
        private readonly Poly hull;
        private readonly double[] coefs;

        public double[,] St
        {
            get { return st; }
        }


        public ODInfo(string dir)
        {
            // ucitaj transponiranu matricu prijelaza rotacije
            st = Loader.LoadData2D(Path.Join(dir, "st.txt"), DoubleParser, Constants.DELIMITER);


            // ucitaj konkavnu ljusku, rangeTree radi po y koordinati
            double[,] hullPoints = Loader.LoadData2D(Path.Join(dir, "hull.txt"), DoubleParser, Constants.DELIMITER);
            hull = new(hullPoints, coord: 1);


            // ucitaj koeficijente
            coefs = Loader.LoadData1D(Path.Join(dir, "coefs.txt"), DoubleParser, Constants.DELIMITER);
        }


        public bool Contains(double x, double y) => hull.Contains(x, y);


        public double Score(double x, double y)
        {

            (double tx, double ty) = Rotate(st, x, y);

            double v = ty - (coefs[0] + coefs[1] * tx + coefs[2] * tx * tx);
            
            
            return v;
        }




    }
}
