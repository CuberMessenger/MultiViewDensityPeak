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
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\Mfeat.mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["data_fac"]), true));
            views.Add(new View(ParseData(matFile["data_fou"]), true));
            views.Add(new View(ParseData(matFile["data_kar"]), true));
            views.Add(new View(ParseData(matFile["data_mor"]), true));
            views.Add(new View(ParseData(matFile["data_pix"]), true));
            views.Add(new View(ParseData(matFile["data_zer"]), true));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(10, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(mvdp.Clustering()), name: "Mfeat");
        }


        private static void MfeatBySingleView(string entry)
        {
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab1\datasets\Mfeat.mat", FileMode.Open))).Read();

            DensityPeak dp = new DensityPeak(10, ParseData(matFile[entry]));

            dp.Clustering();

            Console.WriteLine("Single by " + entry);
            Measure(ans: GetLabels(matFile["classid"]), myans: GetLabels(dp.DataPoints), name: "Mfeat_" + entry);
        }
    }
}