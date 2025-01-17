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
using Algoloop.ViewModel.Internal.Lean;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantConnect;
using QuantConnect.Logging;
using System;
using System.IO;
using static Algoloop.Model.BacktestModel;

namespace Algoloop.Tests.Lean
{
    [TestClass()]
    public class LeanLauncherTests
    {
        private SettingModel _settings;
        private string _exeFolder;

        [TestInitialize()]
        public void Initialize()
        {
            Log.LogHandler = new ConsoleLogHandler();

            _exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string dataFolder = Path.Combine(_exeFolder, "Data");

            if (Directory.Exists(dataFolder))
            {
                Directory.Delete(dataFolder, true);
            }

            MainService.CopyDirectory(Path.Combine("Content", "ProgramData"), "Data", true);

            _settings = new SettingModel { DataFolder = dataFolder };
        }

        [TestMethod()]
        public void RunTest()
        {
            var provider = new ProviderModel()
            {
                Name = nameof(AccountModel.AccountType.Backtest)
            };
            var account = new AccountModel()
            {
                Provider = provider
            };
            var backtest = new BacktestModel
            {
                Account = provider.Name,
                AlgorithmLanguage = Language.CSharp,
                AlgorithmLocation = Path.Combine(_exeFolder, "QuantConnect.Algorithm.CSharp.dll"),
                AlgorithmName = "BasicTemplateFrameworkAlgorithm",
                StartDate = new DateTime(2016, 01, 01)
            };

            using var launcher = new LeanLauncher();
            launcher.Run(backtest, account, _settings, _exeFolder);

            Assert.IsTrue(backtest.Status.Equals(CompletionStatus.Success));
            Assert.IsFalse(backtest.Active);
            Assert.IsNotNull(backtest.Logs);
            Assert.IsTrue(backtest.Logs.Length > 0);
            Assert.IsNotNull(backtest.Result);
            Assert.IsTrue(backtest.Result.Length > 0);
        }
    }
}
