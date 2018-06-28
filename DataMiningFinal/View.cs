using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    internal class View
    {
        internal double Dc { get; set; }
        public DataPoint[] DataPoints { get; set; }
        internal double[][] Distance { get; set; }
        internal double MaxDistance { get; set; }
        internal double MinDistance { get; set; }
        internal double MaxRho { get; set; }
        internal DensityDefinition DensityDefinition { get; set; }
        internal DcSelection DcSelection { get; set; }
        internal string DistanceMetric { get; set; }

        public View(DataPoint[] dataPoints, string distanceMetric)
        {
            DataPoints = dataPoints;
            for (int i = 0; i < DataPoints.Length; i++)
            {
                DataPoints[i].id = i;
            }
            Distance = Distance is null ? new double[DataPoints.Length][] : Distance;
            DistanceMetric = distanceMetric;
        }

        internal void CalcDc()
        {
            switch (DcSelection)
            {
                case DcSelection.EntropyBased:// To be refine
                    double minE = double.MaxValue;
                    double bestSigma = 1;

                    Parallel.For(1, 4, (x) =>
                    {
                        for (int i = x * 30; i < x * 30 + 30; i++)
                        {
                            double sigma = i * 0.01;
                            double[] Phis = new double[DataPoints.Length];
                            double temp;

                            for (int j = 0; j < DataPoints.Length; j++)
                            {
                                Phis[j] = 0;

                                foreach (var dp in DataPoints)
                                {
                                    temp = (Distance[j][dp.id]) / sigma;
                                    Phis[j] += Math.Exp(-temp * temp);
                                }
                            }

                            double curE = 0;
                            double z = Phis.Sum();
                            foreach (var phi in Phis)
                            {
                                temp = phi / z;
                                curE -= temp * Math.Log(temp);
                            }

                            if (curE < minE)
                            {
                                minE = curE;
                                bestSigma = sigma;
                            }
                        }
                    });

                    Dc = (3 * bestSigma) / Math.Sqrt(2);
                    break;
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
                        sum += row.Sum();
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

        internal void CalculateDistances()
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

            MaxDistance = double.MinValue;
            MinDistance = double.MaxValue;
            for (int i = 0; i < DataPoints.Length; i++)
            {
                for (int j = i; j < DataPoints.Length; j++)
                {
                    Distance[i][j] = ans[i, j];
                    Distance[j][i] = Distance[i][j];
                    MaxDistance = Math.Max(MaxDistance, Distance[i][j]);
                    if (i != j)
                    {
                        MinDistance = Math.Min(MinDistance, Distance[i][j]);
                    }
                }
            }

            Console.WriteLine("Distance calculated!");
        }

        internal void CalculateRhos()
        {
            var DcSquare = Dc * Dc;
            MaxRho = double.MinValue;
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
                MaxRho = Math.Max(MaxRho, DataPoints[i].rho);
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
