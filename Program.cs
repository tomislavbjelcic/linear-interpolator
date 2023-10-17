
using ConsoleApp1;
using System.Text;
using System.Globalization;

class Program
{



    public static void Main(string[] args)
    {

        //string dir = @"C:\Users\tomislav.bjelcic\Desktop\ehkaze\oddata\normal";
        //Interpolator2d interpolator = new Improved2DInterpolator(dir);


        //while (true)
        //{
        //    string line = Console.ReadLine()!;
        //    if (line.Trim().Length == 0) break;

        //    string[] splitted = line.Split();
        //    double x = double.Parse(splitted[0], CultureInfo.InvariantCulture);
        //    double y = double.Parse(splitted[1], CultureInfo.InvariantCulture);
        //    Pq(interpolator, x, y);
        //}

        // VerifyResults();

        Err(@"C:\Users\tomislav.bjelcic\Desktop\ehkaze\oddata\normal", 
            @"C:\Users\tomislav.bjelcic\Desktop\rcca\RCCA_03\lut_1_5.txt");



    }


    private static void VerifyResults()
    {
        string dir = @"C:\Users\tomislav.bjelcic\Desktop\ehkaze\oddata\ukosen";
        string testFile = @"C:\Users\tomislav.bjelcic\Desktop\ehkaze\aeee.txt";


        Interpolator2d model = new Improved2DInterpolator(dir);


        double[,] testData = Loader.LoadData2D(testFile, Util.DoubleParser, Constants.DELIMITER);
        int nTestData = testData.GetLength(0);
        double[,] testDataExtraColumn = new double[nTestData, 3];
        Console.WriteLine(nTestData);

        for (int i = 0; i < nTestData; i++)
        {
            double x = testData[i, 0];
            double y = testData[i, 1];

            double z = model.Interpolate(x, y);

            testDataExtraColumn[i, 0] = x;
            testDataExtraColumn[i, 1] = y;
            testDataExtraColumn[i, 2] = z;
        }

        Console.WriteLine("Done, saving...");
        string outfile = @"C:\Users\tomislav.bjelcic\Desktop\ehkaze\testset_out.txt";
        SaveDataDouble2D(outfile, testDataExtraColumn);
    }


    private static void SaveDataDouble2D(string fileName, double[,] data, string delimiter = Constants.DELIMITER)
    {
        StringBuilder sb = new();

        using (StreamWriter sw = new(File.OpenWrite(fileName)))
        {
            for (int i = 0; i< data.GetLength(0); i++)
            {
                
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    sb.Append(data[i, j].ToString(CultureInfo.InvariantCulture));
                    if (j < data.GetLength(1)-1)
                    {
                        sb.Append(delimiter);
                    }
                }

                sw.WriteLine(sb.ToString());
                sb.Clear();
                
            }
        }

    }

    

    public static void MockDemo()
    {
        string dir = @"C:\Users\tomislav.bjelcic\Desktop\ehkaze\fejkdata";
        string dataFile = Path.Join(dir, "data.txt");

        Linear2DInterpolator itp = new(dir, dataFile);
        Pq(itp, 0, 0);
        Pq(itp, 0.6, 0);
        Pq(itp, 1, 0);
        Pq(itp, 0.3, -0.4);
        Pq(itp, 0.5, 1.0);
        Console.WriteLine();
        Pq(itp, -3.4, 1.4);
        Pq(itp, 10, 10);
        Pq(itp, 0.1, 0.3); // outside convex hull but inside min max bounds
        Console.WriteLine();
        Pq(itp, 0.3, 0.0); // should be close to 0
        Pq(itp, 0.3, 0.55); // should be close to 0.55
        Pq(itp, 0.4, 0.7); // should be close to 0.7
        Pq(itp, 0.5, 0.9); // should be close to 0.9
    }

    private static void Pq(Interpolator2d interpolator, double x, double y)
    {
        double z = interpolator.Interpolate(x, y);
        Console.WriteLine($"{x}\t{y}\t->\t{z}");
    }


    private static void Pq(Poly poly, double x, double y)
    {
        string tf = poly.Contains(x, y) ? "VVV" : "---";
        Console.WriteLine($"{x}\t{y}\t->\t{tf}");
    }



    private static void Err(string dir, string dataFile)
    {
        double[,] testData = Loader.LoadData2D(dataFile, Util.DoubleParser, Constants.DELIMITER);

        Interpolator2d model = new Improved2DInterpolator(dir);
        int nTestData = testData.GetLength(0);
        double errSum = 0.0;

        for (int i = 0; i < nTestData; i++)
        {
            double x = testData[i, 0];
            double y = testData[i, 1];
            double z = testData[i, 2];

            double z_calculated = model.Interpolate(x, y);
            errSum += Math.Abs(z - z_calculated);

        }

        Console.WriteLine(errSum);

    }




}

