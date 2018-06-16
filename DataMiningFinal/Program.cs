using MatFileHandler;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathWorks.MATLAB.NET.Arrays;
using MatlabUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DataMiningFinal
{
    class Program
    {
        public static MatlabMethods MatlabMethods;
        public static Thread initThread;
        static void Main(string[] args)
        {
            initThread = new Thread(() =>
            {
                MatlabMethods = new MatlabMethods();
                Console.WriteLine("ClusteringMeasure Inited!");
            });
            initThread.Start();

            //ArtificialDataset("Spiral", 3);
            //ArtificialDataset("Pathbased", 3);
            //ArtificialDataset("Jain", 2);
            //ArtificialDataset("Flame", 2);
            //ArtificialDataset("Aggregation", 7);

            //MfeatBySingleView("data_fac");
            //MfeatBySingleView("data_fou");
            //MfeatBySingleView("data_kar");
            //MfeatBySingleView("data_mor");
            //MfeatBySingleView("data_pix");
            //MfeatBySingleView("data_zer");
            //MfeatByMultiView();

            University("cornell");
            University("texas");
            University("washington");
            University("wisconsin");

            //OptDigits();
        }

        private static int[] GetLabels(DataPoint[] dataPoints)
        {
            int[] ans = new int[dataPoints.Length];
            for (int i = 0; i < dataPoints.Length; i++)
            {
                ans[i] = dataPoints[i].clusterID is null ? -1 : (int)dataPoints[i].clusterID;
            }
            return ans;
        }

        private static void OptDigits()
        {
            var dataList = new List<DataPoint>();
            var ansList = new List<int>();
            using (StreamReader sr = new StreamReader(@"C:\Users\cuber\Desktop\OptDigits\optdigits-orig.tra"))
            {
                string line = "";
                for (int i = 0; i < 21; i++)
                {
                    line = sr.ReadLine();
                }

                while (!sr.EndOfStream)
                {
                    var cur = new List<double>();
                    for (int i = 0; i < 32; i++)
                    {
                        line = sr.ReadLine();
                        for (int j = 0; j < 32; j++)
                        {
                            cur.Add(double.Parse(line[j].ToString()));
                        }
                    }
                    dataList.Add(new DataPoint(cur.ToArray()));
                    ansList.Add(int.Parse(sr.ReadLine()));
                }
            }

            var data = dataList.ToArray();
            var ans = ansList.ToArray();

            DensityPeak dp = new DensityPeak(10, data);
            dp.Clustering();

            Measure(ans: ans, myans: GetLabels(dp.DataPoints), name: "OptDigits");
        }

        private static void ArtificialDataset(string name, int k)
        {
            StreamReader sr = new StreamReader(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\" + name + ".txt");
            var data = ParseData(sr.ReadToEnd());

            DensityPeak dp = new DensityPeak(k, data);
            dp.Clustering();

            sr.Close();
            sr = new StreamReader(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\anses\" + name + ".txt");

            Measure(ans: ParseAnswer(sr.ReadToEnd()), myans: GetLabels(dp.DataPoints), name: name);
            //WriteAns(dp);
        }

        private static void University(string universityName)
        {
            List<DensityPeak> views = new List<DensityPeak>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab2\实验2\数据集\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            views.Add(new DensityPeak(5, ParseData(matFile["A"])));
            views.Add(new DensityPeak(5, ParseData(matFile["F"])));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(views.ToArray());
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(mvdp.Clustering()), name: universityName);
        }

        private static void MfeatByMultiView()
        {
            List<DensityPeak> views = new List<DensityPeak>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\Mfeat.mat", FileMode.Open))).Read();
            //views.Add(new DensityPeak(10, ParseData(matFile["data_fac"])));
            views.Add(new DensityPeak(10, ParseData(matFile["data_fou"])));
            //views.Add(new DensityPeak(10, ParseData(matFile["data_kar"])));
            //views.Add(new DensityPeak(10, ParseData(matFile["data_mor"])));
            views.Add(new DensityPeak(10, ParseData(matFile["data_pix"])));
            //views.Add(new DensityPeak(10, ParseData(matFile["data_zer"])));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(views.ToArray());
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(mvdp.Clustering()), name: "Mfeat");
        }

        private static void MfeatBySingleView(string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\Mfeat.mat", FileMode.Open))).Read();

            DensityPeak dp = new DensityPeak(10, ParseData(matFile[entry]));
            dp.Clustering();

            Console.WriteLine("Single by " + entry);
            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(dp.DataPoints), name: "Mfeat");
            //WriteAns(densityPeak);
        }

        private static int[] GetLabels(IVariable variable)
        {
            var raw = variable.Value.ConvertToDoubleArray();
            var ans = new int[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                ans[i] = (int)raw[i];
            }
            return ans;
        }

        private static void Measure(int[] ans, int[] myans, string name)
        {
            initThread.Join();
            Console.ForegroundColor = ConsoleColor.Green;
            var resultObj = MatlabMethods.ClusteringMeasure(1, new MWNumericArray(ans as Array), new MWNumericArray(myans as Array));
            var result = resultObj.First().ToArray() as double[,];
            Console.WriteLine("For dataset " + name + ":\nACC = {0}\nNMI = {1}\nPUR = {2}\n", result[0, 0], result[0, 1], result[0, 2]);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static int[] ParseAnswer(string rawData)
        {
            List<int> ans = new List<int>();
            var allraw = rawData.Split(' ');
            foreach (var raw in allraw)
            {
                ans.Add(int.Parse(raw));
            }
            return ans.ToArray();
        }

        private static DataPoint[] ParseData(string rawData)
        {
            List<DataPoint> dataPoints = new List<DataPoint>();

            var allraw = rawData.Split("\r\n".ToArray());
            foreach (var rawline in allraw)
            {
                if (rawline == "")
                    continue;
                var raw = rawline.Split('\t');
                dataPoints.Add(new DataPoint(new double[] { double.Parse(raw.First()), double.Parse(raw.Last()) }));
            }

            return dataPoints.ToArray();
        }

        private static DataPoint[] ParseData(IVariable variable)
        {
            double[] raw = variable.Value.ConvertToDoubleArray();
            int nor = variable.Value.Dimensions.First();
            int noc = variable.Value.Dimensions.Last();
            DataPoint[] ans = new DataPoint[nor];
            for (int i = 0; i < nor; i++)
            {
                var temp = new List<double>();
                for (int j = 0; j < noc; j++)
                {
                    temp.Add(raw[i * noc + j]);
                }
                ans[i] = new DataPoint(temp.ToArray());
            }

            return ans;
        }

        private static void WriteAns(DensityPeak densityPeak)
        {
            using (StreamWriter sw = new StreamWriter(@"C:\Users\cuber\Desktop\ans.txt"))
            {
                foreach (var dp in densityPeak.DataPoints)
                {
                    sw.WriteLine(dp.clusterID);
                }
            }
        }
    }
}
