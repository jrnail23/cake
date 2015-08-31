using System;
using System.Threading;
using CommandLine;
using CommandLine.Text;

namespace Cake.Testing.MockProcess
{
    /// <summary>
    ///     This console app acts as a mock for integration testing Cake's Process features.
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     Entry point for the console app.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static int Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine(options.GetUsage());
                return 0;
            }

            Thread.Sleep(options.Delay);

            if (options.LoadedMessage != null)
            {
                Console.Out.WriteLine(options.LoadedMessage);
            }

            foreach (var key in options.EnvironmentVariables)
            {
                Console.Out.WriteLine("{0}: '{1}'", key, Environment.GetEnvironmentVariable(key));
            }

            foreach (var outContent in options.StandardOutputToWrite)
            {
                Console.Out.WriteLine(outContent);
            }

            foreach (var errContent in options.StandardErrorToWrite)
            {
                Console.Error.WriteLine(errContent);
            }

            Thread.Sleep(options.Sleep);

            if (options.Pause)
            {
                Console.ReadKey();
            }

            return options.ExitCode;
        }
    }

    public class Options
    {
        [Option('s', "sleep", DefaultValue = 0, HelpText = "Number of milliseconds to sleep for before exiting.")]
        public int Sleep { get; set; }

        [Option('l', "loadedmsg", DefaultValue = null, HelpText = "Message to write to stdout when processing begins.")]
        public string LoadedMessage { get; set; }

        [Option('e', "exitcode", DefaultValue = 0, HelpText = "Exit code to return upon program completion.")]
        public int ExitCode { get; set; }

        [Option('p', "pause", DefaultValue = false, HelpText = "Pause to wait for user input before exiting.")]
        public bool Pause { get; set; }

        [OptionArray('v', "environmentVariables", DefaultValue = new string[0],
            HelpText = "Keys of environment variables to be written to stdout.")]
        public string[] EnvironmentVariables { get; set; }

        [Option('d', "delay", DefaultValue = 0, HelpText = "Number of milliseconds to delay before executing (simulates startup time).")]
        public int Delay { get; set; }

        [OptionArray("out", DefaultValue = new string[0], HelpText = "Lines of content to write to stdout.")]
        public string[] StandardOutputToWrite { get; set; }

        [OptionArray("err", DefaultValue = new string[0], HelpText = "Lines of content to write to stderr.")]
        public string[] StandardErrorToWrite { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                AddDashesToOption = true,
                AdditionalNewLineAfterOption = true,
                Heading = new HeadingInfo(GetType().Assembly.GetName().Name)
            };

            help.AddOptions(this);
            return help;
        }
    }
}