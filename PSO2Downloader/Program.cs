using System;
using System.Collections.Generic;
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
                Console.WriteLine("pso2downloader /data/win32reboot/9c/a433e75e9cef9c6d0a318bde62bda6 /data/win32/1c5f7a7fbcdd873336048eaf6e26cd87");
                Console.WriteLine("Write the path for any expected file. The author of this tool bears no responsiblity for any issues resulting from its usage.");
                return;
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
            foreach (string file in args)
            {
                Directory.CreateDirectory(directoryToSave + "\\" + Path.GetDirectoryName(file));
                try
                {
                    webClient.Headers.Add("user-agent", "AQUA_HTTP");
                    webClient.DownloadFile(patchUrl + file + ".pat", directoryToSave + "\\" + file);
                    //Example: webClient.DownloadFile(patchUrl + "/data/win32reboot/9c/a433e75e9cef9c6d0a318bde62bda6" + ".pat", directoryToSave + "\\" + "9ca433e75e9cef9c6d0a318bde62bda6");
                    streamWriter.WriteLine(file + "\tpatches");
                    streamWriter.Flush();
                }
                catch (Exception ex1)
                {
                    try
                    {
                        webClient.Headers.Add("user-agent", "AQUA_HTTP");
                        webClient.DownloadFile(backupPatchUrl + file + ".pat", directoryToSave + "\\" + file);
                        streamWriter.WriteLine(file + "\told patches");
                        streamWriter.Flush();
                    }
                    catch (Exception ex2)
                    {
                        streamWriter.WriteLine(file + "\tfailed");
                        streamWriter.Flush();
                    }
                }
            }
            streamWriter.Close();
        }
    }
}
