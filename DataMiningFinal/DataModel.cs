using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathWorks.MATLAB.NET.Arrays;
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
        public int id { get; set; }
        public double rho { get; set; }
        public double delta { get; set; }
        public double tau { get; set; }
        public DataPoint senior { get; set; }

        public DataPoint(int i)
        {
            id = i;
            senior = this;
        }

        public DataPoint(double[] data)
        {
            features = new DenseVector(data);
            rho = 0;
            senior = this;
        }

        public static double operator -(DataPoint dp1, DataPoint dp2) => (dp1.features - dp2.features).L2Norm();
    }

    public enum DensityDefinition { NumOfNeighbor, GaussianKernal }
    public enum DcSelection { EntropyBased, DistanceBased, NumOfPointsBased, AverageDistance }
}
