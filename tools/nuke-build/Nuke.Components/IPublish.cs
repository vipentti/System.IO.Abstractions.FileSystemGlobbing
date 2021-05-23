﻿// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System.Collections.Generic;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ValueInjection.ValueInjectionUtility;

namespace Nuke.Components
{
    [PublicAPI]
    public interface IPublish : IPack, ITest
    {
        [Parameter] string NuGetSource => TryGetValue(() => NuGetSource) ?? "https://api.nuget.org/v3/index.json";
        [Parameter] [Secret] string NuGetApiKey => TryGetValue(() => NuGetApiKey);

        Target Publish => _ => _
            .DependsOn(Test, Pack)
            .Requires(() => NuGetApiKey)
            .Executes(() =>
            {
                DotNetNuGetPush(_ => _
                        .Apply(PushSettingsBase)
                        .Apply(PushSettings)
                        .CombineWith(PushPackageFiles, (_, v) => _
                            .SetTargetPath(v))
                        .Apply(PackagePushSettings),
                    PushDegreeOfParallelism,
                    PushCompleteOnFailure);
            });

        sealed Configure<DotNetNuGetPushSettings> PushSettingsBase => _ => _
            .SetSource(NuGetSource)
            .SetApiKey(NuGetApiKey);

        Configure<DotNetNuGetPushSettings> PushSettings => _ => _;
        Configure<DotNetNuGetPushSettings> PackagePushSettings => _ => _;

        IEnumerable<AbsolutePath> PushPackageFiles => PackagesDirectory.GlobFiles("*.nupkg");

        bool PushCompleteOnFailure => true;
        int PushDegreeOfParallelism => 5;
    }
}
