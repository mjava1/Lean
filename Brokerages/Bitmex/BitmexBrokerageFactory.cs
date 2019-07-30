using System;
using System.Collections.Generic;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Util;
using RestSharp;

namespace QuantConnect.Brokerages.Bitmex
{
    /// <summary>
    /// Factory method to create bitmex Websockets brokerage
    /// </summary>
    public class BitmexBrokerageFactory : BrokerageFactory
    {
        /// <summary>
        /// Factory constructor
        /// </summary>
        public BitmexBrokerageFactory() : base(typeof(BitmexBrokerage))
        {
        }

        /// <summary>
        /// Not required
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// provides brokerage connection data
        /// </summary>
        public override Dictionary<string, string> BrokerageData => new Dictionary<string, string>
        {
            { "bitmex-rest" , Config.Get("bitmex-rest", "https://api.bitmex.com")},
            { "bitmex-url" , Config.Get("bitmex-url", "wss://api.bitmex.com/ws")},
            { "bitmex-api-key", Config.Get("bitmex-api-key")},
            { "bitmex-api-secret", Config.Get("bitmex-api-secret")}
        };

        /// <summary>
        /// The brokerage model
        /// </summary>
        public override IBrokerageModel BrokerageModel => new BitfinexBrokerageModel();

        /// <summary>
        /// Create the Brokerage instance
        /// </summary>
        /// <param name="job"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        public override IBrokerage CreateBrokerage(Packets.LiveNodePacket job, IAlgorithm algorithm)
        {
            var required = new[] { "bitmex-rest", "bitmex-url", "bitmex-api-secret", "bitmex-api-key" };

            foreach (var item in required)
            {
                if (string.IsNullOrEmpty(job.BrokerageData[item]))
                    throw new Exception($"bitmexBrokerageFactory.CreateBrokerage: Missing {item} in config.json");
            }

            var priceProvider = new ApiPriceProvider(job.UserId, job.UserToken);

            var brokerage = new BitmexBrokerage();

            //var brokerage = new BitmexBrokerage(
            //    job.BrokerageData["bitmex-url"],
            //    job.BrokerageData["bitmex-rest"],
            //    job.BrokerageData["bitmex-api-key"],
            //    job.BrokerageData["bitmex-api-secret"],
            //    algorithm,
            //    priceProvider);

            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }
    }
}
