using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scanDirectory
{
    public class scanDirectory
    {

        public string sourceDir { get; set; }
        public string fileFormat { get; set; }


        public string initScanDir(string scanDir, string fileFormat)
        {
            try
            {
                List<string> scanFiles = Directory.EnumerateFiles(sourceDir, fileFormat, SearchOption.AllDirectories);

                Console.WriteLine("-- Audio Files: --");
                foreach (string file in scanFiles)
                {
                    Console.WriteLine(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }

}
