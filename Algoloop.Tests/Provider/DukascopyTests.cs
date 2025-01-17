/*
 * Copyright 2019 Capnode AB
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

using Algoloop.Model;
using Algoloop.ViewModel.Internal.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using System;
using System.IO;
using System.Linq;

namespace Algoloop.Tests.Provider
{
    [TestClass()]
    public class DukascopyTests
    {
        private const string DataFolder = "Data";
        private const string TestData = "TestData";
        private const string MarketHours = "market-hours";
        private const string SymbolProperties = "symbol-properties";

        private string _forexFolder;

        [TestInitialize()]
        public void Initialize()
        {
            Log.LogHandler = new ConsoleLogHandler();

            // Set Globals
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataFolder);
            Config.Set("data-directory", dataFolder);
            Config.Set("data-folder", dataFolder);
            Config.Set("cache-location", dataFolder);
            Config.Set("version-id", string.Empty);
            Globals.Reset();

            _forexFolder = Path.Combine(DataFolder, SecurityType.Forex.SecurityTypeToLower(), Market.Dukascopy);

            // Remove temp dirs
            foreach (string dir in Directory.EnumerateDirectories(".", "temp*", SearchOption.TopDirectoryOnly))
            {
                Directory.Delete(dir, true);
            }

            // Prepare datafolder
            if (Directory.Exists(DataFolder))
            {
                Directory.Delete(DataFolder, true);
            }
            MainService.CopyDirectory(
                Path.Combine(TestData, MarketHours),
                Path.Combine(DataFolder, MarketHours));
            MainService.CopyDirectory(
                Path.Combine(TestData, SymbolProperties),
                Path.Combine(DataFolder, SymbolProperties));
        }

        [TestMethod()]
        public void Download_no_symbols()
        {
            var utcStart = new DateTime(2019, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date,
                Resolution = Resolution.Daily
            };

            // Just update symbol list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            provider.GetUpdate(market, null);

            Assert.IsFalse(market.Active);
            Assert.AreEqual(date, market.LastDate);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(0, market.Symbols.Where(m => m.Active).Count());
        }

        [TestMethod()]
        public void Download_one_symbol()
        {
            var utcStart = new DateTime(2019, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date,
                Resolution = Resolution.Daily
            };
            market.Symbols.Add(new SymbolModel("EURUSD", Market.Dukascopy, SecurityType.Forex));

            // Dwonload symbol and update list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            provider.GetUpdate(market, null);

            Assert.IsTrue(market.Active);
            Assert.AreEqual(date.AddDays(1), market.LastDate);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(1, market.Symbols.Where(m => m.Active).Count());
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "daily", "eurusd.zip")));
        }

        [TestMethod()]
        public void Download_two_symbols()
        {
            var utcStart = new DateTime(2019, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date,
                Resolution = Resolution.Daily
            };
            market.Symbols.Add(new SymbolModel("EURUSD", Market.Dukascopy, SecurityType.Forex));
            market.Symbols.Add(new SymbolModel("GBPUSD", Market.Dukascopy, SecurityType.Forex));

            // Dwonload symbol and update list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            provider.GetUpdate(market, null);

            Assert.IsTrue(market.Active);
            Assert.AreEqual(date.AddDays(1), market.LastDate);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(2, market.Symbols.Where(m => m.Active).Count());
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "daily", "eurusd.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "daily", "gbpusd.zip")));
        }

        [TestMethod()]
        public void Download_two_symbols_tick()
        {
            var utcStart = new DateTime(2019, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date,
                Resolution = Resolution.Tick
            };
            market.Symbols.Add(new SymbolModel("EURUSD", Market.Dukascopy, SecurityType.Forex));
            market.Symbols.Add(new SymbolModel("GBPUSD", Market.Dukascopy, SecurityType.Forex));

            // Dwonload symbol and update list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            provider.GetUpdate(market, null);

            Assert.IsTrue(market.Active);
            Assert.AreEqual(date.AddDays(1), market.LastDate);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(2, market.Symbols.Where(m => m.Active).Count());
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "daily", "eurusd.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "daily", "gbpusd.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "hour", "eurusd.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "hour", "gbpusd.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "minute", "eurusd", "20190501_quote.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "minute", "gbpusd", "20190501_quote.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "second", "eurusd", "20190501_quote.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "second", "gbpusd", "20190501_quote.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "tick", "eurusd", "20190501_quote.zip")));
            Assert.IsTrue(File.Exists(Path.Combine(_forexFolder, "tick", "gbpusd", "20190501_quote.zip")));
        }

        [TestMethod()]
        [ExpectedException(typeof(ApplicationException), "An invalid symbol name was accepted")]
        public void Download_invalid_symbol()
        {
            var utcStart = new DateTime(2019, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date,
                Resolution = Resolution.Daily
            };
            market.Symbols.Add(new SymbolModel("noname", Market.Dukascopy, SecurityType.Forex));

            // Dwonload symbol and update list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            Assert.IsNotNull(provider);

            provider.GetUpdate(market, null);
            Assert.IsFalse(market.Active);
            Assert.AreEqual(market.LastDate, date);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(0, market.Symbols.Where(m => m.Active).Count());
        }

        [TestMethod()]
        public void Download_today()
        {
            DateTime utcStart = DateTime.UtcNow.Date;
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date,
                Resolution = Resolution.Second
            };
            market.Symbols.Add(new SymbolModel("EURUSD", Market.Dukascopy, SecurityType.Forex));

            // Dwonload symbol and update list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            provider.GetUpdate(market, null);

            Assert.IsFalse(market.Active);
            Assert.AreEqual(date, market.LastDate);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(1, market.Symbols.Where(m => m.Active).Count());
        }

        [TestMethod()]
        public void Download_yesterday()
        {
            DateTime utcStart = DateTime.UtcNow.Date;
            var date = utcStart.ToLocalTime();
            var market = new ProviderModel
            {
                Active = true,
                Name = "Dukascopy",
                Provider = Market.Dukascopy,
                LastDate = date.AddDays(-1),
                Resolution = Resolution.Second
            };
            market.Symbols.Add(new SymbolModel("EURUSD", Market.Dukascopy, SecurityType.Forex));

            // Dwonload symbol and update list
            using IProvider provider = ProviderFactory.CreateProvider(market.Provider);
            provider.GetUpdate(market, null);

            Assert.IsTrue(market.Active);
            Assert.AreEqual(date, market.LastDate);
            Assert.AreEqual(78, market.Symbols.Count);
            Assert.AreEqual(1, market.Symbols.Where(m => m.Active).Count());
        }
    }
}
