using System;
using System.Threading.Tasks;

public class MonteCarloSimulator
{
    private static readonly Random rand = new Random();
    private static readonly object lockObject = new object();

    public static double CalculatePrice(Bond bond, int numSimulations)
    {
        double totalPV = 0.0;
        double totalPVAntithetic = 0.0;

        Parallel.For(0, numSimulations, () => (0.0, 0.0), (i, state, local) =>
        {
            double pv = SimulateBondPrice(bond);
            double pvAntithetic = SimulateBondPrice(bond, antithetic: true);
            local.Item1 += pv;
            local.Item2 += pvAntithetic;
            return local;
        },
        local =>
        {
            lock (lockObject)
            {
                totalPV += local.Item1;
                totalPVAntithetic += local.Item2;
            }
        });

        double averagePV = (totalPV + totalPVAntithetic) / (2 * numSimulations);
        return averagePV;
    }

    private static double SimulateBondPrice(Bond bond, bool antithetic = false)
    {
        double pv = 0.0;
        bool defaulted = false;

        switch (bond.Type)
        {
            case BondType.Regular:
                pv = SimulateRegularBond(bond, antithetic);
                break;
            case BondType.ZeroCoupon:
                pv = SimulateZeroCouponBond(bond, antithetic);
                break;
            case BondType.FloatingRate:
                pv = SimulateFloatingRateBond(bond, antithetic);
                break;
            case BondType.Convertible:
                pv = SimulateConvertibleBond(bond, antithetic);
                break;
        }

        return pv;
    }

    private static double SimulateRegularBond(Bond bond, bool antithetic)
    {
        double couponPayment = bond.FaceValue * bond.CouponRate;
        double pv = 0.0;

        for (int t = 1; t <= bond.Maturity; t++)
        {
            lock (lockObject)
            {
                if (rand.NextDouble() < bond.DefaultProbability)
                {
                    return 0.0;
                }
            }
            double rate = antithetic ? 1.0 - rand.NextDouble() : rand.NextDouble();
            pv += couponPayment / Math.Pow(1 + bond.MarketRate, t);
        }

        pv += bond.FaceValue / Math.Pow(1 + bond.MarketRate, bond.Maturity);
        return pv;
    }

    private static double SimulateZeroCouponBond(Bond bond, bool antithetic)
    {
        if (rand.NextDouble() < bond.DefaultProbability)
        {
            return 0.0;
        }

        double rate = antithetic ? 1.0 - rand.NextDouble() : rand.NextDouble();
        return bond.FaceValue / Math.Pow(1 + bond.MarketRate, bond.Maturity);
    }

    private static double SimulateFloatingRateBond(Bond bond, bool antithetic)
    {
        double pv = 0.0;

        for (int t = 1; t <= bond.Maturity; t++)
        {
            lock (lockObject)
            {
                if (rand.NextDouble() < bond.DefaultProbability)
                {
                    return 0.0;
                }
            }

            double rate = antithetic ? 1.0 - rand.NextDouble() : rand.NextDouble();
            double adjustedRate = bond.MarketRate + bond.FloatingRate * rate;
            double couponPayment = bond.FaceValue * adjustedRate;
            pv += couponPayment / Math.Pow(1 + adjustedRate, t);
        }

        pv += bond.FaceValue / Math.Pow(1 + bond.MarketRate, bond.Maturity);
        return pv;
    }

    private static double SimulateConvertibleBond(Bond bond, bool antithetic)
    {
        double pv = SimulateRegularBond(bond, antithetic);
        double stockPrice = GetStockPriceAtMaturity(bond.Maturity, antithetic);
        double conversionValue = stockPrice * GetConversionRatio();

        return Math.Max(pv, conversionValue);
    }

    private static double GetStockPriceAtMaturity(int maturity, bool antithetic)
    {
        double rate = antithetic ? 1.0 - rand.NextDouble() : rand.NextDouble();
        // Placeholder: Implement a method to simulate or retrieve the stock price at bond maturity
        return 100.0 + rate * 10.0;
    }

    private static double GetConversionRatio()
    {
        // Placeholder: Implement logic to return the conversion ratio for convertible bonds
        return 1.0;
    }
}
