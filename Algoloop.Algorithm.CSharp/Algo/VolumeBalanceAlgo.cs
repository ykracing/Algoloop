/*
 * Copyright 2020 Capnode AB
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

using Algoloop.Algorithm.CSharp.Model;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Orders.Fees;
using QuantConnect.Parameters;
using QuantConnect.Securities;
using System.Globalization;

namespace Algoloop.Algorithm.CSharp
{
    public class VolumeBalanceAlgo : QCAlgorithm
    {
        [Parameter("symbols")]
        private readonly string _symbols = null;

        [Parameter("security")]
        private readonly string _security = null;

        [Parameter("resolution")]
        private readonly string _resolution = null;

        [Parameter("market")]
        private readonly string _market = null;

        [Parameter("Fee")]
        private readonly string _fee = "0";

        [Parameter("Period1")]
        private readonly string _period1 = "0";

        [Parameter("Period2")]
        private readonly string _period2 = "0";

        [Parameter("Hold")]
        private readonly string _hold = "1";

        [Parameter("Slots")]
        private readonly string _slots = "1";

        [Parameter("Reinvest")]
        private readonly string _reinvest = "false";

        [Parameter("Rebalance trigger (min)")]
        private readonly string _rebalance = "0";

        [Parameter("Tracker stoploss period1")]
        private readonly string _trackerPeriod1 = "0";

        [Parameter("Tracker stoploss period2")]
        private readonly string _trackerPeriod2 = "0";

        [Parameter("Daily turnover (min)")]
        private readonly string _turnover = "0";

        [Parameter("Daily turnover period")]
        private readonly string _turnoverPeriod = "0";

        public override void Initialize()
        {
            if (string.IsNullOrEmpty(_symbols)) throw new ArgumentNullException(nameof(_symbols));
            if (string.IsNullOrEmpty(_security)) throw new ArgumentNullException(nameof(_security));
            if (string.IsNullOrEmpty(_resolution)) throw new ArgumentNullException(nameof(_resolution));
            if (string.IsNullOrEmpty(_market)) throw new ArgumentNullException(nameof(_market));

            SecurityType securityType = (SecurityType)Enum.Parse(typeof(SecurityType), _security);
            Resolution resolution = (Resolution)Enum.Parse(typeof(Resolution), _resolution);
            decimal fee = decimal.Parse(_fee, CultureInfo.InvariantCulture);
            int period1 = int.Parse(_period1, CultureInfo.InvariantCulture);
            int period2 = int.Parse(_period2, CultureInfo.InvariantCulture);
            int hold = int.Parse(_hold, CultureInfo.InvariantCulture);
            int slots = int.Parse(_slots, CultureInfo.InvariantCulture);
            long turnover = long.Parse(_turnover, CultureInfo.InvariantCulture);
            int turnoverPeriod = int.Parse(_turnoverPeriod, CultureInfo.InvariantCulture);
            bool reinvest = bool.Parse(_reinvest);
            float rebalance = float.Parse(_rebalance, CultureInfo.InvariantCulture);
            int trackerPeriod1 = int.Parse(_trackerPeriod1, CultureInfo.InvariantCulture);
            int trackerPeriod2 = int.Parse(_trackerPeriod2, CultureInfo.InvariantCulture);
            List<Symbol> symbols = _symbols
                .Split(';')
                .Select(x => QuantConnect.Symbol.Create(x, SecurityType.Equity, _market))
                .ToList();

            EnableAutomaticIndicatorWarmUp = true;
            UniverseSettings.Resolution = resolution;
            MarketHoursDatabase.Entry entry = MarketHoursDatabase.GetEntry(_market, (string)null, securityType);
            SetTimeZone(entry.DataTimeZone);
            SetUniverseSelection(new ManualUniverseSelectionModel(symbols));
            SetPortfolioConstruction(new SlotPortfolio(
                slots: slots,
                reinvest: reinvest,
                rebalance: rebalance,
                trackerPeriod1: trackerPeriod1 >= 0 ? trackerPeriod1 : period1,
                trackerPeriod2: trackerPeriod2 >= 0 ? trackerPeriod2 : period2));
            SetExecution(new LimitExecution(slots));
            SetRiskManagement(new NullRiskManagementModel());
            SetBenchmark(QuantConnect.Symbol.Create("OMXSPI.ST", securityType, _market));
            FeeModel feeModel = fee < 1 ? new PercentFeeModel(fee) : new ConstantFeeModel(fee);
            SetSecurityInitializer(security =>
            {
                security.FeeModel = feeModel;
                security.FillModel = new TouchFill();
            });
            int warmupPeriod = Math.Max(period1, Math.Max(period2, turnoverPeriod));
            SetWarmUp((int)(1.1 * warmupPeriod), Resolution.Daily);
            SetAlpha(new MultiSignalAlpha(InsightDirection.Up, resolution, hold, symbols,
                (symbol) => new TurnoverSignal(turnoverPeriod, turnover),
                (symbol) => new VolumeBalanceSignal(period1)));
        }

        public override void OnEndOfAlgorithm()
        {
            PortfolioConstruction.CreateTargets(this, null);
        }
    }
}
