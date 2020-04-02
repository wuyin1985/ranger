using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace testwsl
{
    internal class Program
    {
        private static readonly Regex s_regex =
            new Regex(@"/mnt/([a-z])/", RegexOptions.Singleline);

        public static string WordScrambler(Match match)
        {
            var first = match.Groups[1];
            return $"{first}:/";
        }

        private struct Config
        {
            public bool isDir;
            public bool isToClipBoard;
        }

        private static void paste_win_path(ref string[] args, Config config)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (s_regex.IsMatch(s))
                {
                    s = s_regex.Replace(s, WordScrambler, 1);
                    if (!File.Exists(s))
                    {
                        Console.Error.WriteLine($"file {s} not exist");
                        return;
                    }

                    args[i] = s;
                }
            }
        }

        private static Config get_config(ref string[] args)
        {
            var c = new Config();
            var remains = new List<string>();
            foreach (var s in args)
            {
                switch (s)
                {
                    case "-dir":
                    {
                        c.isDir = true;
                        break;
                    }
                    case "-clip":
                    {
                        c.isToClipBoard = true;
                        break;
                    }

                    default:
                    {
                        remains.Add(s);
                        break;
                    }
                }
            }

            args = remains.ToArray();
            return c;
        }

        public static void SetClipboard(string value)
        {
            Clipboard.SetText(value);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var config = get_config(ref args);
            paste_win_path(ref args, config);

            StringBuilder sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(arg).Append(' ');
            }

            if (config.isToClipBoard)
            {
                Console.WriteLine($"set [{sb}] to clipboard");
                var path = Directory.GetCurrentDirectory();
                SetClipboard($@"{path}\{sb}");
                return;
            }

            Console.WriteLine($"input args is {sb} [END]");
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + sb)
            {
                //WorkingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName,
                UseShellExecute = false,
                //RedirectStandardOutput = true,
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();
        }
    }
}