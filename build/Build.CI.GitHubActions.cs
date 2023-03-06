using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities.Collections;
using Nuke.GitHub;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.GitHub.GitHubTasks;
using static Nuke.Common.IO.PathConstruction;

[GitHubActions("CI",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    AutoGenerate = true,
    OnPushBranches = new[] { "main" },
    ImportSecrets = new[] { nameof(NuGetApiKey), nameof(PersonalAccessToken) },
    InvokedTargets = new[] { nameof(NuGetPush), nameof(GitHubPush), nameof(PublishGithubRelease) })]
partial class Build
{
    [Parameter("NuGet API Key")] [Secret] readonly string NuGetApiKey;
    [Parameter("GitHub Personal Access Token")] [Secret] readonly string PersonalAccessToken;

    [GitRepository] readonly GitRepository GitRepository;

    Target Pack => _ => _
        .Description("Packs the project")
        .DependsOn(Compile)
        .Produces(ArtifactsDirectory / "*.nupkg")
        .Executes(() =>
        {
            Solution.GetProjects("FluentValidation.OptionsValidation*")
                .ForEach(x =>
                {
                    DotNetPack(s => s
                        .SetProject(Solution.GetProject(x))
                        .SetConfiguration(Configuration)
                        .SetOutputDirectory(ArtifactsDirectory)
                        .SetVersion(GitVersion.SemVer)
                        .EnableNoRestore()
                        .EnableNoBuild());
                });
        });

    Target NuGetPush => _ => _
        .Description("Pushes the package to NuGet")
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => IsServerBuild)
        .Requires(() => NuGetApiKey)
        .Executes(() =>
        {
            Assert.True(!string.IsNullOrEmpty(NuGetApiKey));

            GlobFiles(ArtifactsDirectory, "*.nupkg")
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource("https://api.nuget.org/v3/index.json")
                        .SetApiKey(NuGetApiKey)
                        .EnableSkipDuplicate());
                });
        });

    Target GitHubPush => _ => _
        .Description("Pushes the package to GitHub")
        .DependsOn(Pack)
        .OnlyWhenStatic(() => IsServerBuild)
        .Requires(() => PersonalAccessToken)
        .Executes(() =>
        {
            Assert.True(!string.IsNullOrEmpty(PersonalAccessToken));

            GlobFiles(ArtifactsDirectory, "*.nupkg")
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource($"https://nuget.pkg.github.com/{GitRepository.GetGitHubOwner()}/index.json")
                        .SetApiKey(PersonalAccessToken)
                        .EnableSkipDuplicate());
                });
        });

    Target PublishGithubRelease => _ => _
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch())
        .Requires(() => PersonalAccessToken)
        .Executes<Task>(async () =>
        {
            var nugetPackages = ArtifactsDirectory.GlobFiles("*.nupkg")
                .Select(x => x.ToString())
                .ToArray();

            Assert.NotEmpty(nugetPackages);

            await PublishRelease(c => c
                .SetArtifactPaths(nugetPackages)
                .SetCommitSha(GitVersion.Sha)
                .SetRepositoryName(GitRepository.GetGitHubName())
                .SetRepositoryOwner(GitRepository.GetGitHubOwner())
                .SetTag($"v{GitVersion.Major}.{GitVersion.Minor}.{GitVersion.Patch}")
                .SetToken(PersonalAccessToken)
                .DisablePrerelease());
        });
}
