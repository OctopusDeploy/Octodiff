// ReSharper disable RedundantUsingDirective

using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Nuke.OctoVersion;
using OctoVersion.Core;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;

    [NukeOctoVersion] readonly OctoVersionInfo OctoVersionInfo;

    [Solution] readonly Solution Solution;

    [Parameter("Test filter expression", Name = "where")] readonly string TestFilter = string.Empty;

    AbsolutePath SourceDirectory => RootDirectory / "source";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath LocalPackagesDirectory => RootDirectory / ".." / "LocalPackages";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(OctoVersionInfo.FullSemVer)
                .SetAssemblyVersion(OctoVersionInfo.MajorMinorPatch)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .DependsOn()
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .AddProperty("Version", OctoVersionInfo.FullSemVer)
            );
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetLoggers("trx")
                .SetVerbosity(DotNetVerbosity.Normal)
                .SetFilter(TestFilter)
                .EnableNoBuild()
                .EnableNoRestore());
            GlobFiles(SourceDirectory, "**/*.trx")
                .ForEach(x => CopyFileToDirectory(x, ArtifactsDirectory, FileExistsPolicy.Overwrite));
        });

    Target CopyToLocalPackages => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .TriggeredBy(Compile)
        .DependsOn(Pack)
        .Executes(() =>
        {
            GlobFiles(ArtifactsDirectory, $"*.{OctoVersionInfo.FullSemVer}.nupkg")
                .ForEach(x => CopyFileToDirectory(x, LocalPackagesDirectory, FileExistsPolicy.Overwrite));
        });

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.CopyToLocalPackages, x => x.Test);
}
