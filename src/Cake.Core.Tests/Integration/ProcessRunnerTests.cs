using System;
using System.Diagnostics;
using System.Linq;
using Cake.Core.IO;
using Cake.Testing.Fakes;
using Xunit;
using Path = System.IO.Path;

namespace Cake.Core.Tests.Integration
{
    public class ProcessRunnerTests
    {
        // this is one of the ugly parts -- where should we actually expect the MockProcess.exe to go, and should it be customizable?
        private static readonly FilePath _appExe =
            new FilePath(Path.Combine(Environment.CurrentDirectory,
                @"..\..\..\Cake.Testing.MockProcess\bin\Cake.Testing.MockProcess.exe"));

        [Fact, Trait("Category", "integration")]
        public void Process_Should_Return_Correct_Exit_Code()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings().WithArguments(args => args.Append("--exitcode 3"))
                    .UseWorkingDirectory(Environment.CurrentDirectory);

            using (var process = runner.Start(_appExe, settings))
            {
                process.WaitForExit();
                var exitCode = process.GetExitCode();
                Console.WriteLine(string.Join("\r\n", log.Messages));
                Assert.Equal(3, exitCode);
            }
        }

        [Fact, Trait("Category", "integration")]
        public void Process_Should_Return_Correct_StandardOutput()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings().WithArguments(args => args.Append("--out line1 line2 line3"))
                    .UseWorkingDirectory(Environment.CurrentDirectory)
                    .SetRedirectStandardOutput(true);

