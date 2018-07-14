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

        private double WeightedAverage(double x, double y)
        {
            if (x + y == 0)
            {
                return 0;
            }
            else
            {
                return (2 * x * y) / (x + y);
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

            var minFactor1 = Views.Min(v => v.ViewQualityFactor1);
            var maxFactor1 = Views.Max(v => v.ViewQualityFactor1);
            var minFactor2 = Views.Min(v => v.ViewQualityFactor2);
            var maxFactor2 = Views.Max(v => v.ViewQualityFactor2);
            foreach (var view in Views)
            {
                view.ViewQualityFactor1 = (view.ViewQualityFactor1 - minFactor1) / (maxFactor1 - minFactor1) + 0.1;
                view.ViewQualityFactor2 = (view.ViewQualityFactor2 - minFactor2) / (maxFactor2 - minFactor2) + 0.1;
                Console.WriteLine("Quality Factors: {0}\t{1}\t{2}", view.ViewQualityFactor1, view.ViewQualityFactor2, WeightedAverage(view.ViewQualityFactor1, view.ViewQualityFactor2));
            }

            Parallel.ForEach(Views, (view) =>
            {
                for (int i = 0; i < NumOfDataPoints; i++)
                {
                    for (int j = 0; j < NumOfDataPoints; j++)
                    {
                        AbstractDistance[i][j] += ((view.Distance[i][j] - view.MinDistance) / (view.MaxDistance - view.MinDistance));
                    }
                }
            });

            //datapoints
            AbstractDataPoints = new DataPoint[NumOfDataPoints];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDataPoints[i] = new DataPoint(i);
                AbstractDataPoints[i].rho = 1d;
                foreach (var view in Views)
                {
                    var factor = WeightedAverage(view.ViewQualityFactor1, view.ViewQualityFactor2);
                    AbstractDataPoints[i].rho *= Math.Pow(view.DataPoints[i].rho, factor);
                }
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
