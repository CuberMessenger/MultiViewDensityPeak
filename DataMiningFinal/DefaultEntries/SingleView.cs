using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void SingleViewArtificial(string name, int k)
        {
            StreamReader sr = new StreamReader(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\" + name + ".txt");
            var data = ParseData(sr.ReadToEnd());

            DensityPeak dp = new DensityPeak(k, data);
            dp.Clustering();

            sr.Close();
            sr = new StreamReader(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\anses\" + name + ".txt");

            Measure(ans: ParseAnswer(sr.ReadToEnd()), myans: GetLabels(dp.DataPoints), name: name);
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
    }
}