            using (var process = runner.Start(_appExe, settings))
            {
                process.WaitForExit();
                var output = process.GetStandardOutput().ToArray();

                Assert.Equal(new[] { "line1", "line2", "line3" }, output);
            }
        }

        [Fact, Trait("Category", "integration")]
        public void Process_Should_Return_Correct_StandardError()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings().WithArguments(
                    args => args.Append("--err \"error line1\" \"error line2\" \"error line3\""))
                    .UseWorkingDirectory(Environment.CurrentDirectory)
                    .SetRedirectStandardError(true);

            using (var process = runner.Start(_appExe, settings))
            {
                process.WaitForExit();
                var output = process.GetStandardError().ToArray();

                Assert.Equal(new[] { "error line1", "error line2", "error line3" }, output);
            }
        }

        [Fact, Trait("Category", "integration")]
        public void Process_Can_Be_Killed()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings()
                    .UseWorkingDirectory(Environment.CurrentDirectory)
                    .WithArguments(args => args.Append("--sleep 5000"));

            using (var process = runner.Start(_appExe, settings))
            {
                process.Kill();
                Assert.True(process.HasExited);
            }
        }

        [Fact, Trait("Category", "integration")]
        public void Kill_Process_Returns_Minus1_ExitCode()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings()
                    .UseWorkingDirectory(Environment.CurrentDirectory)
                    .WithArguments(args => args.Append("--sleep 5000 --exitcode 3"));

            using (var process = runner.Start(_appExe, settings))
            {
                process.Kill();
                Assert.Equal(-1, process.GetExitCode());
            }
        }

        [Fact, Trait("Category", "integration")]
        public void Dispose_Does_Not_Kill_Underlying_Process_If_Still_Running()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings()
                    .UseWorkingDirectory(Environment.CurrentDirectory)
                    .WithArguments(args => args.Append("--sleep 5000"));

            int processId;
            using (var process = runner.Start(_appExe, settings))
            {
                processId = process.ProcessId;
                Assert.False(process.HasExited);
            }

            using (var p2 = Process.GetProcessById(processId))
            {
                Assert.False(p2.HasExited);
                p2.Kill();
            }
        }

        [Fact, Trait("Category", "integration")]
        public void Process_Should_Use_Provided_EnvironmentVariables()
        {
            var environment = FakeEnvironment.CreateWindowsEnvironment();
            var log = new FakeLog();
            var runner = new ProcessRunner(environment, log);

            var settings =
                new ProcessSettings()
                    .WithArguments(args => args.Append("--environmentVariables EnvVar1 EnvVar2"))
                    .UseWorkingDirectory(Environment.CurrentDirectory)
                    .WithEnvironmentVariable("EnvVar1", "Value1")
                    .WithEnvironmentVariable("EnvVar2", "Value2")
                    .SetRedirectStandardOutput(true);

            using (var process = runner.Start(_appExe, settings))
            {
                process.WaitForExit();
                var output = process.GetStandardOutput().ToArray();

                Assert.Equal(new[] { "EnvVar1: 'Value1'", "EnvVar2: 'Value2'" }, output);
            }
        }

        public class TheExitedEvent
        {

            [Fact, Trait("Category", "integration")]
            public void Process_Without_EventsEnabled_Will_Not_Raise_Exited_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetEnableRaisingEvents(false);

                bool exitedEventWasRaised = false;

                using (var process = runner.Start(_appExe, settings))
                {
                    process.Exited += (sender, args) =>
                    {
                        exitedEventWasRaised = true;
                    };

                    process.WaitForExit();

                    Assert.False(exitedEventWasRaised);
                }
            }

            [Fact, Trait("Category", "integration")]
            public void Process_With_EventsEnabled_Raises_Exited_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetEnableRaisingEvents(true);

                bool exitedEventWasRaised = false;

                using (var process = runner.Start(_appExe, settings))
                {
                    process.Exited += (sender, args) =>
                    {
                        exitedEventWasRaised = true;
                    };

                    process.WaitForExit();

                    Assert.True(exitedEventWasRaised);
                }
            }
        }

        public class TheErrorDataReceivedEvent
        {
            [Fact, Trait("Category", "integration")]
            public void Process_Without_EventsEnabled_Will_Not_Raise_ErrorDataReceived_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--err errorLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardError(true)
                        .SetEnableRaisingEvents(false);

                using (var process = runner.Start(_appExe, settings))
                {
                    Assert.Throws<InvalidOperationException>(() => process.ErrorDataReceived += (sender, args) => { });
                }
            }

            [Fact, Trait("Category", "integration")]
            public void Process_With_EventsEnabled_But_Error_Not_Redirected_Will_Throw()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--err errorLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardError(false)
                        .SetEnableRaisingEvents(true);

                using (var process = runner.Start(_appExe, settings))
                {
                    Assert.Throws<InvalidOperationException>(() => process.ErrorDataReceived += (sender, args) => {});
                }
            }

            [Fact, Trait("Category", "integration")]
            public void Process_With_EventsEnabled_Raises_ErrorDataReceived_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--err errorLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardError(true)
                        .SetEnableRaisingEvents(true);

                bool errorDataReceivedEventWasRaised = false;

                using (var process = runner.Start(_appExe, settings))
                {
                    process.ErrorDataReceived += (sender, args) =>
                    {
                        errorDataReceivedEventWasRaised = true;
                    };

                    process.WaitForExit();

                    Assert.True(errorDataReceivedEventWasRaised);
                }
            }
        }

        public class TheOutputDataReceivedEvent
        {

            [Fact, Trait("Category", "integration")]
            public void Process_Without_EventsEnabled_Will_Not_Raise_OutputDataReceived_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--out OutputLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardOutput(true)
                        .SetEnableRaisingEvents(false);

                using (var process = runner.Start(_appExe, settings))
                {
                    Assert.Throws<InvalidOperationException>(() => process.OutputDataReceived += (sender, args) => { });
                }
            }

            [Fact, Trait("Category", "integration")]
            public void Process_With_EventsEnabled_But_Output_Not_Redirected_Will_Not_Raise_OutputDataReceived_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--out OutputLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardOutput(false)
                        .SetEnableRaisingEvents(true);

                using (var process = runner.Start(_appExe, settings))
                {
                    Assert.Throws<InvalidOperationException>(() => process.OutputDataReceived += (sender, args) => { });
                }
            }

            [Fact, Trait("Category", "integration")]
            public void Process_With_EventsEnabled_Raises_OutputDataReceived_Event()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--out OutputLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardOutput(true)
                        .SetEnableRaisingEvents(true);

                bool outputDataReceivedEventWasRaised = false;

                using (var process = runner.Start(_appExe, settings))
                {
                    process.OutputDataReceived += (sender, args) =>
                    {
                        outputDataReceivedEventWasRaised = true;
                    };

                    process.WaitForExit();

                    Assert.True(outputDataReceivedEventWasRaised);
                }
            }

            [Fact, Trait("Category", "integration")]
            public void Process_With_EventsEnabled_Raises_OutputDataReceived_Event_Initially_With_Null_Data()
            {
                var environment = FakeEnvironment.CreateWindowsEnvironment();
                var log = new FakeLog();
                var runner = new ProcessRunner(environment, log);

                var settings =
                    new ProcessSettings()
                        .WithArguments(args => args.Append("--out OutputLine1"))
                        .UseWorkingDirectory(Environment.CurrentDirectory)
                        .SetRedirectStandardOutput(true)
                        .SetEnableRaisingEvents(true);

                string outputDataReceived = "empty";

                using (var process = runner.Start(_appExe, settings))
                {
                    process.OutputDataReceived += (sender, args) =>
                    {
                        outputDataReceived = args.Data;
                    };

                    process.WaitForExit();

                    Assert.Null(outputDataReceived);
                }
            }
        }
    }
}