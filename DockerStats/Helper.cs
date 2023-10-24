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
/// <param name="TotalNetworkBytesTransferedInKB"></param>
/// <param name="TotalNetworkBytesReceivedInKB"></param>
/// <param name="TimeStamp">In ISO Date Time Format yyyy-MM-dd'T'HH:mm:ss.SSSXXX — for example, "2000-10-31T01:30:00.000-05:00".</param>
public record struct DockerStats(double CpuUsageInPercent,
                                double MemoryUsageInKB, 
                                double MemoryLimitInKB,
                                float MemoryUsageInPercent,
                                double TotalNetworkBytesTransferedInKB,
                                double TotalNetworkBytesReceivedInKB,
                                String TimeStamp);
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
    public static DockerStats ParseStats(ContainerStatsResponse containerStatsResponse,bool UseUtcTimestamp=true)
    {
        // CPU & Memory Usage calcuation is based on the go Docker Client
        // https://github.com/docker/cli/blob/fc247d6911944b3c8dca524c93f291e5f79ec3da/cli/command/container/stats_helpers.go#L166
        // https://docs.docker.com/engine/api/v1.43/#tag/Container/operation/ContainerStats

        double cpuPercent = calculateCpuPercentage(containerStatsResponse);

        double memUsage = calculateMemUsageUnixNoCache(containerStatsResponse);
        double memLimit = containerStatsResponse.MemoryStats.Limit;


        double memUsageInPercent = (memUsage / memLimit) * (double)100.0;

        (double txInBytes, double rxinBytes) NetworkTransfer = calculateNetworkTransfer(containerStatsResponse);

        String timeStamp = String.Empty;
        //  using universal sortable ("u") format specifier
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#UniversalSortable
        if (UseUtcTimestamp)
        {
            timeStamp = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture);
        }else
        {
            timeStamp = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);
        }
        
        var stats = new DockerStats(cpuPercent,
                                    (long)Bytes.ToKilobytes(memUsage),
                                    (long)Bytes.ToKilobytes(memLimit),
                                    (float)memUsageInPercent,
                                    Bytes.ToKilobytes(NetworkTransfer.txInBytes),
                                    Bytes.ToKilobytes(NetworkTransfer.rxinBytes),
                                    timeStamp);


        

        return stats;
    }
    // https://github.com/docker/cli/blob/fc247d6911944b3c8dca524c93f291e5f79ec3da/cli/command/container/stats_helpers.go#L239
    private static double calculateMemUsageUnixNoCache(ContainerStatsResponse containerStatsResponse)
    {
        float totalMemoryUsage = 0;
        // cgroup v1
        // if total_inactive_file exists in the map & it is less MemoryStats.Usage, than it is cgroup1 
        if (containerStatsResponse.MemoryStats.Stats.TryGetValue("total_inactive_file", out var total_inactive_file))
        {
            if (total_inactive_file < containerStatsResponse.MemoryStats.Usage)
            {
                totalMemoryUsage = (containerStatsResponse.MemoryStats.Usage - total_inactive_file);
                return totalMemoryUsage;
            }
        }
        
        // cgroup v2
        if ( containerStatsResponse.MemoryStats.Stats.TryGetValue("inactive_file", out var inactive_file))
        {
            if (inactive_file < containerStatsResponse.MemoryStats.Usage)
            {
                totalMemoryUsage = (containerStatsResponse.MemoryStats.Usage - inactive_file);
                return totalMemoryUsage;
            }
        }

        return containerStatsResponse.MemoryStats.Usage;
    }

    private static double calculateCpuPercentage(ContainerStatsResponse containerStatsResponse)
    {
        double cpuPercent = 0.0;
        var previousCPU = containerStatsResponse.PreCPUStats.CPUUsage.TotalUsage;
        var previousSystem = containerStatsResponse.PreCPUStats.SystemUsage;

        double cpuDelta = (float)containerStatsResponse.CPUStats.CPUUsage.TotalUsage - (float)previousCPU;
        double systemDelta = (float)(containerStatsResponse.CPUStats.SystemUsage) - (float)previousSystem;
        uint onlineCPUs = containerStatsResponse.CPUStats.OnlineCPUs;
        if (onlineCPUs == 0)
        {
            onlineCPUs = containerStatsResponse.CPUStats.CPUUsage.PercpuUsage != null  ? 
                    (uint)containerStatsResponse.CPUStats.CPUUsage.PercpuUsage?.Count() : 0;
        }


        if (systemDelta > 0.0 && cpuDelta > 0.0)
        {
            cpuPercent = (cpuDelta / systemDelta) * (double)onlineCPUs * 100.0;
        }
        return cpuPercent;
    }
    private static (double txInBytes,double rxinBytes) calculateNetworkTransfer(ContainerStatsResponse containerStatsResponse)
    {
        double txInBytes = 0;
        double rxinBytes = 0;
        foreach (var kvp in containerStatsResponse.Networks)
        {
            txInBytes += kvp.Value.TxBytes;
            rxinBytes += kvp.Value.RxBytes;
        }
        return (txInBytes, rxinBytes);  
    }

}
