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
    internal class Parser
    {

        public static bool doParse(string url, string pathCSV, bool hideBrowser = false)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(pathCSV))
            {
                Console.WriteLine("Ocorreu um erro ao criar o arquivo csv");
                return false;
            }
            if (File.Exists(pathCSV)) { File.Delete(pathCSV); }

            WDUtil wdutil = new();
            IWebDriver? driver = wdutil.getDriver(hideBrowser);
            if (driver == null)
            {
                Console.WriteLine("Ocorreu um erro ao criar o arquivo csv");
                return false;
            }

            driver.Navigate().GoToUrl(url);


            //bolitas
            IWebElement? weTitulo1 = wdutil.wait(By.ClassName("bolitas"));
            if (weTitulo1 == null)
            {
                Console.WriteLine("Ocorreu um erro ao criar o arquivo csv");
                driver.Quit();
                return false;
            }

            ReadOnlyCollection<IWebElement>? welis = weTitulo1.FindElements(By.TagName("li"));
            if (welis == null || welis.Count <= 0)
            {
                Console.WriteLine("Ocorreu um erro ao criar o arquivo csv");
                driver.Quit();
                return false;
            }

            using (var sw = new StreamWriter(File.Open(pathCSV, FileMode.CreateNew), Encoding.UTF8))
            {
                Console.WriteLine("-----------------------------------------");
                foreach (IWebElement weLi in welis)
                {
                    if (weLi == null || string.IsNullOrEmpty(weLi.Text)) { continue; }
                    sw.WriteLine(weLi.Text);
                    Console.WriteLine(weLi.Text);
                }
                Console.WriteLine("-----------------------------------------");
            }

            driver.Quit();

            Console.WriteLine(string.Format("Arquivo salvo em: {0}", pathCSV));
            return true;
        }

    }
}