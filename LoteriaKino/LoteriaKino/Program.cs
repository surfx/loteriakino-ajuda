using LoteriaKino.code.parser;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Display the number of command line arguments.
        string url = "https://sorteosenvivo.loteria.cl/loteriaweb/resultados/kino";
        Console.WriteLine(string.Format("Analisando: {0}", url));

        string pathCSV = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "saida.csv";
        //Console.WriteLine(pathCSV);

        if (Parser.doParse(url, pathCSV, false)) {
            Process.Start("explorer.exe", pathCSV);
        }
    }
}