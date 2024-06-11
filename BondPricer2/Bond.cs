public enum BondType
{
    Regular,
    ZeroCoupon,
    FloatingRate,
    Convertible
}

public class Bond
{
    public double FaceValue { get; set; }
    public double CouponRate { get; set; }
    public int Maturity { get; set; } // in years
    public double MarketRate { get; set; }
    public double DefaultProbability { get; set; }
    public BondType Type { get; set; }
    public double FloatingRate { get; set; } // Only for Floating Rate Bonds

    public Bond(double faceValue, double couponRate, int maturity, double marketRate, double defaultProbability, BondType type, double floatingRate = 0.0)
    {
        FaceValue = faceValue;
        CouponRate = couponRate;
        Maturity = maturity;
        MarketRate = marketRate;
        DefaultProbability = defaultProbability;
        Type = type;
        FloatingRate = floatingRate;
    }
}
