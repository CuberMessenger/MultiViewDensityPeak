using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void Plant()
        {
            List<View> views = new List<View>();
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\data_Mar_64.txt"))
            {
                views.Add(new View(ParsePlantData(sr.ReadToEnd())));
            }
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\data_Sha_64.txt"))
            {
                views.Add(new View(ParsePlantData(sr.ReadToEnd())));
            }
            using (StreamReader sr = new StreamReader(@"..\..\..\Datasets\plant\100 leaves plant species\data_Tex_64.txt"))
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
    }
}