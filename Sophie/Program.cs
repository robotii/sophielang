using System;
using System.IO;
using Sophie.Core.VM;

namespace Sophie
{
    static class Program
    {
        private static string _loadedFile;

        static int Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    RunRepl();
                    break;
                case 1:
                    int r = RunFile(args[0]);
                    return r;
                default:
                    Console.WriteLine("Usage: sophie [file]\n");
                    return 1; // EX_USAGE.
            }
            return 0;
        }

        static int RunFile(string path)
        {
            if (File.Exists(path))
            {
                _loadedFile = path;
                string source = File.ReadAllText(path);
                SophieVM vm = new SophieVM { LoadModuleFn = LoadModule };
                return (int)vm.Interpret(path, source);
            }
            return 66; // File Not Found
        }

        static void RunRepl()
        {
            SophieVM vm = new SophieVM();

            Console.WriteLine("-- sophie v0.0.0");

            string line = "";

            for (; line != "/exit"; )
            {
                Console.Write("> ");
                line = Console.ReadLine();

                // TODO: Handle failure.
                vm.Interpret("Prompt", line);
            }
        }

        static string LoadModule(string name)
        {
            int lastPathSeparator = _loadedFile.LastIndexOf("\\", StringComparison.Ordinal);
            if (lastPathSeparator < 0)
                lastPathSeparator = _loadedFile.LastIndexOf("/", StringComparison.Ordinal);
            string rootDir = _loadedFile.Substring(0, lastPathSeparator + 1);
            string path = rootDir + name + ".sophie";
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            if (Directory.Exists(path.Substring(0, path.Length - 7)))
            {
                path = path.Substring(0, path.Length - 7) + "\\" + "module.sophie";
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            return null;
        }
    }
}
