using System;
using System.IO;
using System.Text;

namespace Tactics_Script_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length < 3)
            {
                Console.WriteLine("Tactics Script Tool");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Export text     : Script_Tool -e [read encoding] [file|folder]");
                Console.WriteLine("  Export all text : Script_Tool -a [read encoding] [file|folder]");
                Console.WriteLine("  Import text     : Script_Tool -b [read encoding] [write encoding] [file|folder]");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var mode = args[0];

            switch (mode)
            {
                case "-e":
                case "-a":
                {
                    bool exportAll = (mode == "-a");

                    void ExportText(string filePath)
                    {
                        Console.WriteLine($"Exporting strings from {Path.GetFileName(filePath)}");

                        try
                        {
                            var readEncoding = Encoding.GetEncoding(args[1]);
                            var script = new Script(readEncoding);
                            script.Load(filePath);
                            script.ExportText(Path.ChangeExtension(filePath, "txt"), exportAll);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    var path = Path.GetFullPath(args[2]);

                    if (Utility.PathIsFolder(path))
                    {
                        foreach (var item in Directory.EnumerateFiles(path, "*.bin"))
                        {
                            ExportText(item);
                        }
                    }
                    else
                    {
                        ExportText(path);
                    }

                    break;
                }
                case "-b":
                {
                    if (args.Length < 4)
                    {
                        Console.WriteLine("ERROR: 4 parameters are required.");
                        return;
                    }

                    void RebuildScript(string filePath)
                    {
                        Console.WriteLine($"Rebuilding script {Path.GetFileName(filePath)}");

                        try
                        {
                            var readEncoding = Encoding.GetEncoding(args[1]);
                            var writeEncoding = Encoding.GetEncoding(args[2]);
                            var textFilePath = Path.ChangeExtension(filePath, "txt");
                            var newFilePath = Path.GetDirectoryName(filePath) + @"\rebuild\" + Path.GetFileName(filePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                            var script = new Script(readEncoding);
                            script.Load(filePath);
                            script.ImportText(textFilePath, writeEncoding);
                            script.Save(newFilePath);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    var path = Path.GetFullPath(args[3]);

                    if (Utility.PathIsFolder(path))
                    {
                        foreach (var item in Directory.EnumerateFiles(path, "*.bin"))
                        {
                            RebuildScript(item);
                        }
                    }
                    else
                    {
                        RebuildScript(path);
                    }

                    break;
                }
            }
        }
    }
}
