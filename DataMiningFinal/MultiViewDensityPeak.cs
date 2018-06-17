using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiningFinal
{
    internal class MultiViewDensityPeak
    {
        private int NumOfDataPoints { get; set; }
        private DensityPeak[] Views { get; set; }
        private DataPoint[] AbstractDataPoints { get; set; }
        private double[][] AbstractDistance { get; set; }

        public MultiViewDensityPeak(DensityPeak[] views)
        {
            Views = views;
            NumOfDataPoints = Views.First().DataPoints.Length;
        }

        public void ConstructAbstractData()
        {
            //distance
            AbstractDistance = new double[NumOfDataPoints][];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDistance[i] = new double[NumOfDataPoints];
                for (int j = 0; j < NumOfDataPoints; j++)
                {
                    AbstractDistance[i][j] = 0;
                }
            }
            foreach (var view in Views)
            {
                view.CalculateDistances();
                view.CalcDc();
                view.CalculateDistanceEntropy();
                for (int i = 0; i < NumOfDataPoints; i++)
                {
                    for (int j = 0; j < NumOfDataPoints; j++)
                    {
                        AbstractDistance[i][j] += view.Distance[i][j] / view.MaxDistance;
                    }
                }
            }

            //datapoints
            foreach (var view in Views)
            {
                view.CalculateRhos();
            }
            AbstractDataPoints = new DataPoint[NumOfDataPoints];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDataPoints[i] = new DataPoint(i);
                Views.ToList().ForEach(view => AbstractDataPoints[i].rho += view.DataPoints[i].rho);
            }

        }

        public DataPoint[] Clustering()
        {
            DensityPeak abs = new DensityPeak(Views.First().K, AbstractDataPoints, AbstractDistance);
            abs.CalculateDeltas();
            abs.CalculateTaus();
            abs.AssignSeniors();
            abs.Clustering(false);
            return abs.DataPoints;
        }
    }
}
