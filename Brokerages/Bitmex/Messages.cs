using System;

namespace QuantConnect.Brokerages.Bitmex
{
    public enum BitmexSide
    {
        Undefined,
        Buy,
        Sell
    }

    public enum BitmexTickDirection
    {
        Undefined,
        MinusTick,
        PlusTick,
        ZeroMinusTick,
        ZeroPlusTick
    }

    public class Trade
    {
        public DateTime Timestamp { get; set; }
        public string Symbol { get; set; }
        public BitmexSide Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
        public BitmexTickDirection TickDirection { get; set; }
        public string TrdMatchId { get; set; }
        public long? GrossValue { get; set; }
    }
}
