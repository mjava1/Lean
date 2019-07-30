using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;

namespace QuantConnect.Brokerages.Bitmex
{
    public class BitmexBrokerage : Brokerage, IDataQueueHandler
    {
        private BitemexTradesSubscribe _bitmexTradesSubscribe;
        public List<Tick> Ticks = new List<Tick>();
        
        /// <summary>
        /// Locking object for the Ticks list in the data queue handler
        /// </summary>
        public readonly object TickLocker = new object();

        public BitmexBrokerage() : base("BitmexBrokerage")
        {
            _bitmexTradesSubscribe = new BitemexTradesSubscribe();
        }

        public override bool IsConnected
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool CancelOrder(Order order)
        {
            throw new System.NotImplementedException();
        }

        private void HandleBitMexTrade(Trade trade)
        {
            try
            {                
                var price = Convert.ToDecimal(trade.Price);               
                var symbol = Symbol.Create("XBTUSD",SecurityType.Crypto, Market.Bitmex);
                lock (TickLocker)
                {
                    Ticks.Add(new Tick
                    {
                        Value = price,
                        Time = trade.Timestamp,
                        Symbol = symbol,
                        TickType = TickType.Trade,
                        Quantity = trade.Size
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public override void Connect()
        {
            Task.Run(() => _bitmexTradesSubscribe.Subscribe(HandleBitMexTrade));
        }

        public override void Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public override List<Holding> GetAccountHoldings()
        {
            throw new System.NotImplementedException();
        }

        public override List<CashAmount> GetCashBalance()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<BaseData> GetNextTicks()
        {
            lock (TickLocker)
            {
                var copy = Ticks.ToArray();
                Ticks.Clear();
                return copy;
            }
        }

        public override List<Order> GetOpenOrders()
        {
            throw new System.NotImplementedException();
        }

        public override bool PlaceOrder(Order order)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols)
        {
            throw new System.NotImplementedException();
        }

        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols)
        {
            throw new System.NotImplementedException();
        }

        public override bool UpdateOrder(Order order)
        {
            throw new System.NotImplementedException();
        }
    }
}
