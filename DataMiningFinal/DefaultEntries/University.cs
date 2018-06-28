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
            IMatFile matFile = (new MatFileReader(new FileStream(@"..\..\..\Datasets\university\" + universityName + @"\" + universityName + ".mat", FileMode.Open))).Read();
            views.Add(new View(ParseData(matFile["A"]), "cityblock"));
            views.Add(new View(ParseData(matFile["F"]), "cityblock"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(5, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance);
            mvdp.ConstructAbstractData();

            Measure(ans: GetLabels(matFile["label"]), myans: GetLabels(mvdp.Clustering()), name: universityName);
        }
    }
}