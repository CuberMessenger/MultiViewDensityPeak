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
            views.Add(new View(ParseData(matFile["A"]), "euclidean"));
            views.Add(new View(ParseData(matFile["F"]), "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(5, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(mvdp.Clustering()), name: universityName);
        }

        private static void UniversityBySingleView(string universityName, string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\university\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            DensityPeak dp = new DensityPeak(5, ParseData(matFile[entry]), "euclidean");
            dp.Clustering();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(dp.DataPoints), name: universityName + "_" + entry);
        }
    }
}