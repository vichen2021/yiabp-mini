using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Yi.Framework.Core.Helper
{
    public class ComputerHelper
    {

        /// <summary>
        /// 将object转换为long，若转换失败，则返回0。不抛出异常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static long ParseToLong( object obj)
        {
            try
            {
                return long.Parse(obj.ToString());
            }
            catch
            {
                return 0L;
            }
        }

        /// <summary>
        /// 将string转换为DateTime，若转换失败，则返回日期最小值。不抛出异常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static DateTime ParseToDateTime( string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return DateTime.MinValue;
                }
                if (str.Contains("-") || str.Contains("/"))
                {
                    return DateTime.Parse(str);
                }
                else
                {
                    int length = str.Length;
                    switch (length)
                    {
                        case 4:
                            return DateTime.ParseExact(str, "yyyy", System.Globalization.CultureInfo.CurrentCulture);
                        case 6:
                            return DateTime.ParseExact(str, "yyyyMM", System.Globalization.CultureInfo.CurrentCulture);
                        case 8:
                            return DateTime.ParseExact(str, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        case 10:
                            return DateTime.ParseExact(str, "yyyyMMddHH", System.Globalization.CultureInfo.CurrentCulture);
                        case 12:
                            return DateTime.ParseExact(str, "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture);
                        case 14:
                            return DateTime.ParseExact(str, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        default:
                            return DateTime.ParseExact(str, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                    }
                }
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        private static double ParseToDouble(object obj)
        {
            try
            {
                return double.Parse(obj.ToString());
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// CPU使用情况
        /// </summary>
        /// <returns></returns>
        public static CPUMetrics GetCPUMetrics()
        {
            CPUMetrics cpuMetrics = new CPUMetrics();
           var cpudetail =  GetCPUDetails();
            cpuMetrics.CoreTotal = cpudetail.Cores;
            cpuMetrics.LogicalProcessors =cpudetail.LogicalProcessors;
            cpuMetrics.CPURate = Math.Ceiling(ParseToDouble(GetCPURate()));
            cpuMetrics.FreeRate = 1 - cpuMetrics.CPURate;
            return cpuMetrics;
        }
        /// <summary>
        /// 内存使用情况
        /// </summary>
        /// <returns></returns>
        public static MemoryMetrics GetMemoryMetrics()
        {
            try
            {
                MemoryMetricsClient client = new();
                MemoryMetrics memoryMetrics = IsUnix() ? client.GetUnixMetrics() : client.GetWindowsMetrics();

                memoryMetrics.FreeRam = Math.Round(memoryMetrics.Free / 1024, 2) + "GB";
                memoryMetrics.UsedRam = Math.Round(memoryMetrics.Used / 1024, 2) + "GB";
                memoryMetrics.TotalRAM = Math.Round(memoryMetrics.Total / 1024, 2) + "GB";
                memoryMetrics.RAMRate = Math.Ceiling(100 * memoryMetrics.Used / memoryMetrics.Total).ToString() + "%";
  
                return memoryMetrics;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取内存使用出错，msg=" + ex.Message + "," + ex.StackTrace);
            }
            return new MemoryMetrics();
        }

        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        /// <returns></returns>
        public static List<DiskInfo> GetDiskInfos()
        {
            List<DiskInfo> diskInfos = new();

            if (IsUnix())
            {
                try
                {
                    string output = ShellHelper.Bash("df -m / | awk '{print $2,$3,$4,$5,$6}'");
                    var arr = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 0) return diskInfos;

                    var rootDisk = arr[1].Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
                    if (rootDisk == null || rootDisk.Length == 0)
                    {
                        return diskInfos;
                    }
                    DiskInfo diskInfo = new()
                    {
                        DiskName = "/",
                        TotalSize = long.Parse(rootDisk[0]) / 1024,
                        Used = long.Parse(rootDisk[1]) / 1024,
                        AvailableFreeSpace = long.Parse(rootDisk[2]) / 1024,
                        AvailablePercent = decimal.Parse(rootDisk[3].Replace("%", ""))
                    };
                    diskInfos.Add(diskInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取磁盘信息出错了" + ex.Message);
                }
            }
            else
            {
                var driv = DriveInfo.GetDrives();
                foreach (var item in driv)
                {
                    try
                    {
                        var obj = new DiskInfo()
                        {
                            DiskName = item.Name,
                            TypeName = item.DriveType.ToString(),
                            TotalSize = item.TotalSize / 1024 / 1024 / 1024,
                            AvailableFreeSpace = item.AvailableFreeSpace / 1024 / 1024 / 1024,
                        };
                        obj.Used = obj.TotalSize - obj.AvailableFreeSpace;
                        obj.AvailablePercent = decimal.Ceiling(obj.Used / (decimal)obj.TotalSize * 100);
                        diskInfos.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("获取磁盘信息出错了" + ex.Message);
                        continue;
                    }
                }
            }

            return diskInfos;
        }

        public static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            return isUnix;
        }
        
        public static string GetCPURate()
        {
            string cpuRate;
            if (IsUnix())
            {
                string output = ShellHelper.Bash("top -b -n1 | grep \"Cpu(s)\" | awk '{print $2 + $4}'");
                cpuRate = output.Trim();
            }
            else
            {
                // 使用 PowerShell 获取 CPU 使用率
                string output = ShellHelper.PowerShell("Get-CimInstance Win32_Processor | Measure-Object -property LoadPercentage -Average | Select-Object -ExpandProperty Average");
                cpuRate = output.Trim();
            }
            return cpuRate;
        }

        /// <summary>
        /// 获取系统运行时间
        /// </summary>
        /// <returns></returns>
        public static string GetRunTime()
        {
            string runTime = string.Empty;
            try
            {
                if (IsUnix())
                {
                    string output = ShellHelper.Bash("uptime -s").Trim();
                    runTime = DateTimeHelper.FormatTime(ParseToLong((DateTime.Now - ParseToDateTime(output)).TotalMilliseconds.ToString().Split('.')[0]));
                }
                else
                {
                    // 使用 PowerShell 获取系统启动时间
                    // 方法1: 尝试获取格式化的日期时间
                    string output = ShellHelper.PowerShell("$bootTime = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime; $bootTime.ToString('yyyyMMddHHmmss')");
                    if (string.IsNullOrWhiteSpace(output))
                    {
                        // 方法2: 如果方法1失败，尝试直接获取并格式化
                        output = ShellHelper.PowerShell("(Get-CimInstance Win32_OperatingSystem).LastBootUpTime | ForEach-Object { $_.ToString('yyyyMMddHHmmss') }");
                    }
                    
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        DateTime bootTime = ParseToDateTime(output.Trim());
                        if (bootTime != DateTime.MinValue)
                        {
                            runTime = DateTimeHelper.FormatTime(ParseToLong((DateTime.Now - bootTime).TotalMilliseconds.ToString().Split('.')[0]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取runTime出错" + ex.Message);
            }
            return runTime;
        }
        


        public static CPUInfo GetCPUDetails()
        {
            int logicalProcessors = 0;
            int cores = 0;

            if (IsUnix())
            {
                string logicalOutput = ShellHelper.Bash("lscpu | grep '^CPU(s):' | awk '{print $2}'");
                logicalProcessors = int.Parse(logicalOutput.Trim());

                string coresOutput = ShellHelper.Bash("lscpu | grep 'Core(s) per socket:' | awk '{print $4}'");
                string socketsOutput = ShellHelper.Bash("lscpu | grep 'Socket(s):' | awk '{print $2}'");
                cores = int.Parse(coresOutput.Trim()) * int.Parse(socketsOutput.Trim());
            }
            else
            {
                // 使用 PowerShell 获取 CPU 信息
                try
                {
                    string coresOutput = ShellHelper.PowerShell("(Get-CimInstance Win32_Processor).NumberOfCores | Measure-Object -Sum | Select-Object -ExpandProperty Sum");
                    string logicalOutput = ShellHelper.PowerShell("(Get-CimInstance Win32_Processor).NumberOfLogicalProcessors | Measure-Object -Sum | Select-Object -ExpandProperty Sum");
                    
                    if (!string.IsNullOrWhiteSpace(coresOutput))
                    {
                        cores = int.Parse(coresOutput.Trim());
                    }
                    if (!string.IsNullOrWhiteSpace(logicalOutput))
                    {
                        logicalProcessors = int.Parse(logicalOutput.Trim());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取CPU详细信息出错: " + ex.Message);
                }
            }

            return new CPUInfo
            {
                LogicalProcessors = logicalProcessors,
                Cores = cores
            };
        }
    }
    public class CPUInfo
    {
        public int LogicalProcessors { get; set; }
        public int Cores { get; set; }
    }
    public class CPUMetrics
    {
        /// <summary>
        /// 内核数
        /// </summary>
        public int CoreTotal { get; set; }
        /// <summary>
        /// 逻辑处理器数
        /// </summary>
        public int LogicalProcessors { get; set; }
        /// <summary>
        /// CPU使用率%
        /// </summary>
        public double CPURate { get; set; }
        /// <summary>
        /// CPU空闲率%
        /// </summary>
        public double FreeRate { get; set; }
    }
    
    /// <summary>
    /// 内存信息
    /// </summary>
    public class MemoryMetrics
    {
        [JsonIgnore]
        public double Total { get; set; }
        [JsonIgnore]
        public double Used { get; set; }
        [JsonIgnore]
        public double Free { get; set; }

        public string UsedRam { get; set; }
   
        /// <summary>
        /// 总内存 GB
        /// </summary>
        public string TotalRAM { get; set; }
        /// <summary>
        /// 内存使用率 %
        /// </summary>
        public string RAMRate { get; set; }
        /// <summary>
        /// 空闲内存
        /// </summary>
        public string FreeRam { get; set; }
    }

    public class DiskInfo
    {
        /// <summary>
        /// 磁盘名
        /// </summary>
        public string DiskName { get; set; }
        public string TypeName { get; set; }
        public long TotalFree { get; set; }
        public long TotalSize { get; set; }
        /// <summary>
        /// 已使用
        /// </summary>
        public long Used { get; set; }
        /// <summary>
        /// 可使用
        /// </summary>
        public long AvailableFreeSpace { get; set; }
        public decimal AvailablePercent { get; set; }
    }

    public class MemoryMetricsClient
    {
        #region 获取内存信息

        /// <summary>
        /// windows系统获取内存信息
        /// </summary>
        /// <returns></returns>
        public MemoryMetrics GetWindowsMetrics()
        {
            var metrics = new MemoryMetrics();
            try
            {
                // 使用 PowerShell 获取内存信息
                string totalOutput = ShellHelper.PowerShell("(Get-CimInstance Win32_OperatingSystem).TotalVisibleMemorySize");
                string freeOutput = ShellHelper.PowerShell("(Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory");

                if (!string.IsNullOrWhiteSpace(totalOutput) && !string.IsNullOrWhiteSpace(freeOutput))
                {
                    double total = double.Parse(totalOutput.Trim());
                    double free = double.Parse(freeOutput.Trim());
                    
                    metrics.Total = Math.Round(total / 1024, 0);
                    metrics.Free = Math.Round(free / 1024, 0); // MB
                    metrics.Used = metrics.Total - metrics.Free;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取Windows内存信息出错: " + ex.Message);
            }

            return metrics;
        }

        /// <summary>
        /// Unix系统获取
        /// </summary>
        /// <returns></returns>
        public MemoryMetrics GetUnixMetrics()
        {
            string output = ShellHelper.Bash(@"
# 从 /proc/meminfo 文件中提取总内存
 total_mem=$(cat /proc/meminfo | grep -i ""MemTotal"" | awk '{print $2}')
 # 从 /proc/meminfo 文件中提取剩余内存
free_mem=$(cat /proc/meminfo | grep -i ""MemFree"" | awk '{print $2}')
# 显示提取的信息
echo $total_mem $used_mem $free_mem
 ");
            var metrics = new MemoryMetrics();
 
            if (!string.IsNullOrWhiteSpace(output))
            {
                var memory = output.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
                if (memory.Length >= 2)
                {
                    metrics.Total =  Math.Round(double.Parse(memory[0]) / 1024, 0);
               
                    metrics.Free = Math.Round(double.Parse(memory[1])/ 1024, 0);//m
                    metrics.Used =   metrics.Total   - metrics.Free;
                }
            }
            return metrics;
        }
        #endregion
    }
}