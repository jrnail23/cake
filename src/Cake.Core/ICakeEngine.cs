﻿using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Core
{
    public interface ICakeEngine
    {
        IFileSystem FileSystem { get; }
        ICakeEnvironment Environment { get; }
        IGlobber Globber { get; }
        ILogger Log { get; }

        CakeTask Task(string name);
        void Run(string target);
    }
}