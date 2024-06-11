using System;

namespace BondPricer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                double faceValue = GetValidatedInput("Enter Face Value: ");
                BondType type = GetBondType();

                double couponRate = 0.0;
                double floatingRate = 0.0;
                if (type != BondType.ZeroCoupon)
                {
                    couponRate = GetValidatedInput("Enter Coupon Rate (as decimal, e.g., 0.05 for 5%): ");
                }
                if (type == BondType.FloatingRate)
                {
                    floatingRate = GetValidatedInput("Enter Floating Rate Spread (as decimal, e.g., 0.02 for 2%): ");
                }

                int maturity = (int)GetValidatedInput("Enter Maturity (in years): ", isInteger: true);
                double marketRate = GetValidatedInput("Enter Market Rate (as decimal, e.g., 0.03 for 3%): ");
                double defaultProbability = GetValidatedInput("Enter Default Probability (as decimal, e.g., 0.02 for 2%): ");
                int numSimulations = (int)GetValidatedInput("Enter Number of Simulations: ", isInteger: true);

                Bond bond = new Bond(faceValue, couponRate, maturity, marketRate, defaultProbability, type, floatingRate);
                double price = MonteCarloSimulator.CalculatePrice(bond, numSimulations);

                Console.WriteLine($"The bond price is: {price:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static double GetValidatedInput(string prompt, bool isInteger = false)
        {
            double result;

            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                try
                {
                    if (isInteger)
                    {
                        result = Convert.ToInt32(input);
                    }
                    else
                    {
                        result = Convert.ToDouble(input);
                    }
                    break;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("Input is too large or too small. Please enter a valid number.");
                }
            }

            return result;
        }

        static BondType GetBondType()
        {
            while (true)
            {
                Console.Write("Enter Bond Type (Regular, ZeroCoupon, FloatingRate, Convertible): ");
                string input = Console.ReadLine().Trim().ToLower();

                switch (input)
                {
                    case "regular":
                        return BondType.Regular;
                    case "zerocoupon":
                        return BondType.ZeroCoupon;
                    case "floatingrate":
                        return BondType.FloatingRate;
                    case "convertible":
                        return BondType.Convertible;
                    default:
                        Console.WriteLine("Invalid input. Please enter one of the specified bond types.");
                        break;
                }
            }
        }
    }
}
