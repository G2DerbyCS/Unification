using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scanDirectory
{
    public class scanDirectory
    {
        /*Stores the user prodivded directory to be scanned.*/
        public string sourceDir { get; set; }
        /*Stores the type of audio file formats.*/
        public string fileFormat { get; set; }

        /* <param name="sourceDir">Source of directory scanned.</param>
        <param name="fileFormat">Types of files retreived.</param>
        */
        public string initScanDir(string scanDir, string fileFormat)
        {
            
            try
            {
                /*Retreives files (fileFormat) from a directory and subdirectories (sourceDir).
                Could use GetFiles, but Enumeratefiles starts enumerating the collection before the whole collection is return.
                Better performance with EnumerateFiles.
                */
                List<string> scanFiles = Directory.EnumerateFiles(sourceDir, fileFormat, SearchOption.AllDirectories);
                
                /* Outputs to console the result of the scan. This was added for the purpose of testing.*/
                Console.WriteLine("-- Audio Files: --");
                foreach (string file in scanFiles)
                {
                    Console.WriteLine(file);
                }
            }
            /* Exception handler
            */
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }

}
