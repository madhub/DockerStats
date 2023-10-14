using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerStats;
/// <summary>
/// Helper class to log message to console or file
/// </summary>
public static class LogUtils
{
    public enum LogOutType
    {
        Console,
        File
    };
    private static LogOutType logOutType = LogOutType.Console;
    private static String file = string.Empty;
    public static void SetLogOut(LogOutType logoutputType,DockerStatsOptions options )
    {
        logOutType = logoutputType;
        file = options.output;
    }
    public static void Log(string message)
    {
        if ( logOutType== LogOutType.Console )
        {
            Console.WriteLine(message);
        }else
        {
            File.AppendAllText(file, $"{message}{System.Environment.NewLine}");
        }
    }
}
