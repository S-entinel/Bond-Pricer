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
        lock (lockObject)
        {
            if (rand.NextDouble() < bond.DefaultProbability)
            {
                return 0.0;
            }
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
        double conversionValue = stockPrice * GetConversionRatio(bond);

        return Math.Max(pv, conversionValue);
    }

    private static double GetStockPriceAtMaturity(int maturity, bool antithetic)
    {
        double rate = antithetic ? 1.0 - rand.NextDouble() : rand.NextDouble();
        
        // Simulate stock price at maturity using a simple geometric Brownian motion model.
        double initialStockPrice = 100.0;
        double drift = 0.05; // Expected return
        double volatility = 0.2; // Stock price volatility
        double time = maturity;

        double stockPrice = initialStockPrice * Math.Exp((drift - 0.5 * volatility * volatility) * time + volatility * Math.Sqrt(time) * rate);
        return stockPrice;
    }

    private static double GetConversionRatio(Bond bond)
    {
        // Assuming a fixed conversion ratio for simplicity.
        return 10.0; // Example: each bond can be converted into 10 shares of stock
    }
}

