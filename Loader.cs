

namespace ConsoleApp1
{
    public static class Loader
    {


        public static alglib.kdtree LoadKDTree(string fileName)
        {
            using FileStream fs = File.OpenRead(fileName);
            alglib.kdtreeunserialize(fs, out alglib.kdtree kdt);
            return kdt;
        }


        public static List<int[]> LoadVts(string fileName, Func<string, int> parser, string delimiter)
        {
            List<int[]> vts = new();

            using (StreamReader sr = File.OpenText(fileName))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()!.Trim();
                    if (line.Length == 0)
                    {
                        vts.Add(Array.Empty<int>());
                        continue;
                    }

                    string[] splitted = line.Split(delimiter);
                    int[] v = new int[splitted.Length];
                    for (int i=0; i<splitted.Length; i++)
                    {
                        v[i] = parser(splitted[i]);
                    }

                    vts.Add(v);
                }
            }
            return vts;
        }

        public static T[,] LoadData2D<T>(string fileName, Func<string, T> parser, string delimiter)
        {
            int lines = File.ReadLines(fileName).Count();
            T[,] data = new T[0, 0];
            if (lines  == 0)
            {
                return data;
            }

            
            int columns = -1;

            using (StreamReader sr = File.OpenText(fileName))
            {
                for (int i = 0; i < lines; i++)
                {
                    string line = sr.ReadLine()!;
                    string[] splitted = line.Split(delimiter);
                    if (columns == -1)
                    {
                        columns = splitted.Length;
                        data = new T[lines, columns];
                    }
                    if (columns != splitted.Length)
                    {
                        throw new SystemException($"Line \"{line}\" has {splitted.Length} values but expected {columns}.");
                    }

                    for (int j = 0; j < columns; j++)
                    {
                        data[i, j] = parser(splitted[j]);
                    }
                }
            }

            return data;
        }


        public static T[] LoadData1D<T>(string fileName, Func<string, T> parser, string delimiter)
        {
            string[] lines = File.ReadAllLines(fileName);
            if (lines.Length == 0)
            {
                return Array.Empty<T>();
            }

            if (lines.Length > 1)
            {
                throw new SystemException($"Read {lines.Length} lines but expected 1.");
            }

            string line = lines[0];
            string[] splitted = line.Split(delimiter);
            T[] data = new T[splitted.Length];
            for (int i = 0; i < splitted.Length; i++)
            {
                data[i] = parser(splitted[i]);
            }
            return data;
        }
    }
}
