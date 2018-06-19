using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    internal class DensityPeak : View
    {
        internal int K { get; set; }
        private List<DataPoint> Centroids { get; set; }

        public DensityPeak(int k, DataPoint[] dataPoints,
            DensityDefinition densityDefinition = DensityDefinition.GaussianKernal,
            DcSelection dcSelection = DcSelection.AverageDistance,
            double[][] distance = null) : base(dataPoints)
        {
            K = k;
            Distance = distance is null ? new double[DataPoints.Length][] : distance;

            //Configure
            DensityDefinition = densityDefinition;
            DcSelection = dcSelection;
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

        internal void AssignClusterID()
        {
            var order = DataPoints.OrderByDescending(dp => dp.rho);

            foreach (var dp in order)
            {
                if (dp.clusterID is null)
                {
                    dp.clusterID = dp.senior.clusterID;
                }
            }
        }

        public void Clustering(bool needCalcDis = true)
        {
            if (needCalcDis)
            {
                CalculateDistances();
                CalcDc();
                CalculateRhos();
                Thread t1 = new Thread(CalculateDeltas);
                Thread t2 = new Thread(CalculateTaus);
                t1.Start(); t2.Start(); t1.Join(); t2.Join();
            }

            //Determine centroids
            Centroids = new List<DataPoint>(DataPoints.OrderByDescending(dp => dp.rho * (dp.delta - dp.tau)).Take(K));

            //Assign ClusterID
            for (int i = 0; i < Centroids.Count; i++)
            {
                Centroids[i].clusterID = i;
            }
            AssignClusterID();
        }
    }
}
