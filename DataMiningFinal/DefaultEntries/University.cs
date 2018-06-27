using MatFileHandler;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataMiningFinal
{
    partial class Program
    {
        private static void University(string universityName)
        {
            List<View> views = new List<View>();
            IMatFile matFile = (new MatFileReader(new FileStream(@"D:\OneDrive\资料\大三\大三下\数据挖掘\lab\Lab2\实验2\数据集\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["A"]), "euclidean"));
            views.Add(new View(ParseData(matFile["F"]), "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(5, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(mvdp.Clustering()), name: universityName);
        }
    }
}