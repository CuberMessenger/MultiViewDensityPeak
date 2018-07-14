using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMiningFinal
{
    internal class View
    {
        internal int K { get; set; }
        internal double Dc { get; set; }
        internal DataPoint[] DataPoints { get; set; }
        internal List<DataPoint> Centroids { get; set; }
        internal double[][] Distance { get; set; }
        internal double MaxDistance { get; set; }
        internal double MinDistance { get; set; }
        internal double ViewQualityFactor1 { get; set; }
        internal double ViewQualityFactor2 { get; set; }
        internal DensityDefinition DensityDefinition { get; set; }
        internal DcSelection DcSelection { get; set; }
        internal string DistanceMetric { get; set; }
        internal string Tag { get; set; }

        public View(DataPoint[] dataPoints, string distanceMetric, string tag = null)
        {
            DataPoints = dataPoints;
            for (int i = 0; i < DataPoints.Length; i++)
            {
                DataPoints[i].id = i;
            }
            Distance = Distance is null ? new double[DataPoints.Length][] : Distance;
            DistanceMetric = distanceMetric;
            Tag = tag;
        }

        internal void CalcDc()
        {
            switch (DcSelection)
            {
                case DcSelection.DistanceBased:
                    var distinctDistances = new List<double>();
                    for (int i = 0; i < DataPoints.Length; i++)
                    {
                        for (int j = i + 1; j < DataPoints.Length; j++)
                        {
                            distinctDistances.Add(Distance[i][j]);
                        }
                    }
                    distinctDistances.Sort();
                    Dc = distinctDistances[(int)(DataPoints.Length * 0.02)];
                    break;
                case DcSelection.NumOfPointsBased:
                    Dc = DataPoints.Length * 0.01;
                    break;
                case DcSelection.AverageDistance:
                    double sum = 0;
                    foreach (var row in Distance)
                    {
                        row.Where(d => !double.IsInfinity(d)).ToList().ForEach(d => sum += d);
                    }
                    Dc = Math.Sqrt(sum / (Distance.Length * Distance.Length));
                    break;
            }

            Console.WriteLine("DC = {0}", Dc);
        }

        internal double[][] GetFeatureMatrix()
        {
            double[][] ans = new double[DataPoints.Length][];
            for (int i = 0; i < DataPoints.Length; i++)
            {
                ans[i] = DataPoints[i].features.ToArray();
            }
            return ans;
        }

        internal void CalculateViewQuality()
        {
            var averageRho = DataPoints.Average(dp => dp.rho);
            Centroids = new List<DataPoint>(DataPoints.OrderByDescending(dp => dp.rho * (dp.delta - dp.tau)).Take(K));
            ViewQualityFactor1 = Centroids.Average(c => (c.delta - c.tau) / MaxDistance);
            ViewQualityFactor2 = Centroids.Average(c => c.rho / averageRho);
        }

        internal void CalculateDistances()
        {
            MaxDistance = double.MinValue;
            MinDistance = double.MaxValue;

            if (DistanceMetric == "geodesic")
            {
                for (int i = 0; i < DataPoints.Length; i++)
                {
                    Distance[i] = DataPoints[i].features.ToArray();
                    for (int j = 0; j < DataPoints.Length; j++)
                    {
                        if (Distance[i][j] == 0 && i != j)
                        {
                            Distance[i][j] = double.PositiveInfinity;
                        }
                    }
                }

                for (int k = 0; k < DataPoints.Length; k++)
                {
                    for (int i = 0; i < DataPoints.Length; i++)
                    {
                        for (int j = 0; j < DataPoints.Length; j++)
                        {
                            Distance[i][j] = Math.Min(Distance[i][j], Distance[i][k] + Distance[k][j]);
                        }
                    }
                }

                for (int i = 0; i < DataPoints.Length; i++)
                {
                    for (int j = 0; j < DataPoints.Length; j++)
                    {
                        MaxDistance = double.IsInfinity(Distance[i][j]) ? MaxDistance : Math.Max(MaxDistance, Distance[i][j]);
                        MinDistance = Math.Min(MinDistance, Distance[i][j]);
                    }
                }
            }
            else
            {
                Program.initThread.Join();
                var ansObj = Program.MatlabMethods.CalculateDistance(1,
                    new MWNumericArray(GetFeatureMatrix() as Array),
                    new MWCharArray(DistanceMetric));//euclidean
                var ans = ansObj[0].ToArray() as double[,];

                for (int i = 0; i < DataPoints.Length; i++)
                {
                    Distance[i] = new double[DataPoints.Length];
                }

                for (int i = 0; i < DataPoints.Length; i++)
                {
                    for (int j = i; j < DataPoints.Length; j++)
                    {
                        Distance[i][j] = ans[i, j];
                        Distance[j][i] = Distance[i][j];
                        MaxDistance = Math.Max(MaxDistance, Distance[i][j]);
                        MinDistance = Math.Min(MinDistance, Distance[i][j]);
                    }
                }
            }

            Console.WriteLine("Distance calculated!");
        }

        internal void CalculateRhos()
        {
            var DcSquare = Dc * Dc;
            for (int i = 0; i < DataPoints.Length; i++)
            {
                switch (DensityDefinition)
                {
                    case DensityDefinition.NumOfNeighbor:
                        DataPoints[i].rho = Distance[i].Where(d => d <= Dc).Count();
                        break;
                    case DensityDefinition.GaussianKernal:
                        DataPoints[i].rho = -1;
                        Distance[i].ToList().ForEach(dis => DataPoints[i].rho += Math.Exp(-((dis * dis) / (DcSquare))));
                        break;
                }
            }

            Console.WriteLine("Rhos calculated!");
        }

        internal void CalculateDeltas()
        {
            foreach (DataPoint dp in DataPoints)
            {
                var seniors = DataPoints.Where(p => p.rho > dp.rho);
                if (seniors.Count() > 0)
                {
                    var senior = seniors.OrderBy(p => Distance[dp.id][p.id]).First();
                    dp.delta = Distance[dp.id][senior.id];
                    dp.senior = senior;
                }
                else
                {
                    dp.delta = MaxDistance;
                }
            }

            Console.WriteLine("Deltas calculated!");
        }

        internal void CalculateTaus()
        {
            foreach (DataPoint dp in DataPoints)
            {
                var juniors = DataPoints.Where(p => p.rho < dp.rho);
                if (juniors.Count() > 0)
                {
                    var junior = juniors.OrderBy(p => Distance[dp.id][p.id]).First();
                    dp.tau = Distance[dp.id][junior.id];
                }
                else
                {
                    dp.tau = dp.delta;
                }
            }
            Console.WriteLine("Taus calculated!");
        }
    }
}
