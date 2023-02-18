using System;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions("release-main",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "main", "develop" },
    ImportSecrets = new[] { nameof(NuGetApiKey) },
    InvokedTargets = new[] { nameof(Clean), nameof(NugetPush) },
    FetchDepth = 0)]
partial class Build
{
    [Parameter] readonly string NuGetPublishUrl = "https://api.nuget.org/v3/index.json";
    [Parameter] [Secret] readonly string NuGetApiKey;

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .CombineWith(GetPackageProjects(), (s, p) => s.SetProject(p)));
        });

    Target NugetPush => _ => _
        .Requires(() => NuGetApiKey)
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => IsCiBuild() && IsStableOrRelease())
        .Executes(() =>
        {
            PushNuGetPackages(NuGetPublishUrl, NuGetApiKey);
        });

    void PushNuGetPackages(string githubPackageSource, string githubTokenVariable)
    {
        var packages = ArtifactsDirectory.GlobFiles("*.nupkg");

        DotNetNuGetPush(s => s
            .SetSource(githubPackageSource)
            .SetApiKey(Environment.GetEnvironmentVariable(githubTokenVariable))
            .CombineWith(packages, (s, p) => s
                .SetTargetPath(p)));
    }

    IEnumerable<AbsolutePath> GetPackageProjects() => SourceDirectory.GlobFiles("**/**.csproj");

    bool IsCiBuild() => GitHubActions.Instance != null;

    bool IsStableOrRelease() => GitVersion.PreReleaseLabel.Equals("rc", StringComparison.OrdinalIgnoreCase) ||
                                string.IsNullOrEmpty(GitVersion.PreReleaseLabel);
}
