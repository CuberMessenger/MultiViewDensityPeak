using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DataMiningFinal
{
    partial class Program
    {
        private const int NumOfArticalsIn3Sourse = 416;
        private static void ThreeSource()
        {
            List<int> jointArticalsID;
            int[] answer;
            List<View> views = new List<View>();
            List<DataPoint> bbcDataPoints = new List<DataPoint>();
            List<DataPoint> reutersDataPoints = new List<DataPoint>();
            List<DataPoint> guardianDataPoints = new List<DataPoint>();
            double[][] bbcMTX = new double[NumOfArticalsIn3Sourse][];
            double[][] reutersMTX = new double[NumOfArticalsIn3Sourse][];
            double[][] guardianMTX = new double[NumOfArticalsIn3Sourse][];

            var bbcIDs = ParseArticalID(@"..\..\..\Datasets\3sources\3sources_bbc.docs");
            var reutersIDs = ParseArticalID(@"..\..\..\Datasets\3sources\3sources_reuters.docs");
            var guardianIDs = ParseArticalID(@"..\..\..\Datasets\3sources\3sources_guardian.docs");
            jointArticalsID = bbcIDs.Intersect(reutersIDs).Intersect(guardianIDs).ToList();

            Thread bbcMTXThread = new Thread(
                () => ParseMTX(@"..\..\..\Datasets\3sources\3sources_bbc.mtx", ref bbcMTX, ref bbcIDs));
            Thread reutersMTXThread = new Thread(
                () => ParseMTX(@"..\..\..\Datasets\3sources\3sources_reuters.mtx", ref reutersMTX, ref reutersIDs));
            Thread guardianMTXThread = new Thread(
                () => ParseMTX(@"..\..\..\Datasets\3sources\3sources_guardian.mtx", ref guardianMTX, ref guardianIDs));
            bbcMTXThread.Start();
            reutersMTXThread.Start();
            guardianMTXThread.Start();
            bbcMTXThread.Join();
            reutersMTXThread.Join();
            guardianMTXThread.Join();

            foreach (int id in jointArticalsID)
            {
                bbcDataPoints.Add(new DataPoint(bbcMTX[id - 1]));
                reutersDataPoints.Add(new DataPoint(reutersMTX[id - 1]));
                guardianDataPoints.Add(new DataPoint(guardianMTX[id - 1]));
            }

            var bbcDataPointsArray = bbcDataPoints.ToArray();
            var reutersDataPointsArray = reutersDataPoints.ToArray();
            var guardianDataPointsArray = guardianDataPoints.ToArray();
            MinMaxNormalize(ref bbcDataPointsArray);
            MinMaxNormalize(ref reutersDataPointsArray);
            MinMaxNormalize(ref guardianDataPointsArray);

            answer = Parse3SourceLabel(ref jointArticalsID);

            views.Add(new View(bbcDataPointsArray, "euclidean"));
            views.Add(new View(reutersDataPointsArray, "euclidean"));
            views.Add(new View(guardianDataPointsArray, "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(6, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: answer, myans: GetLabels(mvdp.Clustering()), name: "3Source");
        }

        private static void ThreeSourceBySingleView(string entry)
        {
            List<int> jointArticalsID;
            int[] answer;
            List<DataPoint> dataPoints = new List<DataPoint>();
            double[][] MTX = new double[NumOfArticalsIn3Sourse][];

            var bbcIDs = ParseArticalID(@"..\..\..\Datasets\3sources\3sources_bbc.docs");
            var reutersIDs = ParseArticalID(@"..\..\..\Datasets\3sources\3sources_reuters.docs");
            var guardianIDs = ParseArticalID(@"..\..\..\Datasets\3sources\3sources_guardian.docs");
            jointArticalsID = bbcIDs.Intersect(reutersIDs).Intersect(guardianIDs).ToList();

            Thread MTXThread = new Thread(
                () => ParseMTX(@"..\..\..\Datasets\3sources\3sources_" + entry + ".mtx", ref MTX, ref bbcIDs));
            MTXThread.Start();
            MTXThread.Join();

            foreach (int id in jointArticalsID)
            {
                dataPoints.Add(new DataPoint(MTX[id - 1]));
            }

            answer = Parse3SourceLabel(ref jointArticalsID);

            DensityPeak dp = new DensityPeak(6, dataPoints.ToArray(), "euclidean");
            dp.Clustering();

            Measure(ans: answer, myans: GetLabels(dp.DataPoints), name: "3Source_" + entry);
        }

        private static int[] Parse3SourceLabel(ref List<int> jointIDs)
        {
            int[] ans = new int[jointIDs.Count];
            int[] allLabel = new int[NumOfArticalsIn3Sourse];
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\3sources\3sources.disjoint.clist"))
            {
                string line = sr.ReadLine();
                int label = 0;
                while (line != null)
                {
                    var raw = line.Split(':').Last().Split(',');
                    raw.ToList().ForEach(r => allLabel[int.Parse(r) - 1] = label);
                    label++;
                    line = sr.ReadLine();
                }
            }
            for (int i = 0; i < ans.Length; i++)
            {
                ans[i] = allLabel[jointIDs[i] - 1];
            }
            return ans;
        }

        private static List<int> ParseArticalID(string path)
        {
            List<int> ans = new List<int>();
            using (StreamReader sr = new StreamReader(path))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    ans.Add(int.Parse(line));
                    line = sr.ReadLine();
                }
            }
            return ans;
        }

        private static void ParseMTX(string path, ref double[][] destination, ref List<int> IDs)
        {
            string line = null;
            using (StreamReader sr = new StreamReader(path))
            {
                sr.ReadLine();
                var norc = sr.ReadLine().Split(' ');
                int nor = int.Parse(norc[1]);
                int noc = int.Parse(norc[0]);
                for (int i = 0; i < NumOfArticalsIn3Sourse; i++)
                {
                    destination[i] = Enumerable.Repeat(0d, noc).ToArray();
                }

                line = sr.ReadLine();
                while (line != null)
                {
                    var raw = line.Split(' ');
                    destination[IDs[int.Parse(raw[1]) - 1] - 1][int.Parse(raw[0]) - 1] = double.Parse(raw[2]);
                    line = sr.ReadLine();
                }
            }
        }
    }
}