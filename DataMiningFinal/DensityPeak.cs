using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DataMiningFinal
{
    internal class DensityPeak : View
    {
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

        internal void WriteDataPoints(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var dp in DataPoints)
                {
                    for (int i = 0; i < dp.features.Count; i++)
                    {
                        sw.Write(dp.features[i] + ",");
                    }
                    sw.Write(dp.rho + ",");
                    sw.WriteLine(dp.delta);
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
                CalculateDeltas();
                CalculateTaus();
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
