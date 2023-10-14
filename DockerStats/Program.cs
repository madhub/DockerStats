
using CommandLine;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerStats;
using System.Runtime.InteropServices;

var parsedArgs = Parser.Default.ParseArguments<DockerStatsOptions>(args);
parsedArgs.WithNotParsed(list =>
{
    Environment.Exit(-1);
});


if (!string.IsNullOrEmpty(parsedArgs.Value.output))
{
    Console.WriteLine($"Writing starts to file {parsedArgs.Value.output}"); 
    LogUtils.SetLogOut(LogUtils.LogOutType.File, parsedArgs.Value);
}

DockerClient? client = default;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    client = new DockerClientConfiguration(
        new Uri("npipe://./pipe/docker_engine"))
         .CreateClient();
}
else
{
    // Default Docker Engine on Linux
    client = new DockerClientConfiguration(
        new Uri("unix:///var/run/docker.sock"))
         .CreateClient();
}

// declare variable for listing containers & Getting Docker Starts
var containerListParameters = new ContainersListParameters()
{
    All = true,
    // filter for running & created containers
    Filters = new Dictionary<string, IDictionary<string, bool>>
    {
        ["status"] = new Dictionary<string, bool>
        {
            ["running"] = true,
            ["created"] = true
        }
    },

};

var containerStartsParams = new ContainerStatsParameters() { OneShot = true, Stream = false };

// callback function to logs the receives stats from Docker Daemon
var statsCallback = new Progress<ContainerStatsResponse>(m =>
{

    var dockerStats = Helper.ParseStats(m);
    LogUtils.Log($"{m.Name},{dockerStats.CpuUsageInPercent:F2},{(long)dockerStats.MemoryUsageInKB},{(long)dockerStats.MemoryLimitInKB},{dockerStats.MemoryUsageInPercent:F2},{dockerStats.TimeStamp}");


});

// Write header
LogUtils.Log($"ContainerName,CpuPercent,MemUsageInKB,MaxAviMemInKB,MemUsageInPercent,DateTime");
while (true)
{
    var containers = await client.Containers.ListContainersAsync(containerListParameters);
    foreach (var container in containers)
    {
        try
        {
            await client.Containers.GetContainerStatsAsync(container.ID, containerStartsParams, statsCallback);
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Exception: {exp}");
            Environment.Exit(-1);
        }


    }
    await Task.Delay((int)TimeSpan.FromSeconds((int)parsedArgs.Value.pollfrequence).TotalMilliseconds);
}
