using MatFileHandler;
using System.Collections.Generic;
using System.IO;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void UniversityByMultiView(string universityName)
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\university\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            var aView = ParseData(matFile["A"]);
            var fView = ParseData(matFile["F"]);
            MinMaxNormalize(ref aView);
            MinMaxNormalize(ref fView);
            views.Add(new View(aView, "euclidean"));
            views.Add(new View(fView, "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(5, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(mvdp.Clustering()), name: universityName);
        }

        private static void UniversityBySingleView(string universityName, string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\university\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            var dataPoints = ParseData(matFile[entry]);
            MinMaxNormalize(ref dataPoints);
            DensityPeak dp = new DensityPeak(5, dataPoints, "euclidean");
            dp.Clustering();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(dp.DataPoints), name: universityName + "_" + entry);
        }
    }
}