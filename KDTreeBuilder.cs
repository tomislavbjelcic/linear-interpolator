
using System.Diagnostics;

namespace ConsoleApp1
{
    public static class KDTreeBuilder
    {

        public static void KDTreeBuildMain(string[] args)
        {
            // program.ext fileName dir
            if (args.Length < 2)
            {
                Console.WriteLine("Expected 2 arguments: data file and directory.");
                return;
            }
            string fileName = args[0];
            string dir = args[1];
            double[,] points = Loader.LoadData2D(fileName, Util.DoubleParser, Constants.DELIMITER);
            int[] omitted = Loader.LoadData1D(Path.Join(dir, "omitted.txt"), Util.IntParser, Constants.DELIMITER);

            // sagradi K-D stablo i spremi ga na disk
            // u redu je slati Nx3 matricu jer će kod gradnje se ignorirati treći stupac
            alglib.kdtree kdt = KDTreeBuild(points, omitted);
            string outfile = Path.Join(dir, "kdtree.bin");
            using (FileStream fs = File.OpenWrite(outfile))
            {
                alglib.kdtreeserialize(kdt, fs);
            }

            Console.WriteLine($"Done writing to {outfile}");

        }


        private static alglib.kdtree KDTreeBuild(double[,] points, int[] omitted)
        {

            int nInputPoints = points.GetLength(0);
            int nOmitted = omitted.Length;
            Console.WriteLine($"Input points: {nInputPoints}");
            HashSet<int> omittedSet = new(omitted);
            Debug.Assert(nOmitted == omittedSet.Count);
            
            
            
            int[] tags = new int[nInputPoints - nOmitted];

            int j = 0;
            for (int i = 0; i < nInputPoints; i++)
            {
                if (omittedSet.Contains(i))
                    continue;

                tags[j++] = i;
            }

            int nx = 2;
            int ny = 0;
            int normtype = 2;

            double[,] finalPoints = points;

            if (nOmitted > 0)
            {
                // postoje odbacene tocke, slozi novo polje points
                int nCols = points.GetLength(1);
                double[,] newpoints = new double[nInputPoints - nOmitted, nCols];

                j = 0;
                for (int i = 0; i < nInputPoints; i++)
                {
                    if (omittedSet.Contains(i))
                        continue;

                    for (int k = 0; k < nCols; k++)
                    {
                        newpoints[j, k] = points[i, k];
                    }

                    j++;
                }

                finalPoints = newpoints;
            }

            

            alglib.kdtreebuildtagged(finalPoints, tags, nx, ny, normtype, out alglib.kdtree kdt);
            Console.WriteLine($"Successfully built K-D tree with {finalPoints.GetLength(0)} input points.");
            return kdt;
        }



    }
}
