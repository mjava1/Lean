﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.IO;

namespace QuantConnect.Data.UniverseSelection
{
    /// <summary>
    /// Custom base data class used for <see cref="ConstituentsUniverse"/>
    /// </summary>
    public class ConstituentsUniverseData : BaseData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoarseFundamental"/> class
        /// </summary>
        public ConstituentsUniverseData()
        {
            DataType = MarketDataType.Auxiliary;
        }

        /// <summary>
        /// The end time of this data.
        /// </summary>
        public override DateTime EndTime
        {
            get { return Time + QuantConnect.Time.OneDay; }
            set { Time = value - QuantConnect.Time.OneDay; }
        }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            var universe = config.MappedSymbol.Substring(config.MappedSymbol.LastIndexOf('-') + 1).ToLower();

            if (isLiveMode)
            {
                date = date.AddDays(-1);
            }
            var path = Path.Combine(Globals.DataFolder,
                config.SecurityType.SecurityTypeToLower(),
                config.Market,
                "universes",
                config.Resolution.ResolutionToLower(),
                universe,
                $"{date:yyyyMMdd}.csv");
            return new SubscriptionDataSource(path, SubscriptionTransportMedium.LocalFile, FileFormat.Csv);
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object
        /// each time it is called.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            try
            {
                var csv = line.Split(',');
                var preselected = new ConstituentsUniverseData
                {
                    Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]),
                    Time = isLiveMode ? date.AddDays(-1) : date
                };

                return preselected;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
