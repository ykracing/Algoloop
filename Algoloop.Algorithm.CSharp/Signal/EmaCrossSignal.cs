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

using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Indicators;

namespace Algoloop.Algorithm.CSharp.Model
{
    public class EmaCrossSignal : ISignal
    {
        private readonly ExponentialMovingAverage _fastEma;
        private readonly ExponentialMovingAverage _slowEma;
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;

        public EmaCrossSignal(QCAlgorithm algorithm, int fastPeriod, int slowPeriod)
        {
            _ = algorithm;
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
            if (fastPeriod > 0)
            {
                _fastEma = new ExponentialMovingAverage(fastPeriod);
            }
            if (slowPeriod > 0)
            {
                _slowEma = new ExponentialMovingAverage(slowPeriod);
            }
        }

        public float Update(QCAlgorithm algorithm, BaseData bar)
        {
            decimal close = bar.Price;
            //algorithm.Log($"{bar.Time:d} {bar.Symbol.ID.Symbol} {close}");
            _fastEma.Update(bar.Time, close);
            _slowEma.Update(bar.Time, close);
            if (_fastEma == null && _slowEma == null) return float.NaN;
            if (_fastEma == null || _slowEma == null) return 0;
            if (_fastPeriod >= _slowPeriod) return 0;
            if (!_fastEma.IsReady || !_slowEma.IsReady) return 0;
            //algorithm.Log($"{bar.Time:u} {bar.Symbol.ID.Symbol} _fastEma={_fastEma} _slowEma={_slowEma}");
            return _fastEma > _slowEma ? 1 : 0;
        }

        public void Done()
        {
        }
    }
}

