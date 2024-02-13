using LoteriaKino.code.util;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Text;

namespace LoteriaKino.code.parser
{
    /// <summary>
    /// parser de https://sorteosenvivo.loteria.cl/loteriaweb/resultados/kino
    /// 
    /// comentário: https://www.facebook.com/share/p/nVgC9jeemZ9m6MvA/?mibextid=2JQ9oc
    /// </summary>
    internal class Parser : IDisposable
    {

        private WDUtil wdutil = new();
        IWebDriver? _driver = null;

        /// <summary>
        /// parser
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pathCSV"></param>
        /// <param name="hideBrowser"></param>
        /// <returns></returns>
        public bool doParse(string url, string pathCSV, bool hideBrowser = false)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(pathCSV)) { return false; }
            if (File.Exists(pathCSV)) { File.Delete(pathCSV); }

            _driver = wdutil.getDriver(hideBrowser);
            if (_driver == null) { return false; }

            _driver.Navigate().GoToUrl(url);

            StreamWriter sw = new(File.Open(pathCSV, FileMode.CreateNew), Encoding.UTF8);
            // faz o parser do estado da página atual
            IWebElement? weSorteo = wdutil.wait(By.Id("sorteo"));
            ReadOnlyCollection<IWebElement>? weOptions = weSorteo == null ? null : weSorteo.FindElements(By.TagName("option"));
            if (weOptions != null && weOptions.Count > 0)
            {
                for (int i = 0; i < weOptions.Count; i++)
                {
                    IWebElement option = weOptions[i];
                    if (option == null || !option.Selected) { continue; }
                    parserHtml(sw, option.Text);
                    break;
                }
            }

            // o de índice 0 já foi feito - estado da página atual
            LastIndex li = new(false, 1);
            do
            {
                li = nextSorteio(sw, li.Index);
                Thread.Sleep(200);
            } while (li.Ok);


            sw.Flush(); sw.Close(); sw.Dispose();
            _driver.Quit();

            Console.WriteLine(string.Format("Arquivo salvo em: {0}", pathCSV));
            return true;
        }

        struct LastIndex
        {
            public bool Ok { get; set; }
            public int Index { get; set; }
            public LastIndex(bool ok, int index) { Ok = ok; Index = index; }
        }

        private LastIndex nextSorteio(StreamWriter sw, int index = -1)
        {
            IWebElement? weSorteo = wdutil.wait(By.Id("sorteo"));
            if (weSorteo == null) { return new(false, -1); }

            ReadOnlyCollection<IWebElement>? weOptions = weSorteo.FindElements(By.TagName("option"));
            if (weOptions == null || weOptions.Count <= 0 || index >= weOptions.Count) { return new(false, -1); }

            if (index >= 0)
            {
                IWebElement option = weOptions[index];
                if (option == null) { return new(false, -1); }
                string nSorteio = option.Text; // precisa setar a string por conta da referência
                option.Click();
                Thread.Sleep(500);
                parserHtml(sw, nSorteio);
                return new(true, index + 1);
            }

            for (int i = 0; i < weOptions.Count; i++)
            {
                IWebElement option = weOptions[i];
                if (option == null || option.Selected) { continue; }
                // vai para o próximo select
                string nSorteio = option.Text; // precisa setar a string por conta da referência
                option.Click();
                Thread.Sleep(500);
                parserHtml(sw, nSorteio);
                return new(true, i);
            }
            return new(false, -1);

        }

        private bool parserHtml(StreamWriter sw, string sorteio)
        {
            IWebElement? weTitulo1 = wdutil.wait(By.ClassName("bolitas"));
            if (weTitulo1 == null)
            {
                Console.WriteLine("Ocorreu um erro ao criar o arquivo csv");
                return false;
            }

            ReadOnlyCollection<IWebElement>? welis = weTitulo1.FindElements(By.TagName("li"));
            if (welis == null || welis.Count <= 0)
            {
                Console.WriteLine("Ocorreu um erro ao criar o arquivo csv");
                return false;
            }

            sw.Write("\"" + sorteio + "\";");
            Console.Write("\"" + sorteio + "\";");

            foreach (IWebElement weLi in welis)
            {
                if (weLi == null || string.IsNullOrEmpty(weLi.Text)) { continue; }
                string resultado = string.Format("\"{0}\";", weLi.Text);
                sw.Write(resultado);
                Console.Write(resultado);
            }
            sw.WriteLine();
            Console.WriteLine();
            return true;
        }

        public void Dispose()
        {
            try
            {
                _driver.Quit();
            }
            catch (Exception)
            {
            }
            _driver = null;

            wdutil = null;
        }

    }
}