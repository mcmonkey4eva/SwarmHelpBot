using System;
using System.Threading;
using FreneticUtilities.FreneticToolkit;

namespace SwarmHelpBot;

public static class Program
{
    public static void Main(string[] args)
    {
        SpecialTools.Internationalize();
        Run(args);
    }

    public static void Run(string[] args)
    {
        try
        {
            new SwarmBot().InitAndRun(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Discord crash: " + ex.ToString());
            Thread.Sleep(10 * 1000);
            Run([]);
        }
    }
}
