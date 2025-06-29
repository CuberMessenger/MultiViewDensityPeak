﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    internal class MultiViewDensityPeak
    {
        private static double Alpha = 0.1d;

        private int K { get; set; }
        private int NumOfDataPoints { get; set; }
        private View[] Views { get; set; }
        private DataPoint[] GlobalViewDataPoints { get; set; }
        private double[][] GlobalViewDistance { get; set; }
        private DensityPeak GlobalViewDensityPeak { get; set; }
        private string DistanceMetricForGlobalData { get; set; }
        private bool UseQualityEstimation { get; set; }

        public MultiViewDensityPeak(int k, View[] views,
            DensityDefinition densityDefinition,
            DcSelection dcSelection,
            string distanceMetric = "euclidean",
            bool useQualityEstimation = true)
        {
            K = k;
            Views = views;
            DistanceMetricForGlobalData = distanceMetric;
            NumOfDataPoints = Views.First().DataPoints.Length;
            UseQualityEstimation = useQualityEstimation;

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
            GlobalViewDistance = new double[NumOfDataPoints][];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                GlobalViewDistance[i] = Enumerable.Repeat(0d, NumOfDataPoints).ToArray();
            }

            //foreach (var view in Views)
            Parallel.ForEach(Views, (view) =>
            {
                view.CalculateDistances();
                view.CalcDc();
                view.CalculateRhos();
                view.CalculateDeltas();
                view.CalculateTaus();
                view.CalculateViewQuality();

                lock (GlobalViewDistance)
                {
                    for (int i = 0; i < NumOfDataPoints; i++)
                    {
                        for (int j = 0; j < NumOfDataPoints; j++)
                        {
                            GlobalViewDistance[i][j] += ((view.Distance[i][j] - view.MinDistance) / (view.MaxDistance - view.MinDistance));
                        }
                    }
                }
            });

            var minFactor1 = Views.Min(v => v.ViewQualityFactor1);
            var maxFactor1 = Views.Max(v => v.ViewQualityFactor1);
            var minFactor2 = Views.Min(v => v.ViewQualityFactor2);
            var maxFactor2 = Views.Max(v => v.ViewQualityFactor2);
            foreach (var view in Views)
            {
                view.ViewQualityFactor1 = (view.ViewQualityFactor1 - minFactor1) / (maxFactor1 - minFactor1) + Alpha;
                view.ViewQualityFactor2 = (view.ViewQualityFactor2 - minFactor2) / (maxFactor2 - minFactor2) + Alpha;
                Console.WriteLine("Quality Factors: {0}\t{1}\t{2}", view.ViewQualityFactor1, view.ViewQualityFactor2, WeightedAverage(view.ViewQualityFactor2, view.ViewQualityFactor1));
            }

            //datapoints
            GlobalViewDataPoints = new DataPoint[NumOfDataPoints];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                GlobalViewDataPoints[i] = new DataPoint(i);
                GlobalViewDataPoints[i].rho = 1d;
                foreach (var view in Views)
                {
                    var factor = WeightedAverage(view.ViewQualityFactor1, view.ViewQualityFactor2);
                    GlobalViewDataPoints[i].rho *= Math.Pow(view.DataPoints[i].rho, UseQualityEstimation ? factor : 1d);
                }
            }
        }

        public DataPoint[] Clustering()
        {
            GlobalViewDensityPeak =
                new DensityPeak(K, GlobalViewDataPoints, DistanceMetricForGlobalData, DensityDefinition.GaussianKernal, DcSelection.AverageDistance, GlobalViewDistance);
            GlobalViewDensityPeak.FindMaxDistance();
            GlobalViewDensityPeak.CalculateDeltas();
            GlobalViewDensityPeak.CalculateTaus();
            GlobalViewDensityPeak.Clustering(false);
            return GlobalViewDensityPeak.DataPoints;
        }
    }
}
