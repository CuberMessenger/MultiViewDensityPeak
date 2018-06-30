using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DataMiningFinal
{
    internal class DensityPeak : View
    {
        internal int K { get; set; }
        private List<DataPoint> Centroids { get; set; }

        public DensityPeak(int k, DataPoint[] dataPoints,
            string distanceMetric,
            DensityDefinition densityDefinition = DensityDefinition.GaussianKernal,
            DcSelection dcSelection = DcSelection.AverageDistance,
            double[][] distance = null) : base(dataPoints, distanceMetric)
        {
            K = k;
            Distance = distance is null ? new double[DataPoints.Length][] : distance;
            for (int i = 0; i < DataPoints.Length; i++)
            {
                DataPoints[i].id = i;
            }

            //Configure
            DensityDefinition = densityDefinition;
            DcSelection = dcSelection;
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

        internal void FindMaxDistance()
        {
            MaxDistance = double.MinValue;
            foreach (var line in Distance)
            {
                foreach (var dis in line)
                {
                    MaxDistance = Math.Max(MaxDistance, dis);
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
