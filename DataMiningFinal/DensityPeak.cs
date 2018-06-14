using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    public enum DensityDefinition { NumOfNeighbor, GaussianKernal }
    public enum DcSelection { EntropyBased, DistanceBased, NumOfPointsBased, AverageDistance }
    public class DensityPeak
    {
        private double Dc { get; set; }
        private int K { get; set; }
        public DataPoint[] DataPoints { get; set; }
        private List<DataPoint> Centroids { get; set; }
        private double[][] Distance { get; set; }
        private DensityDefinition DensityDefinition { get; set; }
        private DcSelection DcSelection { get; set; }

        public DensityPeak(int k, DataPoint[] dataPoints)
        {
            K = k;
            DataPoints = dataPoints;
            Distance = new double[DataPoints.Length][];

            //Configure
            DensityDefinition = DensityDefinition.GaussianKernal;
            DcSelection = DcSelection.AverageDistance;
        }

        private void CalcDc()
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
                                    temp = (DataPoints[j] - dp) / sigma;
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

        private double[][] GetFeatureMatrix()
        {
            double[][] ans = new double[DataPoints.Length][];
            for (int i = 0; i < DataPoints.Length; i++)
            {
                ans[i] = DataPoints[i].features.ToArray();
            }
            return ans;
        }

        private void CalculateDistances()
        {
            Program.initThread.Join();
            var ansObj = Program.MatlabMethods.CalculateDistance(1,
                new MWNumericArray(GetFeatureMatrix() as Array),
                new MWCharArray("euclidean"));
            var ans = ansObj[0].ToArray() as double[,];

            /////////////////////////////////////////////
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
                }
            }
            /////////////////////////////////////////////

            Console.WriteLine("Distance calculated!");
        }

        private void CalculateRhos()
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
                        double sum = 0;
                        Distance[i].ToList().ForEach(dis => sum += Math.Exp(-((dis * dis) / (DcSquare))));
                        DataPoints[i].rho = sum - 1;
                        break;
                }
            }

            Console.WriteLine("Rhos calculated!");
        }

        private void CalculateDeltas()
        {
            foreach (DataPoint dp in DataPoints)
            {
                var seniors = DataPoints.Where(p => p.rho > dp.rho);
                if (seniors.Count() > 0)
                {
                    var senior = seniors.OrderBy(p => dp - p).First();
                    dp.delta = dp - senior;
                    dp.senior = senior;
                }
                else
                {
                    dp.delta = DataPoints.Except(new DataPoint[] { dp }).Max(p => dp - p);
                }
            }

            Console.WriteLine("Deltas calculated!");
        }

        private void CalculateTaus()
        {
            foreach (DataPoint dp in DataPoints)
            {
                var juniors = DataPoints.Where(p => p.rho < dp.rho);
                if (juniors.Count() > 0)
                {
                    var junior = juniors.OrderBy(p => dp - p).First();
                    dp.tau = dp - junior;
                }
                else
                {
                    dp.tau = dp.delta;
                }
            }

            Console.WriteLine("Taus calculated!");
        }

        private IEnumerable<DataPoint> GetNeighbours(DataPoint dp) => DataPoints.Where(p => p - dp <= Dc);

        private void AssignClusterID()
        {
            var order = DataPoints.OrderByDescending(dp => dp.rho);

            foreach (var dp in order)
            {
                if (dp.clusterID is null)
                {
                    //dp.clusterID = DataPoints.Where(p => p.rho > dp.rho).OrderBy(p => p - dp).First().clusterID;
                    dp.clusterID = dp.senior.clusterID;
                }
            }
        }

        public void Clustering()
        {
            Thread t1, t2;
            CalculateDistances();
            CalcDc();
            CalculateRhos();
            //CalculateDeltas();
            //CalculateTaus();
            t1 = new Thread(CalculateDeltas);
            t2 = new Thread(CalculateTaus);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            //Determine centroids
            Centroids = new List<DataPoint>(DataPoints.OrderByDescending(dp => dp.rho * (dp.delta - dp.tau)).Take(K));

            //Assign ClusterID
            for (int i = 0; i < Centroids.Count; i++)
            {
                Centroids[i].clusterID = i;
            }
            AssignClusterID();

            var x = DataPoints.Where(dp => dp.clusterID != null);
        }
    }
}
