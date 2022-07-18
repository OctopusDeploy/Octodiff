// ReSharper disable RedundantUsingDirective
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    const string CiBranchNameEnvVariable = "OCTOVERSION_CurrentBranch";

    readonly Configuration Configuration = Configuration.Release;

    [Solution] readonly Solution Solution;
    
    [Parameter("Whether to auto-detect the branch name - this is okay for a local build, but should not be used under CI.")] 
    readonly bool AutoDetectBranch = IsLocalBuild;
    
    [Parameter("Branch name for OctoVersion to use to calculate the version number. Can be set via the environment variable " + CiBranchNameEnvVariable + ".", Name = CiBranchNameEnvVariable)]
    string BranchName { get; set; }
    

    [OctoVersion(BranchParameter = nameof(BranchName), AutoDetectBranchParameter = nameof(AutoDetectBranch))] 
    public OctoVersionInfo OctoVersionInfo;
    
    [Parameter("Test filter expression", Name = "where")] readonly string TestFilter = string.Empty;

    AbsolutePath SourceDirectory => RootDirectory / "source";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath LocalPackagesDirectory => RootDirectory / ".." / "LocalPackages";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "./source/**/TestResults").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target CalculateVersion => _ => _
        .Executes(() =>
        {
            
            //all the magic happens inside `[NukeOctoVersion]` above. we just need a target for TeamCity to call
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building Octopus.Time v{Version}", OctoVersionInfo.FullSemVer);
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(OctoVersionInfo.MajorMinorPatch)
                .SetFileVersion(OctoVersionInfo.MajorMinorPatch)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .EnableNoRestore());
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
            SourceDirectory.GlobFiles( "**/*.trx")
                .ForEach(x => CopyFileToDirectory(x, ArtifactsDirectory, FileExistsPolicy.Overwrite));
        });

    Target Pack => _ => _
        .DependsOn(CalculateVersion)
        .DependsOn(Compile)
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetNoBuild(true)
                .AddProperty("Version", OctoVersionInfo.FullSemVer)
            );
        });

    Target CopyToLocalPackages => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .DependsOn(Pack)
        .Executes(() =>
        {
            EnsureExistingDirectory(LocalPackagesDirectory);
            CopyFileToDirectory(ArtifactsDirectory / $"{Solution.Name}.{OctoVersionInfo.FullSemVer}.nupkg",
                LocalPackagesDirectory, FileExistsPolicy.Overwrite);
        });

    Target Default => _ => _
        .DependsOn(Pack)
        .DependsOn(CopyToLocalPackages);

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Default);
}
