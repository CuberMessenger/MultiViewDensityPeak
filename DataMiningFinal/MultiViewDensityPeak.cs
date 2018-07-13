using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    internal class MultiViewDensityPeak
    {
        private int K { get; set; }
        private int NumOfDataPoints { get; set; }
        private View[] Views { get; set; }
        private DataPoint[] AbstractDataPoints { get; set; }
        private double[][] AbstractDistance { get; set; }
        private DensityPeak AbstractDensityPeak { get; set; }
        private string DistanceMetricForAbstractData { get; set; }

        public MultiViewDensityPeak(int k, View[] views, DensityDefinition densityDefinition, DcSelection dcSelection, string distanceMetric = "euclidean")
        {
            K = k;
            Views = views;
            DistanceMetricForAbstractData = distanceMetric;
            NumOfDataPoints = Views.First().DataPoints.Length;

            foreach (var view in Views)
            {
                view.K = k;
                view.DensityDefinition = densityDefinition;
                view.DcSelection = dcSelection;
            }
        }

        public void ConstructAbstractData()
        {
            //distance
            AbstractDistance = new double[NumOfDataPoints][];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDistance[i] = Enumerable.Repeat(0d, NumOfDataPoints).ToArray();
            }

            var res = Parallel.ForEach(Views, (view) =>
              {
                  view.CalculateDistances();
                  view.CalcDc();
                  view.CalculateRhos();
                  view.CalculateDeltas();
                  view.CalculateTaus();
                  view.CalculateViewQuality();
              });

            var minQuality = double.MaxValue;
            var minIndex = -1;
            for (int i = 0; i < Views.Length; i++)
            {
                if (Views[i].ViewQuality < minQuality)
                {
                    minQuality = Views[i].ViewQuality;
                    minIndex = i;
                }
            }

            Views[minIndex].ViewQuality = Math.E;
            for (int i = 0; i < Views.Length; i++)
            {
                Views[i].ViewQuality = (Views[i].ViewQuality / minQuality) * Math.E;
                Console.WriteLine(Math.Log(Views[i].ViewQuality + 10d));
            }

            Parallel.ForEach(Views, (view) =>
            {
                for (int i = 0; i < NumOfDataPoints; i++)
                {
                    for (int j = 0; j < NumOfDataPoints; j++)
                    {
                        AbstractDistance[i][j] += ((view.Distance[i][j] - view.MinDistance) * Math.Log(view.ViewQuality + 10d)) / (view.MaxDistance - view.MinDistance);
                    }
                }
            });

            //datapoints
            AbstractDataPoints = new DataPoint[NumOfDataPoints];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDataPoints[i] = new DataPoint(i);
                AbstractDataPoints[i].rho = 1d;
                Views.ToList().ForEach(view => AbstractDataPoints[i].rho *= view.DataPoints[i].rho);
            }
        }

        public DataPoint[] Clustering()
        {
            AbstractDensityPeak =
                new DensityPeak(K, AbstractDataPoints, DistanceMetricForAbstractData, DensityDefinition.GaussianKernal, DcSelection.AverageDistance, AbstractDistance);
            AbstractDensityPeak.FindMaxDistance();
            AbstractDensityPeak.CalculateDeltas();
            AbstractDensityPeak.CalculateTaus();
            AbstractDensityPeak.Clustering(false);
            return AbstractDensityPeak.DataPoints;
        }
    }
}
