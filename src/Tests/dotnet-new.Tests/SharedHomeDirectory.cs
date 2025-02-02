// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.NET.TestFramework.Assertions;
using Microsoft.NET.TestFramework.Commands;
using Microsoft.TemplateEngine.TestHelper;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    /// <summary>
    /// This class represents shared /tmp/RANDOM-GUID/.templateengine/dotnetcli-preview/ folder
    /// shared between multiple unit tests in same class, this is so each test
    /// doesn't have to install everything from 0. To save some time executing tests.
    /// </summary>
    public class SharedHomeDirectory : IDisposable
    {
        private readonly HashSet<string> _installedPackages = new();

        public SharedHomeDirectory(IMessageSink messageSink)
        {
            Log = new SharedTestOutputHelper(messageSink);
            Initialize();
        }

        public string HomeDirectory { get; } = Utilities.CreateTemporaryFolder(nameof(SharedHomeDirectory));

        protected ITestOutputHelper Log { get; private set; }

        public void Dispose() => Directory.Delete(HomeDirectory, true);

        public void InstallPackage(string packageName, string? workingDirectory = null, string? nugetSource = null)
        {
            if (!_installedPackages.Add(packageName))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }
            var args = new List<string> { "-i", packageName, };
            if (!string.IsNullOrWhiteSpace(nugetSource))
            {
                args.AddRange(new[] { "--nuget-source", nugetSource });
            }
            new DotnetNewCommand(Log, args.ToArray())
                .WithCustomHive(HomeDirectory)
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }

        private void Initialize()
        {
            new DotnetNewCommand(Log)
                .WithCustomHive(HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();

            new DotnetNewCommand(Log, "--install", TemplatePackagesPaths.MicrosoftDotNetCommonProjectTemplates31Path)
                .WithCustomHive(HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();

            new DotnetNewCommand(Log, "--install", TemplatePackagesPaths.MicrosoftDotNetCommonProjectTemplates50Path)
                .WithCustomHive(HomeDirectory)
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr();
        }
    }
}
