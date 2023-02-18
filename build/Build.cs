using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Description("Cleans all the output directories")
        .Before(Restore)
        .Executes(() =>
        {
            RootDirectory
                .GlobDirectories("src/**/bin", "src/**/obj", SourceDirectory, TestsDirectory)
                .ForEach(DeleteDirectory);

            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Description("Restores all nuget packages")
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Install => _ => _
        .OnlyWhenStatic(() => !IsLocalBuild)
        .Description("Installs `Nuke.GlobalTool` and `GitVersion.Tool`")
        .Executes(() =>
        {
            DotNetToolInstall(s => s
                .SetPackageName("Nuke.GlobalTool")
                .EnableGlobal()
                .SetVersion("6.3.0"));

            DotNetToolInstall(s => s
                .SetPackageName("GitVersion.Tool")
                .EnableGlobal()
                .SetVersion("5.12.0"));
        });

    Target Compile => _ => _
        .Description("Builds all the projects in the solution")
        .DependsOn(Install, Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });
}
