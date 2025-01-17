/*
 * Copyright 2018 Capnode AB
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

using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Parameters;
using System.Globalization;

namespace Algoloop.Algorithm.CSharp
{
    public class LogQuotesAlgo : QCAlgorithm
    {
        [Parameter("symbols")]
        private readonly string _symbols = null;
        private Symbol _symbol;

        [Parameter("resolution")]
        private readonly string _resolution = null;

        [Parameter("market")]
        private readonly string _market = null;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash
        /// and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            if (string.IsNullOrEmpty(_symbols)) throw new ArgumentNullException(nameof(_symbols));
            if (string.IsNullOrEmpty(_resolution)) throw new ArgumentNullException(nameof(_resolution));
            if (string.IsNullOrEmpty(_market)) throw new ArgumentNullException(nameof(_market));

            EnableAutomaticIndicatorWarmUp = true;
            if (!Enum.TryParse(_resolution, out Resolution resolution)) throw new ArgumentException(nameof(_resolution));
            string symbol = _symbols.Split(';')[0];
            var forex = AddForex(symbol, resolution, _market);
            _symbol = forex.Symbol;
        }

        public override void OnData(Slice slice)
        {
            QuoteBar quote = slice.QuoteBars[_symbol.Value];
            Log(string.Format(
                CultureInfo.InvariantCulture,
                "{0:u}: {1} {2} {3} {4} {5}",
                quote.Time,
                quote.Ask.Open,
                quote.Bid.Open,
                quote.Ask.Close,
                quote.Bid.Close,
                IsMarketOpen(_symbol)));
        }
    }
}
