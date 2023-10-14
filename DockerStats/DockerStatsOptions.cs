using CommandLine;

namespace DockerStats;
public class DockerStatsOptions
{
    [Option('t', "pollfrequence", Default=10,Required = false, HelpText = "Time in seconds to poll for stats")]
    public int pollfrequence { get; set; }

    [Option('o', "output", Required = false, HelpText = "output to file , default is console")]
    public string ? output { get; set; }
}
