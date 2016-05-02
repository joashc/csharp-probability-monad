using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProbCSharp;
using static ProbCSharp.ProbBase;

namespace ProbCSharp.Test.Models
{
    public struct BuyData
    {
        public BuyData(double income, double age, bool willBuy)
        {
            Income = income;
            Age = age;
            WillBuy = willBuy;
        }
        public double Income;
        public double Age;
        public bool WillBuy;
    }

    public class BuyWeight
    {
        public BuyWeight(double incomeWeight, double ageWeight)
        {
            IncomeWeight = incomeWeight;
            AgeWeight = ageWeight;
        }
        public double IncomeWeight;
        public double AgeWeight;
    }

    public static class BayesPointMachine
    {
        public static List<BuyData> BpmObserved = new List<BuyData> {
                new BuyData(63, 38, true),
                new BuyData(16, 23, false),
                new BuyData(28, 40, true),
                new BuyData(55, 27, true),
                new BuyData(22, 18, false),
                new BuyData(20, 40, false)
            };

        public static Func<BuyWeight, BuyData, Prob> BpmLikelihood = (w, d) =>
        {
            var noise = Normal(0, 0.1).Sample();
            var lossMagnitude = (w.AgeWeight * d.Age) + (w.IncomeWeight * d.Income) + noise;
            var loss = d.WillBuy ? lossMagnitude : -lossMagnitude;
            return Prob(Sigmoid(loss));
        };

        public static Func<BuyWeight, BuyData, Prob> BpmLikelihood2 = (w, d) =>
        {
            var lossMagnitude = Pdf(NormalC(0, 0.1), (w.AgeWeight * d.Age) - (w.IncomeWeight * d.Income)).Value;
            var loss = d.WillBuy ? lossMagnitude : -lossMagnitude;
            return Prob(loss);
        };

        public static Dist<BuyWeight> BpmUpdate(Dist<BuyWeight> prior, IEnumerable<BuyData> data)
        {
            return data.Aggregate(prior, (dist, datum) => Condition(weight => BpmLikelihood(weight, datum), dist));
        }

        public static Dist<BuyWeight> BpmPrior = from ageWeight in Normal(-1, 1)
                                                 from incomeWeight in Normal(0, 1)
                                                 select new BuyWeight(incomeWeight, ageWeight);


        public static BuyData Person1 = new BuyData(58, 36, true);
        public static BuyData Person2 = new BuyData(18, 24, true);
        public static BuyData Person3 = new BuyData(22, 37, true);

        public static Func<FiniteDist<BuyWeight>, BuyData, Prob> 
        BpmPredict = (dist, datum) =>
            dist.ProbOf(w => BpmLikelihood(w, datum).Value > 0.5);

    }

}
