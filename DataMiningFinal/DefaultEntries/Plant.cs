using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void PlantByMultiView()
        {
            List<View> views = new List<View>();
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\data_Mar_64.txt"))
            {
                var dataPoints = ParsePlantData(sr.ReadToEnd());
                MinMaxNormalize(ref dataPoints);
                views.Add(new View(dataPoints, "cosine"));
            }
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\data_Sha_64.txt"))
            {
                var dataPoints = ParsePlantData(sr.ReadToEnd());
                MinMaxNormalize(ref dataPoints);
                views.Add(new View(dataPoints, "cosine"));
            }
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\data_Tex_64.txt"))
            {
                var dataPoints = ParsePlantData(sr.ReadToEnd(), false);
                MinMaxNormalize(ref dataPoints);
                views.Add(new View(dataPoints, "cosine"));
            }

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(100, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance, "cosine");
            mvdp.ConstructAbstractData();

            Measure(ans: GenerateCorrectPlantAnswer(), myans: GetLabels(mvdp.Clustering()), name: "plant");
        }

        private static int[] GenerateCorrectPlantAnswer()
        {
            var label = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    label.Add(i);
                }
            }
            label.RemoveAt(0);
            return label.ToArray();
        }

        private static void PlantBySingleView(string viewName)
        {
            DensityPeak dp;
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\" + viewName + "_64.txt"))
            {
                var dataPoints = ParsePlantData(sr.ReadToEnd(), viewName != "data_Tex");
                MinMaxNormalize(ref dataPoints);
                dp = new DensityPeak(100, dataPoints, "cosine");
            }
            dp.Clustering();

            Measure(ans: GenerateCorrectPlantAnswer(), myans: GetLabels(dp.DataPoints), name: "plant_" + viewName);
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
    }
}