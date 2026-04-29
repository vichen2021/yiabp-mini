using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Yi.Framework.Core.Helper
{
    public class ShellHelper
    {
        /// <summary>
        /// linux 系统命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string Bash(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();
            return result;
        }

        /// <summary>
        /// windows系统命令
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Cmd(string fileName, string args)
        {
            string output = string.Empty;

            var info = new ProcessStartInfo();
            info.FileName = fileName;
            info.Arguments = args;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            using (var process = Process.Start(info))
            {
                if (process != null)
                {
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }
            }
            return output;
        }

        /// <summary>
        /// 执行 PowerShell 命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string PowerShell(string command)
        {
            string output = string.Empty;
            try
            {
                var info = new ProcessStartInfo();
                info.FileName = "powershell";
                info.Arguments = $"-NoProfile -NonInteractive -Command \"{command}\"";
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.StandardOutputEncoding = Encoding.UTF8;
                info.StandardErrorEncoding = Encoding.UTF8;

                using (var process = Process.Start(info))
                {
                    if (process != null)
                    {
                        output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行 PowerShell 命令出错: {ex.Message}");
            }
            return output;
        }
    }
}
