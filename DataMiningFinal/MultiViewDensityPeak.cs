using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MultiViewDensityPeak(int k, View[] views, DensityDefinition densityDefinition, DcSelection dcSelection)
        {
            K = k;
            Views = views;
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
                AbstractDistance[i] = new double[NumOfDataPoints];
                for (int j = 0; j < NumOfDataPoints; j++)
                {
                    AbstractDistance[i][j] = 0;
                }
            }
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine("UsedDistanceEntropy");
            //Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var view in Views)
            {
                view.CalculateDistances();
                view.CalcDc();
                //view.CalculateDistanceEntropy();
                for (int i = 0; i < NumOfDataPoints; i++)
                {
                    for (int j = 0; j < NumOfDataPoints; j++)
                    {
                        AbstractDistance[i][j] += (view.Distance[i][j]) / (view.MaxDistance);// * view.DistanceEntropy);
                        //AbstractDistance[i][j] += Math.Exp(view.Distance[i][j] / (view.MaxDistance));
                    }
                }
            }

            //datapoints
            Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine("UsedRhosEntropy");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var view in Views)
            {
                view.CalculateRhos();
                view.CalculateRhosEntropy();
            }
            AbstractDataPoints = new DataPoint[NumOfDataPoints];
            for (int i = 0; i < NumOfDataPoints; i++)
            {
                AbstractDataPoints[i] = new DataPoint(i);
                AbstractDataPoints[i].rho = 1d;
                Views.ToList().ForEach(view => AbstractDataPoints[i].rho *= view.DataPoints[i].rho);// * view.RhosEntropy);
            }

        }

        public DataPoint[] Clustering()
        {
            DensityPeak abstractDensityPeak = new DensityPeak(K, AbstractDataPoints, DensityDefinition.GaussianKernal, DcSelection.AverageDistance, AbstractDistance);
            abstractDensityPeak.CalculateMaxDistance();
            abstractDensityPeak.CalculateDeltas();
            abstractDensityPeak.CalculateTaus();
            abstractDensityPeak.Clustering(false);
            return abstractDensityPeak.DataPoints;
        }
    }
}
