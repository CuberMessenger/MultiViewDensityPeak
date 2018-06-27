using MatFileHandler;
using MathWorks.MATLAB.NET.Arrays;
using MatlabUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DataMiningFinal
{
    partial class Program
    {
        public static MatlabMethods MatlabMethods;
        public static Thread initThread;
        static void Main(string[] args)
        {
            initThread = new Thread(() =>
            {
                Console.WriteLine("MatlabUtil initializing......");
                MatlabMethods = new MatlabMethods();
                Console.WriteLine("MatlabUtil Inited!");
            });
            initThread.Start();

            //SingleViewArtificial("Spiral", 3);
            //SingleViewArtificial("Pathbased", 3);
            //SingleViewArtificial("Jain", 2);
            //SingleViewArtificial("Flame", 2);
            //SingleViewArtificial("Aggregation", 7);

            //MfeatBySingleView("data_fac");
            //MfeatBySingleView("data_fou");
            //MfeatBySingleView("data_kar");
            //MfeatBySingleView("data_mor");
            //MfeatBySingleView("data_pix");
            //MfeatBySingleView("data_zer");
            //MfeatByMultiView();

            //University("cornell");
            //University("texas");
            //University("washington");
            //University("wisconsin");

            //MultiViewArtificial();

            PlantBySingleView("data_Mar");
            PlantBySingleView("data_Sha");
            PlantBySingleView("data_Tex");
            PlantByMultiView();

            //ThreeSource();
        }

        private static void MultiViewArtificial()
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\artificial\artificial.mat", FileMode.Open))).Read();
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
