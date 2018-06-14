using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;

namespace DataMiningFinal
{
    public enum Algorithms { KMeans, DBSCAN, DensityPeak }
    public enum PointType { CorePoint, BorderPoint, NoisePoint }

    public class DataPoint
    {
        public Vector<double> features { get; private set; }
        public int? clusterID { get; set; }
        public double rho { get; set; }
        public double delta { get; set; }
        public double tau { get; set; }
        public DataPoint senior { get; set; }

        public DataPoint(double[] data)
        {
            features = new DenseVector(data);
            rho = 0;
        }

        public static double operator -(DataPoint dp1, DataPoint dp2) => (dp1.features - dp2.features).L2Norm();
    }
}
