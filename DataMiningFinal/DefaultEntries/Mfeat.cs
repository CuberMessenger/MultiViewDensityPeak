using MatFileHandler;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void MfeatByMultiView()
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\mfeat\NormalizedMfeat.mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["data_fac"]), "euclidean", "data_fac"));
            views.Add(new View(ParseData(matFile["data_fou"]), "euclidean", "data_fou"));
            views.Add(new View(ParseData(matFile["data_kar"]), "euclidean", "data_kar"));
            views.Add(new View(ParseData(matFile["data_mor"]), "euclidean", "data_mor"));
            views.Add(new View(ParseData(matFile["data_pix"]), "euclidean", "data_pix"));
            views.Add(new View(ParseData(matFile["data_zer"]), "euclidean", "data_zer"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(10, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(mvdp.Clustering()), name: "Mfeat");
        }


        private static void MfeatBySingleView(string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\mfeat\NormalizedMfeat.mat", FileMode.Open))).Read();

            DensityPeak dp = new DensityPeak(10, ParseData(matFile[entry]), "euclidean");

            dp.Clustering();

            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(dp.DataPoints), name: "Mfeat_" + entry);
        }
    }
}