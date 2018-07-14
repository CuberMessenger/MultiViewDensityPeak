using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    partial class Program
    {
        private static Dictionary<string, int> NameTONumDictionaryOfIris = new Dictionary<string, int>
        {
            {"Iris-setosa",  0},
            {"Iris-versicolor", 1 },
            {"Iris-virginica",  2}
        };

        private static void IrisByMultiView()
        {
            List<View> views = new List<View>();
            List<int> answers = new List<int>();
            List<DataPoint> view1DataPoints = new List<DataPoint>();
            List<DataPoint> view2DataPoints = new List<DataPoint>();

            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\iris\iris.data"))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    var raw = line.Split(',');
                    answers.Add(NameTONumDictionaryOfIris[raw.Last()]);

                    double[] v1f = new double[2];
                    double[] v2f = new double[2];
                    v1f[0] = double.Parse(raw[0]);
                    v1f[1] = double.Parse(raw[1]);
                    v2f[0] = double.Parse(raw[2]);
                    v2f[1] = double.Parse(raw[3]);

                    view1DataPoints.Add(new DataPoint(v1f));
                    view2DataPoints.Add(new DataPoint(v2f));

                    line = sr.ReadLine();
                }
            }

            var view1DataPointsArray = view1DataPoints.ToArray();
            var view2DataPointsArray = view2DataPoints.ToArray();
            MinMaxNormalize(ref view1DataPointsArray);
            MinMaxNormalize(ref view2DataPointsArray);

            views.Add(new View(view1DataPointsArray, "euclidean"));
            views.Add(new View(view2DataPointsArray, "euclidean"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(3, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance, "euclidean", false);
            mvdp.ConstructAbstractData();

            Measure(answers.ToArray(), GetLabels(mvdp.Clustering()), "Iris");
        }

        private static void IrisBySingleView(string entry)
        {
            List<View> views = new List<View>();
            List<int> answers = new List<int>();
            List<DataPoint> viewDataPoints = new List<DataPoint>();

            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\iris\iris.data"))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    var raw = line.Split(',');
                    answers.Add(NameTONumDictionaryOfIris[raw.Last()]);
                    double[] vf = new double[2];

                    switch (entry)
                    {
                        case "1":
                            vf[0] = double.Parse(raw[0]);
                            vf[1] = double.Parse(raw[2]);
                            break;
                        case "2":
                            vf[0] = double.Parse(raw[1]);
                            vf[1] = double.Parse(raw[3]);
                            break;
                        default:
                            break;
                    }

                    viewDataPoints.Add(new DataPoint(vf));
                    line = sr.ReadLine();
                }
            }

            var viewDataPointsArray = viewDataPoints.ToArray();
            MinMaxNormalize(ref viewDataPointsArray);

            DensityPeak dp = new DensityPeak(3, viewDataPointsArray, "euclidean");
            dp.Clustering();

            Measure(ans: answers.ToArray(), myans: GetLabels(dp.DataPoints), name: "Iris by view " + entry);
        }
    }
}
