using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace BlueMarsh.Utf8Clip
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                PrintHelp();
            }
            else if (Console.IsInputRedirected)
            {
                ReadInToClipboard();
            }
            else
            {
                WriteClipboardToOut();
            }
        }
        
        private static void PrintHelp()
        {
            var ver = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            Console.WriteLine($"utf8clip {ver}");
            Console.WriteLine();
            Console.WriteLine("If started with file/piped input:");
            Console.WriteLine("    Copies the input, interpreted as UTF-8 text, to the Windows clipboard.");
            Console.WriteLine("Otherwise:");
            Console.WriteLine("    Writes the contents of the Windows clipboard to output as UTF-8 text.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    dir | utf8clip        Places a copy of the current directory");
            Console.WriteLine("                          listing on to the Windows clipboard.");
            Console.WriteLine();
            Console.WriteLine("    utf8clip < README.md  Places a copy of the text from README.md");
            Console.WriteLine("                          on to the Windows clipboard.");
            Console.WriteLine();
            Console.WriteLine("    utf8clip              Writes the current contents of the");
            Console.WriteLine("                          Windows clipboard to the console.");
        }

        private static void ReadInToClipboard()
        {
            var originalEncoding = Console.InputEncoding;
            Console.InputEncoding = Encoding.UTF8;
            try
            {
                Clipboard.SetText(Console.In.ReadToEnd());
            }
            finally
            {
                Console.InputEncoding = originalEncoding;
            }
        }

        private static void WriteClipboardToOut()
        {
            var originalEncoding = Console.OutputEncoding;
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                using (var r = new StringReader(Clipboard.GetText()))
                {
                    bool first = true;
                    string? s;
                    while ((s = r.ReadLine()) != null)
                    {
                        if (first)
                            first = false;
                        else
                            Console.WriteLine();

                        Console.Write(s);
                    }
                }
            }
            finally
            {
                Console.OutputEncoding = originalEncoding;
            }
        }
    }
}
