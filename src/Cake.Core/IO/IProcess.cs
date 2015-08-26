﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cake.Core.IO
{
    /// <summary>
    /// Represents a process.
    /// </summary>
    public interface IProcess : IDisposable
    {
        /// <summary>
        /// Starts a process resource and associates it with a Process component.
        /// </summary>
        /// <returns>true if a process resource is started; false if no new process resource is started (for example, if an existing process is reused).</returns>
        bool Start();

        /// <summary>
        /// Waits for the process to exit.
        /// </summary>
        void WaitForExit();

        /// <summary>
        /// Waits for the process to exit with possible timeout for command.
        /// </summary>
        /// <param name="milliseconds">The amount of time, in milliseconds, to wait for the associated process to exit. The maximum is the largest possible value of a 32-bit integer, which represents infinity to the operating system.</param>
        /// <returns>true if the associated process has exited; otherwise, false.</returns>
        bool WaitForExit(int milliseconds);

        /// <summary>
        /// Gets the exit code of the process.
        /// </summary>
        /// <returns>The exit code of the process.</returns>
        int GetExitCode();

        /// <summary>
        /// Get the standard output of process
        /// </summary>
        /// <returns>Returns process output <see cref="ProcessSettings.RedirectStandardOutput">RedirectStandardOutput</see> is true</returns>
        IEnumerable<string> GetStandardOutput();

        /// <summary>
        /// Get the standard error output of process
        /// </summary>
        /// <returns>Returns process error output <see cref="ProcessSettings.RedirectStandardError">RedirectStandardError</see> is true</returns>
        IEnumerable<string> GetStandardError();

        /// <summary>
        /// Gets the unique identifier for the associated process.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        int ProcessId { get; }

        /// <summary>
        /// Immediately stops the associated process.
        /// </summary>
        void Kill();

        /// <summary>
        /// Gets a value indicating whether this instance has exited.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has exited; otherwise, <c>false</c>.
        /// </value>
        bool HasExited { get; }

        /// <summary>
        /// Occurs when process exits, when <see cref="ProcessSettings.EnableRaisingEvents">EnableRaisingEvents</see> is true.
        /// </summary>
        event EventHandler Exited;

        /// <summary>
        /// Occurs when an application writes to its redirected StandardError stream..
        /// </summary>
        event DataReceivedEventHandler ErrorDataReceived;

        /// <summary>
        /// Occurs when an application writes to its redirected StandardOutput stream..
        /// </summary>
        event DataReceivedEventHandler OutputDataReceived;
    }
}
