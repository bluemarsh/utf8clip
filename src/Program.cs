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
                // StreamReader will use UTF-8 encoding unless there are byte order marks
                // (e.g. for a UTF-16 encoded file input)
                using (var r = new StreamReader(Console.OpenStandardInput()))
                    ReadToClipboard(r);
            }
            else if (Console.IsOutputRedirected)
            {
                // StreamWriter will use UTF-8 encoding without byte order mark by default.
                // When output is redirected we don't modify the console encoding to avoid issues
                // if the consuming program modifies the console encoding.
                using (var w = new StreamWriter(Console.OpenStandardOutput()))
                    WriteToClipboard(w);
            }
            else
            {
                // When output is not redirected, we need to set OutputEncoding so console
                // will display output correctly.
                using (new Utf8EncodingOverride())
                    WriteToClipboard(Console.Out);
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
            Console.WriteLine("    Copies the input, interpreted as UTF-8 text (*), to the Windows clipboard.");
            Console.WriteLine("Otherwise:");
            Console.WriteLine("    Prints the contents of the Windows clipboard to output as UTF-8 text.");
            Console.WriteLine();
            Console.WriteLine("(*) If there is a byte-order mark in the input it will be respected, e.g. for");
            Console.WriteLine("UTF-16 encoded files.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    <program> | utf8clip  Places a copy of the UTF-8 output from <program>");
            Console.WriteLine("                          on to the Windows clipboard. Note that most native");
            Console.WriteLine("                          Windows commands like dir do not write UTF-8 output");
            Console.WriteLine("                          unless the console codepage is changed with chcp.");
            Console.WriteLine();
            Console.WriteLine("    utf8clip < README.md  Places a copy of the text from README.md on to the");
            Console.WriteLine("                          Windows clipboard.");
            Console.WriteLine();
            Console.WriteLine("    utf8clip              Writes the current contents of the Windows clipboard");
            Console.WriteLine("                          to the console.");
            Console.WriteLine();
            Console.WriteLine("PowerShell Core Examples:");
            Console.WriteLine("    ls | utf8clip         Places a copy of the current directory listing");
            Console.WriteLine("                          on to the Windows clipboard. This works correctly as");
            Console.WriteLine("                          PowerShell Core uses UTF-8 output by default.");
            Console.WriteLine();
            Console.WriteLine("    cat .\\README.md | utf8clip");
            Console.WriteLine("                          Places a copy of the text from README.md on to the");
            Console.WriteLine("                          Windows clipboard.");
            Console.WriteLine();
            Console.WriteLine("    utf8clip              Writes the current contents of the Windows clipboard");
            Console.WriteLine("                          to the console.");
        }

        private static void ReadToClipboard(TextReader r)
        {
            using (var w = new StringWriter())
            {
                CopyContent(r, w);
                Clipboard.SetText(w.ToString());
            }
        }

        private static void WriteToClipboard(TextWriter w)
        {
            using (var r = new StringReader(Clipboard.GetText()))
            {
                CopyContent(r, w);
            }
        }

        private static void CopyContent(TextReader r, TextWriter w)
        {
            bool first = true;
            string? s;
            while ((s = r.ReadLine()) != null)
            {
                if (first)
                    first = false;
                else
                    w.WriteLine();

                w.Write(s);
            }
        }

        private sealed class Utf8EncodingOverride : IDisposable
        {
            private readonly Encoding? originalEncoding;

            public Utf8EncodingOverride()
            {
                if (!(Console.OutputEncoding is UTF8Encoding))
                {
                    this.originalEncoding = Console.OutputEncoding;
                    Console.OutputEncoding = new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier: false,
                        throwOnInvalidBytes: false);
                }
            }

            public void Dispose()
            {
                if (this.originalEncoding != null)
                    Console.OutputEncoding = this.originalEncoding;
            }
        }
    }
}
