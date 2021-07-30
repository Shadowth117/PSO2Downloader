using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;
using System.Net;

namespace PSO2Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Usage -");
                Console.WriteLine("pso2downloader data/win32reboot/9c/a433e75e9cef9c6d0a318bde62bda6");
                Console.WriteLine(@"pso2downloader C:\Some\Path\ToTxt\testPatch.txt");
                Console.WriteLine(@"pso2downloader /data/win32/d596292bdefd54f2673b67f9fa313b52 C:\Some\Path\ToTxt\testPatch.txt C:\Some\OtherPath\ToOTherTxt\testPatch2.txt");
                Console.WriteLine("Write the path for any expected file. You may also add .txt files containing a list of files (though you may not put a .txt file list reference inside another.");
                Console.WriteLine("Either method can be used in combination for as many files as windows will let you apply as an argument.");
                Console.WriteLine("The author of this tool bears no responsiblity for any issues resulting from its usage.");
                return;
            }
            #if DEBUG
            Debugger.Launch();
            #endif
            //Gather files
            List<string> files = new List<string>();
            foreach(string arg in args)
            {
                if(arg.Contains(":"))
                {
                    files.AddRange(File.ReadAllLines(arg));
                } else
                {
                    files.Add(arg);
                }
            }
            

            string directoryToSave = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\patchData\\";
            Directory.CreateDirectory(directoryToSave);
            StreamWriter streamWriter = new StreamWriter(directoryToSave + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString("d2") + "_" 
                + DateTime.Now.Day.ToString("d2") + "_" + DateTime.Now.Hour.ToString("d2") + "." + DateTime.Now.Minute.ToString("d2") + "." + DateTime.Now.Second.ToString("d2") + ".txt");
            WebClient webClient = new WebClient();

            //Get management text
            var management = webClient.DownloadData("http://patch01.pso2gs.net/patch_prod/patches/management_beta.txt");
            StreamReader reader = new StreamReader(new MemoryStream(management));
            List<string> lines = new List<string>();
            while(!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            //Get proper patch urls from management text
            var fields = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var equalsIndex = line.IndexOf('=');
                if (equalsIndex == -1)
                    continue;

                var key = line.Substring(0, equalsIndex);
                var value = line.Substring(equalsIndex + 1);

                fields[key] = value;
            }
            var masterUrl = fields["MasterURL"];
            var patchUrl = fields["PatchURL"];
            var backupMasterUrl = fields["BackupMasterURL"];
            var backupPatchUrl = fields["BackupPatchURL"];

            //Go through given files
            foreach (string file in files)
            {
                //Account for extra lines from the user
                if (file == "")
                {
                    continue;
                }
                string fileString;
                if(file[0] == '/' && file.Length > 1)
                {
                    fileString = file.Substring(1);
                } else
                {
                    fileString = file;
                }
                Directory.CreateDirectory(directoryToSave + "\\" + Path.GetDirectoryName(fileString));
                try
                {
                    webClient.Headers.Add("User-Agent", "AQUA_HTTP");
                    webClient.DownloadFile(patchUrl + fileString + ".pat", directoryToSave + "\\" + fileString);
                    streamWriter.WriteLine(fileString + "\tpatches");
                    streamWriter.Flush();
                }
                catch (Exception ex1)
                {
                    try
                    {
                        webClient.Headers.Add("User-Agent", "AQUA_HTTP");
                        webClient.DownloadFile(backupPatchUrl + fileString + ".pat", directoryToSave + "\\" + fileString);
                        streamWriter.WriteLine(fileString + "\told patches");
                        streamWriter.Flush();
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            webClient.Headers.Add("User-Agent", "AQUA_HTTP");
                            webClient.DownloadFile(masterUrl + fileString + ".pat", directoryToSave + "\\" + fileString);
                            streamWriter.WriteLine(fileString + "\tmaster");
                            streamWriter.Flush();
                        }
                        catch (Exception ex3)
                        {
                            try
                            {
                                webClient.Headers.Add("User-Agent", "AQUA_HTTP");
                                webClient.DownloadFile(backupMasterUrl + fileString + ".pat", directoryToSave + "\\" + fileString);
                                streamWriter.WriteLine(fileString + "\told master");
                                streamWriter.Flush();
                            }
                            catch (Exception ex4)
                            {
                            }
                        }
                    }
                }
            }
            streamWriter.Close();
        }
    }
}
