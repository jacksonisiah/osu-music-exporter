using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using TagLib;

namespace osume
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("osu! music exporter");
            
            var osuPath = GetOsuDirectory(); 
            var outputPath = GetOutputDirectory();
            
            // give user 5 seconds to confirm paths 
            Console.WriteLine("starting import in 5 seconds... (ctrl+c to quit)");
            //Thread.Sleep(5000);
            
            Export(osuPath, outputPath);
            Console.ReadKey();
        }

        private static void Export(string osuPath, string outputPath)
        {
            var di = new DirectoryInfo(osuPath);
            int count = 0;
            var searchPattern = new Regex(@"$(?<=\.(ogg|wav|m4a|mp3))", RegexOptions.IgnoreCase);
            
            if (!Directory.Exists(osuPath))
            {
                Console.WriteLine("your osu! directory is invalid.");
                Environment.Exit(0);
            }

            foreach (var fi in di.EnumerateFiles("*", SearchOption.AllDirectories)
               .Where(f => searchPattern.IsMatch(f.Name)))
                {
                    try
                    {
                        var tfile = TagLib.File.Create(@fi.FullName);
                        if (tfile.Properties.Duration > TimeSpan.FromSeconds(30))
                        {
                            fi.CopyTo(outputPath + fi.Name);
                            count++;
                        }
                    }
                    // duplicate audio file most likely. 
                    catch (IOException e)
                    {
                        var tfile = TagLib.File.Create(@fi.FullName);
                        if (tfile.Properties.Duration > TimeSpan.FromSeconds(30))
                        {
                            fi.CopyTo($"{outputPath}{count} {fi.Name}");
                            count++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"exception occured: {e.Message}. it is safe to ignore this.");
                    }
                }
            
            Console.WriteLine($"\nimported {count} songs, press any key to exit");
        }
        
        private static string GetOsuDirectory()
        {
            Console.WriteLine("osu! songs path (press enter for default):");
            var osuPath = Console.ReadLine();
            if (osuPath == "")
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                osuPath = appDataPath.Replace("Roaming", "Local/osu!/Songs");
            }
            
            return osuPath;
        }
        
        private static string GetOutputDirectory()
        {
            Console.WriteLine("your music library path (press enter for default):");
            var outputPath = Console.ReadLine();
            
            // create directory for export so we don't accidentally fuck up a directory & allow for easy undo
            // this assumes that they can create a path here etc.
            if (outputPath == "")
            {
                var music = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                var di = Directory.CreateDirectory(music + "osume-exported/");
                outputPath = di.FullName;
            }
            else
            {
                var di = Directory.CreateDirectory(outputPath + "osume-exported/");
                outputPath = di.FullName;
            }

            return outputPath;
        }
    }
}