using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cake.Core.Diagnostics;

namespace Cake.Core.IO
{
    internal sealed class ProcessWrapper : IProcess
    {
        private readonly Process _process;
        private readonly ICakeLog _log;
        private readonly Func<string, string> _filterOutput;

        public ProcessWrapper(Process process, ICakeLog log, Func<string, string> filterOutput)
        {
            _process = process;
            _log = log;
            _filterOutput = filterOutput ?? (source => "[REDACTED]");
        }

        public bool Start()
        {
            return _process.Start();
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        public bool WaitForExit(int milliseconds)
        {
            if (_process.WaitForExit(milliseconds))
            {
                return true;
            }
            _process.Refresh();
            if (!_process.HasExited)
            {
                _process.Kill();
            }
            return false;
        }

        public int GetExitCode()
        {
            return _process.ExitCode;
        }

        public IEnumerable<string> GetStandardOutput()
        {
            string line;
            while ((line = _process.StandardOutput.ReadLine()) != null)
            {
                _log.Debug("{0}", _filterOutput(line));
                yield return line;
            }
        }

        public IEnumerable<string> GetStandardError()
        {
            string line;
            while ((line = _process.StandardError.ReadLine()) != null)
            {
                _log.Warning("{0}", _filterOutput(line));
                yield return line;
            }
        }

        public int ProcessId
        {
            get
            {
                return _process.Id;
            }
        }

        public void Kill()
        {
            if (!HasExited)
            {
                _process.Kill();
                _process.WaitForExit();
            }
        }

        public bool HasExited
        {
            get
            {
                _process.Refresh();
                return _process.HasExited;
            }
        }

        event EventHandler IProcess.Exited
        {
            add
            {
                if (_process.EnableRaisingEvents)
                {
                    _process.Exited += value;
                }
            }
            remove
            {
                if (_process.EnableRaisingEvents)
                {
                    _process.Exited -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when an application writes to its redirected StandardError stream.
        /// </summary>
        /// <remarks>
        /// The ErrorDataReceived event indicates that the associated process has written to its redirected StandardError stream.
        /// The event only occurs during asynchronous read operations on StandardError. To start asynchronous read operations, 
        /// you must redirect the StandardError stream of a Process, add your event handler to the ErrorDataReceived event, 
        /// and call BeginErrorReadLine. Thereafter, the ErrorDataReceived event signals each time the process writes a 
        /// line to the redirected StandardError stream, until the process exits or calls CancelErrorRead.
        /// <note>The application that is processing the asynchronous output should call the WaitForExit method to ensure that the output buffer has been flushed.</note>
        /// </remarks>
        event DataReceivedEventHandler IProcess.ErrorDataReceived
        {
            add
            {
                if (!_process.EnableRaisingEvents || !_process.StartInfo.RedirectStandardError)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "ErrorDataReceived event requires Process.EnableRaisingEvents and Process.StartInfo.RedirectStandardError to be true.  " +
                            "In this instance, Process.EnableRaisingEvents is {0} and Process.StartInfo.RedirectStandardError is {1}",
                            _process.EnableRaisingEvents, _process.StartInfo.RedirectStandardError));
                }

                _process.BeginErrorReadLine();
                _process.ErrorDataReceived += value;
            }
            remove { _process.ErrorDataReceived -= value; }
        }

        /// <summary>
        /// Occurs when an application writes to its redirected StandardOutput stream.
        /// </summary>
        /// <remarks>
        /// The OutputDataReceived event indicates that the associated process has written to its redirected StandardOutput stream.
        /// The event only occurs during asynchronous read operations on StandardOutput. To start asynchronous read operations, 
        /// you must redirect the StandardOutput stream of a Process, add your event handler to the OutputDataReceived event, 
        /// and call BeginOutputReadLine. Thereafter, the OutputDataReceived event signals each time the process writes a 
        /// line to the redirected StandardOutput stream, until the process exits or calls CancelOutputRead.
        /// <note>The application that is processing the asynchronous output should call the WaitForExit method to ensure that the output buffer has been flushed.</note>
        /// </remarks>
        event DataReceivedEventHandler IProcess.OutputDataReceived
        {
            add
            {
                if (!_process.EnableRaisingEvents || !_process.StartInfo.RedirectStandardOutput)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "OutputDataReceived event requires Process.EnableRaisingEvents and Process.StartInfo.RedirectStandardOutput to be true.  " +
                            "In this instance, Process.EnableRaisingEvents is {0} and Process.StartInfo.RedirectStandardOutput is {1}",
                            _process.EnableRaisingEvents, _process.StartInfo.RedirectStandardOutput));
                }
                _process.OutputDataReceived += value;
                _process.BeginOutputReadLine();

            }
            remove
            {
                _process.OutputDataReceived -= value;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>This also closes the underlying process handle, but does not stop the process if it is still running.</remarks>
        public void Dispose()
        {
            _process.Dispose();
        }
    }
}