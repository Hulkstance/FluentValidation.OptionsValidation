﻿using System.Linq;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Utilities.Collections;

namespace _build;

public static class GitRepositoryExtensions
{
    public static bool CurrentCommitHasVersionTag(this GitRepository gitRepository)
    {
        var versionTagsOnCurrentCommit = gitRepository.Tags
            .Select(t => SemanticVersion.TryParse(t.TrimStart('v'), out var v) ? v : null)
            .WhereNotNull();

        return versionTagsOnCurrentCommit.Any();
    }

    public static SemanticVersion GetLatestVersionTag(this GitRepository gitRepository)
    {
        var versionTagsOnCurrentCommit = gitRepository.Tags
            .Select(t => SemanticVersion.TryParse(t.TrimStart('v'), out var v) ? v : null)
            .WhereNotNull()
            .OrderByDescending(t => t).ToArray();

        Assert.True(versionTagsOnCurrentCommit.Any(), $"The current commit '{gitRepository.Commit}' has no valid tag!");

        return versionTagsOnCurrentCommit.First();
    }
}
