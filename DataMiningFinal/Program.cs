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

            //eu
            ArtificialDataset("Spiral", 3);
            ArtificialDataset("Pathbased", 3);
            ArtificialDataset("Jain", 2);
            ArtificialDataset("Flame", 2);
            ArtificialDataset("Aggregation", 7);

            MfeatBySingleView("data_fac");
            MfeatBySingleView("data_fou");
            MfeatBySingleView("data_kar");
            MfeatBySingleView("data_mor");
            MfeatBySingleView("data_pix");
            MfeatBySingleView("data_zer");
            MfeatByMultiView();

            University("cornell");
            University("texas");
            University("washington");
            University("wisconsin");

            OptDigits();//eu

            MultiViewArtificial();//eu

            Plant();
        }

        private static void Plant()
        {
            List<View> views = new List<View>();
            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\plant\100 leaves plant species\data_Mar_64.txt"))
            {
                views.Add(new View(ParsePlantData(sr.ReadToEnd())));
            }
            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\plant\100 leaves plant species\data_Sha_64.txt"))
            {
                views.Add(new View(ParsePlantData(sr.ReadToEnd())));
            }
            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\plant\100 leaves plant species\data_Tex_64.txt"))
            {
                views.Add(new View(ParsePlantData(sr.ReadToEnd(), false)));
            }

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(100, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            var label = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    label.Add(i);
                }
            }
            label.RemoveAt(0);

            Measure(ans: label.ToArray(), myans: GetLabels(mvdp.Clustering()), name: "plant");
        }

        private static DataPoint[] ParsePlantData(string rawData, bool needJump = true)
        {
            List<DataPoint> ans = new List<DataPoint>();
            var rawlines = rawData.Split('\n');
            foreach (var line in rawlines.Where(s => s != ""))
            {
                if (needJump)
                {
                    needJump = false;
                    continue;
                }
                List<double> features = new List<double>();
                var raw = line.Split(',');
                for (int i = 1; i < raw.Length; i++)
                {
                    features.Add(double.Parse(raw[i]));
                }
                ans.Add(new DataPoint(features.ToArray()));
            }

            return ans.ToArray();
        }

        private static void MultiViewArtificial()
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\artificial\artificial.mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["view1"])));
            views.Add(new View(ParseData(matFile["view2"])));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(2, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["viewans"]), myans: GetLabels(mvdp.Clustering()), name: "artificial");
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
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab2\实验2\数据集\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["A"])));
            views.Add(new View(ParseData(matFile["F"])));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(5, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(mvdp.Clustering()), name: universityName);
        }

        private static void MfeatByMultiView()
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\Mfeat.mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["data_fac"])));
            views.Add(new View(ParseData(matFile["data_fou"])));
            views.Add(new View(ParseData(matFile["data_kar"])));
            views.Add(new View(ParseData(matFile["data_mor"])));
            views.Add(new View(ParseData(matFile["data_pix"])));
            views.Add(new View(ParseData(matFile["data_zer"])));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(10, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
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
            var resultObj = MatlabMethods.ClusteringMeasure(1, new MWNumericArray(ans as Array), new MWNumericArray(myans as Array));
            var result = resultObj.First().ToArray() as double[,];
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("For dataset ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(":\nACC = {0}\nNMI = {1}\nPUR = {2}\n", result[0, 0], result[0, 1], result[0, 2]);
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
    }
}
