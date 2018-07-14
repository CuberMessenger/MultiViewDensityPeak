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
        private static Dictionary<string, int> NameToNumDictionaryOfImageSegmentation = new Dictionary<string, int>
            {
                { "BRICKFACE", 0},
                { "SKY", 1},
                { "FOLIAGE", 2},
                { "CEMENT", 3},
                { "WINDOW", 4},
                { "PATH", 5},
                { "GRASS", 6}
            };
        private static void ImageSegmentationByMultiView()
        {
            List<View> views = new List<View>();
            List<int> answers = new List<int>();
            List<DataPoint> view1DataPoints = new List<DataPoint>();
            List<DataPoint> view2DataPoints = new List<DataPoint>();
            List<DataPoint> view3DataPoints = new List<DataPoint>();
            List<DataPoint> view4DataPoints = new List<DataPoint>();

            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\image segmentation\segmentation.data"))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    var raw = line.Split(',');
                    answers.Add(NameToNumDictionaryOfImageSegmentation[raw.First()]);

                    double[] v1f = new double[3];
                    double[] v2f = new double[2];
                    double[] v3f = new double[4];
                    double[] v4f = new double[10];
                    for (int i = 0; i < 3; i++)
                    {
                        v1f[i] = double.Parse(raw[i + 1]);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        v2f[i] = double.Parse(raw[i + 4]);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        v3f[i] = double.Parse(raw[i + 6]);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        v4f[i] = double.Parse(raw[i + 10]);
                    }

                    view1DataPoints.Add(new DataPoint(v1f));
                    view2DataPoints.Add(new DataPoint(v2f));
                    view3DataPoints.Add(new DataPoint(v3f));
                    view4DataPoints.Add(new DataPoint(v4f));

                    line = sr.ReadLine();
                }
            }

            var view1DataPointsArray = view1DataPoints.ToArray();
            var view2DataPointsArray = view2DataPoints.ToArray();
            var view3DataPointsArray = view3DataPoints.ToArray();
            var view4DataPointsArray = view4DataPoints.ToArray();
            MinMaxNormalize(ref view1DataPointsArray);
            MinMaxNormalize(ref view2DataPointsArray);
            MinMaxNormalize(ref view3DataPointsArray);
            MinMaxNormalize(ref view4DataPointsArray);

            views.Add(new View(view1DataPointsArray, "cityblock"));
            views.Add(new View(view2DataPointsArray, "cityblock"));
            views.Add(new View(view3DataPointsArray, "cityblock"));
            views.Add(new View(view4DataPointsArray, "cityblock"));

            MultiViewDensityPeak mvdp = new MultiViewDensityPeak(7, views.ToArray(), DensityDefinition.GaussianKernal, DcSelection.AverageDistance, "cityblock");
            mvdp.ConstructAbstractData();

            Measure(answers.ToArray(), GetLabels(mvdp.Clustering()), "ImageSegmentaion");
        }

        private static void ImageSegmentationBySingleView(string entry)
        {
            List<int> answers = new List<int>();
            List<DataPoint> viewDataPoints = new List<DataPoint>();

            using (StreamReader sr = new StreamReader(@"D:\Software\Visual Studio 2017\Workplace\DataMiningFinal\Datasets\image segmentation\segmentation.data"))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    var raw = line.Split(',');
                    answers.Add(NameToNumDictionaryOfImageSegmentation[raw.First()]);
                    double[] vf = null;

                    switch (entry)
                    {
                        case "1":
                            vf = new double[3];
                            for (int i = 0; i < 3; i++)
                            {
                                vf[i] = double.Parse(raw[i + 1]);
                            }
                            break;
                        case "2":
                            vf = new double[2];
                            for (int i = 0; i < 2; i++)
                            {
                                vf[i] = double.Parse(raw[i + 4]);
                            }
                            break;
                        case "3":
                            vf = new double[4];
                            for (int i = 0; i < 4; i++)
                            {
                                vf[i] = double.Parse(raw[i + 6]);
                            }
                            break;
                        case "4":
                            vf = new double[10];
                            for (int i = 0; i < 10; i++)
                            {
                                vf[i] = double.Parse(raw[i + 10]);
                            }
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

            DensityPeak dp = new DensityPeak(7, viewDataPointsArray, "cityblock");
            dp.Clustering();

            Measure(ans: answers.ToArray(), myans: GetLabels(dp.DataPoints), name: "ImageSegmenation by view " + entry);
        }
    }
}
