using MatFileHandler;
using System.Collections.Generic;
using System.IO;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void MultiViewArtificial()
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\artificial\artificial.mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["v1"]), "euclidean"));
            views.Add(new View(ParseData(matFile["v2"]), "euclidean"));
            views.Add(new View(ParseData(matFile["v3"]), "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(2, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["answer"]), myans: GetLabels(mvdp.Clustering()), name: "artificial");
        }

        private static void SingleViewArtificial(string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\artificial\artificial.mat", FileMode.Open))).Read();

            DensityPeak dp = new DensityPeak(2, ParseData(matFile[entry]), "euclidean");

            dp.Clustering();

            Measure(ans: GetLabels(matFile["answer"]), myans: GetLabels(dp.DataPoints), name: "artificial_" + entry);
        }
    }
}
