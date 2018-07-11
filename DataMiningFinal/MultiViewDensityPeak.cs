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
                view.DensityDefinition = densityDefinition;
                view.DcSelection = dcSelection;
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
            Parallel.ForEach(Views, (view) =>
            {
                view.CalculateDistances();
                view.CalcDc();
                view.CalculateRhos();
                for (int i = 0; i < NumOfDataPoints; i++)
                {
                    for (int j = 0; j < NumOfDataPoints; j++)
                    {
                        AbstractDistance[i][j] += (view.Distance[i][j] - view.MinDistance) / (view.MaxDistance - view.MinDistance);
                    }
                }
            });

            //datapoints
            AbstractDataPoints = new DataPoint[NumOfDataPoints];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDataPoints[i] = new DataPoint(i);
                AbstractDataPoints[i].rho = 1d;
                Views.ToList().ForEach(view => AbstractDataPoints[i].rho *= view.DataPoints[i].rho);
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

            DensityPeak[] densityPeaks = new DensityPeak[Views.Length];
            for (int i = 0; i < Views.Length; i++)
            {
                densityPeaks[i] = new DensityPeak(K, Views[i]);
                densityPeaks[i].CalculateDeltas();
                densityPeaks[i].CalculateTaus();
                densityPeaks[i].Clustering(false);
            }

            int[][] sameClusterCount = new int[NumOfDataPoints][];
            double[][] votes = new double[NumOfDataPoints][];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                sameClusterCount[i] = Enumerable.Repeat(0, NumOfDataPoints).ToArray();
                votes[i] = Enumerable.Repeat(0d, K).ToArray();
            }
            foreach (var model in densityPeaks)
            {
                for (int i = 0; i < NumOfDataPoints; i++)
                {
                    for (int j = 0; j < NumOfDataPoints; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        if (model.DataPoints[i].clusterID == model.DataPoints[j].clusterID && !(model.DataPoints[i].clusterID is null))
                        {
                            sameClusterCount[i][j]++;
                        }
                        //if (model.DataPoints[i].clusterID != model.DataPoints[j].clusterID && !(model.DataPoints[i].clusterID is null) && !(model.DataPoints[j].clusterID is null))
                        //{
                        //    samePossibility[i][j]--;
                        //}
                    }
                }
            }

            for (int i = 0; i < NumOfDataPoints; i++)
            {
                for (int j = 0; j < NumOfDataPoints; j++)
                {
                    votes[i][(int)AbstractDensityPeak.DataPoints[j].clusterID] += sameClusterCount[i][j] > 1 ? sameClusterCount[i][j] : 0d;
                }
                votes[i][(int)AbstractDensityPeak.DataPoints[i].clusterID] *= votes[i][(int)AbstractDensityPeak.DataPoints[i].clusterID];
            }

            for (int i = 0; i < NumOfDataPoints; i++)
            {
                double max = votes[i].Max();
                int suppose = votes[i].ToList().IndexOf(max);
                if (AbstractDensityPeak.DataPoints[i].clusterID != suppose)
                {
                    AbstractDensityPeak.DataPoints[i].clusterID = suppose;
                }
            }

            return AbstractDensityPeak.DataPoints;
        }
    }
}
