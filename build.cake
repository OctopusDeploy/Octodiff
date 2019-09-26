//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"

//////////////////////////////////////////////////////////////////////
// USINGS
//////////////////////////////////////////////////////////////////////
using Path = System.IO.Path;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var localPackagesDir = "../LocalPackages";
var artifactsDir = "./artifacts";
var packageName = "Octodiff";

GitVersion gitVersionInfo;
string nugetVersion;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json
    });
    nugetVersion = gitVersionInfo.NuGetVersion;

    if(BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(nugetVersion);

    Information("Building Octodiff v{0}", nugetVersion);
    Information("AssemblyVersion -> {0}", gitVersionInfo.AssemblySemVer);
    Information("AssemblyFileVersion -> {0}", $"{gitVersionInfo.MajorMinorPatch}.0");
    Information("AssemblyInformationalVersion -> {0}", gitVersionInfo.InformationalVersion);
});

Teardown(context =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDir);
        CleanDirectories("./**/bin");
        CleanDirectories("./**/obj");
        CleanDirectories("./source/**/TestResults");
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore("source", new DotNetCoreRestoreSettings
        {
            ArgumentCustomization = args => args.Append("--verbosity normal")
        });
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild("./source/Octodiff.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            ArgumentCustomization = args => args
                .Append($"/p:Version={nugetVersion}")
                .Append($"/p:InformationalVersion={gitVersionInfo.InformationalVersion}")
                .Append("--verbosity normal")
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        GetFiles("./source/**/*Tests.csproj")
            .ToList()
            .ForEach(testProjectFile =>
            {
                DotNetCoreTest(testProjectFile.ToString(), new DotNetCoreTestSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                    ArgumentCustomization = args => args
                        .Append("--logger:trx")
                        .Append("--verbosity normal")
                });
            });
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
    {
        DotNetCorePack("./source/OctoDiff", new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDir,
            NoBuild = true,
            ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
        });
    });

Task("Publish")
    .WithCriteria(BuildSystem.IsRunningOnTeamCity)
    .IsDependentOn("Pack")
    .Does(() =>
    {
        NuGetPush($"{artifactsDir}/{packageName}.{nugetVersion}.nupkg", new NuGetPushSettings {
            Source = "https://f.feedz.io/octopus-deploy/dependencies/nuget",
            ApiKey = EnvironmentVariable("FeedzIoApiKey")
        });

        if (string.IsNullOrWhiteSpace(gitVersionInfo.PreReleaseLabel))
        {
            NuGetPush($"{artifactsDir}/{packageName}.{nugetVersion}.nupkg", new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = EnvironmentVariable("NuGetApiKey")
            });
        }
    });

Task("CopyToLocalPackages")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .IsDependentOn("Pack")
    .Does(() =>
    {
        CreateDirectory(localPackagesDir);
        CopyFiles(Path.Combine(artifactsDir, $"{packageName}.{nugetVersion}.nupkg"), localPackagesDir);
    });

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("CopyToLocalPackages")
    .IsDependentOn("Publish");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
