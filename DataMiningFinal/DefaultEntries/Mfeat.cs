using MatFileHandler;
using System.Collections.Generic;
using System.IO;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void MfeatByMultiView()
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\mfeat\Mfeat.mat", FileMode.Open))).Read();
            //fou + fac get current best performance
            //views.Add(new View(ParseData(matFile["data_fac"]), "euclidean"));
            views.Add(new View(ParseData(matFile["data_fou"]), "euclidean"));
            views.Add(new View(ParseData(matFile["data_kar"]), "euclidean"));
            //views.Add(new View(ParseData(matFile["data_mor"]), "euclidean"));
            //views.Add(new View(ParseData(matFile["data_pix"]), "euclidean"));
            //views.Add(new View(ParseData(matFile["data_zer"]), "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(10, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(mvdp.Clustering()), name: "Mfeat");
        }


        private static void MfeatBySingleView(string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\mfeat\Mfeat.mat", FileMode.Open))).Read();

            DensityPeak dp = new DensityPeak(10, ParseData(matFile[entry]), "euclidean");

            dp.Clustering();

            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(dp.DataPoints), name: "Mfeat_" + entry);
        }
    }
}