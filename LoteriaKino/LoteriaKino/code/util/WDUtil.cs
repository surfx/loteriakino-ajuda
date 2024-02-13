using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;
using System.Collections.ObjectModel;
using System.Drawing;

namespace LoteriaKino.code.util
{
    internal class WDUtil
    {

        private const int TIME_WAIT = 10; // segundos

        private IWebDriver? _driver;
        private WebDriverWait? _wait = null;
        private DefaultWait<IWebDriver> _fluentWait = null;

        private readonly string _chromeDriverDirectory = null; // @"C:\Program Files (x86)\webdriver\bin";


        private void configInicial(bool hideBrowser = false)
        {
            const string versao = "Latest"; // Latest, 114.0.5735.90
            new DriverManager().SetUpDriver(new ChromeConfig(), versao, WebDriverManager.Helpers.Architecture.X64);

            ChromeOptions options = new();
            if (hideBrowser)
            {
                options.AddArgument("headless"); // hide
            }
            //options.AddArguments("-incognito"); // modo anônimo
            options.AddArgument("--start-maximized"); // maximize
            options.AddExcludedArgument("enable-automation"); // Chrome is being controlled by automated test software
            options.AddArgument("--enable-javascript"); // habilita javascript
            options.AddArgument("--allow-file-access-from-files");

            //options.AddArgument("disable-infobars"); options.AddArgument("--disable-extensions");

            // don't show 'save password' prompt
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("javascript.enabled", true);


            _driver = string.IsNullOrEmpty(_chromeDriverDirectory) ? new ChromeDriver(options) : new ChromeDriver(_chromeDriverDirectory, options);

            // tamanho e posições iniciais
            _driver.Manage().Window.Size = new Size(904, 803);
            _driver.Manage().Window.Position = new Point(1298, 390);

            _wait = new WebDriverWait(_driver, timeout: TimeSpan.FromSeconds(TIME_WAIT))
            {
                PollingInterval = TimeSpan.FromSeconds(5),
            };
            _fluentWait = new DefaultWait<IWebDriver>(_driver)
            {
                Timeout = TimeSpan.FromSeconds(TIME_WAIT),
                PollingInterval = TimeSpan.FromSeconds(5)
            };

        }


        public IWebDriver? getDriver(bool hideBrowser = false)
        {
            if (_driver == null) { configInicial(hideBrowser); }
            if (_driver == null)
            {
                Console.WriteLine("Erro ao iniciar o webdriver");
                return null;
            }
            return _driver;
        }


        public string getHtml(
            string url, bool hideBrowser = false
        )
        {
            if (string.IsNullOrEmpty(url)) { return string.Empty; }
            if (_driver == null) { getDriver(hideBrowser); }
            if (_driver == null) { return string.Empty; }
            _driver.Navigate().GoToUrl(url);
            string rt = _driver.PageSource;
            _driver.Quit();
            return rt;
        }

        public bool saveHtml(
            string url, string path, bool overwrite = false, bool hideBrowser = false
        )
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path)) { return false; }
            if (File.Exists(path) && !overwrite) { return false; }
            if (File.Exists(path)) { File.Delete(path); }
            if (File.Exists(path)) { return false; }

            string html = getHtml(url, hideBrowser);
            if (string.IsNullOrEmpty(html)) { return false; }
            StreamWriter f = File.CreateText(path);
            f.Write(html);
            f.Flush(); f.Close(); f.Dispose();
            return true;
        }

        public IWebDriver? loadFromHtml(string path, bool hideBrowser = false)
        {
            if (_driver == null) { getDriver(hideBrowser); }
            if (_driver == null) { return null; }

            while (path.Contains(@"\")) { path = path.Replace(@"\", "/"); }
            if (!path.StartsWith("file:///")) { path = "file:///" + path; }
            _driver.Navigate().GoToUrl(path);
            return _driver;
        }


        #region wait

        public IWebElement? wait(By by, bool fluentWait = true)
        {
            if (_wait == null || _driver == null || by == null) { return null; }
            return fluentWait ? waitFluent(by) : waitExplicit(by);
        }

        public ReadOnlyCollection<IWebElement>? waitEls(By by, bool fluentWait = true)
        {
            if (_wait == null || _driver == null || by == null) { return null; }
            return fluentWait ? waitFluentEls(by) : waitExplicitEls(by);
        }

        #region WebDriverWait
        private IWebElement? waitExplicit(By by)
        {
            if (_wait == null || _driver == null || by == null) { return null; }
            _wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            try { return _wait.Until(drv => drv.FindElement(by)); }
            catch (Exception) { return null; }
        }

        private ReadOnlyCollection<IWebElement>? waitExplicitEls(By by)
        {
            if (_wait == null || _driver == null || by == null) { return null; }
            _wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            try { return _wait.Until(drv => drv.FindElements(by)); }
            catch (Exception) { return null; }
        }
        #endregion
        //IWebElement btnLogar

        #region FluentWait
        // https://www.selenium.dev/pt-br/documentation/webdriver/waits/
        private IWebElement? waitFluent(By by)
        {
            if (_fluentWait == null || _driver == null || by == null) { return null; }
            _fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            try { return _fluentWait.Until(drv => drv.FindElement(by)); }
            catch (Exception) { return null; }
        }


        private ReadOnlyCollection<IWebElement>? waitFluentEls(By by)
        {
            if (_fluentWait == null || _driver == null || by == null) { return null; }
            _fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            try { return _fluentWait.Until(drv => drv.FindElements(by)); }
            catch (Exception) { return null; }
        }

        #endregion

        #endregion

    }
}
