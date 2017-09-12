//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0011"
#addin "MagicChunks"

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
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => DotNetCoreRestore("source", new DotNetCoreRestoreSettings
    {
        ArgumentCustomization = args => args.Append("--verbosity normal")
    }));

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
            IncludeSymbols = true,
            ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
        });
    });

Task("Publish")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsRunningOnTeamCity)
    .Does(() =>
    {
        NuGetPush($"artifacts/Octodiff.{nugetVersion}.nupkg", new NuGetPushSettings {
            Source = "https://octopus.myget.org/F/octopus-dependencies/api/v3/index.json",
            ApiKey = EnvironmentVariable("MyGetApiKey")
        });
        NuGetPush($"artifacts/Octodiff.{nugetVersion}.symbols.nupkg", new NuGetPushSettings {
            Source = "https://octopus.myget.org/F/octopus-dependencies/api/v3/index.json",
            ApiKey = EnvironmentVariable("MyGetApiKey")
        });
    });

Task("CopyToLocalPackages")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .Does(() =>
    {
        CreateDirectory(localPackagesDir);
        CopyFiles(Path.Combine(artifactsDir, $"Octodiff.{nugetVersion}.nupkg"), localPackagesDir);
        CopyFiles(Path.Combine(artifactsDir, $"Octodiff.{nugetVersion}.symbols.nupkg"), localPackagesDir);
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
