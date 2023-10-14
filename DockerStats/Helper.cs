using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerStats;


/// <summary>
/// Record class to capture the Parsed Docker stats
/// </summary>
/// <param name="CpuUsageInPercent"></param>
/// <param name="MemoryUsageInKB"></param>
/// <param name="MemoryLimitInKB"></param>
/// <param name="MemoryUsageInPercent"></param>
/// <param name="TimeStamp"></param>
public record struct DockerStats(double CpuUsageInPercent,double MemoryUsageInKB, double MemoryLimitInKB,float MemoryUsageInPercent,String TimeStamp);
/// <summary>
/// Helper class 
/// </summary>
public class Helper
{
    /// <summary>
    /// Helper function to Parse the Docker raw stats
    /// </summary>
    /// <param name="containerStatsResponse"></param>
    /// <returns></returns>
    public static DockerStats ParseStats(ContainerStatsResponse containerStatsResponse)
    {
        // CPU & Memory Usage calcuation is based on the go Docker Client
        // https://github.com/docker/cli/blob/fc247d6911944b3c8dca524c93f291e5f79ec3da/cli/command/container/stats_helpers.go#L166
        var previousCPU = containerStatsResponse.PreCPUStats.CPUUsage.TotalUsage;
        var previousSystem = containerStatsResponse.PreCPUStats.SystemUsage;
        double cpuPercent = 0.0;
        float cpuDelta = (float)containerStatsResponse.CPUStats.CPUUsage.TotalUsage - (float)previousCPU;
        float systemDelta = (float)(containerStatsResponse.CPUStats.SystemUsage) - (float)previousSystem;
        float onlineCPUs = (float)(containerStatsResponse.CPUStats.OnlineCPUs);
        if (onlineCPUs == 0.0)
        {
            onlineCPUs = (float)containerStatsResponse.CPUStats.CPUUsage.PercpuUsage.Count();
        }


        if (systemDelta > 0.0 && cpuDelta > 0.0)
        {
            cpuPercent = (cpuDelta / systemDelta) * onlineCPUs * 100.0;
        }
        float memUsageInPercent = ((float)containerStatsResponse.MemoryStats.Usage / (float)containerStatsResponse.MemoryStats.Limit) * (float)100.0;
        var stats = new DockerStats(cpuPercent,
                                    Bytes.ToKilobytes((long)containerStatsResponse.MemoryStats.Usage),
                                    Bytes.ToKilobytes((long)containerStatsResponse.MemoryStats.Limit),
                                    memUsageInPercent,
                                    DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
        return stats;
    }

}
