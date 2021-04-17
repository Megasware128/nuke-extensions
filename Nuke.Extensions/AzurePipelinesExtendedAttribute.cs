using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Megasware128.Nuke.Extensions.Configuration;
using Megasware128.Nuke.Extensions.ValueInjection;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities;
using Nuke.Common.ValueInjection;
using static Nuke.Common.IO.PathConstruction;

namespace Megasware128.Nuke.Extensions
{
    public class AzurePipelinesExtendedAttribute : AzurePipelinesAttribute
    {
        public AzurePipelinesExtendedAttribute(AzurePipelinesImage image, params AzurePipelinesImage[] images) : base(image, images)
        {
        }

        public AzurePipelinesExtendedAttribute(string suffix, AzurePipelinesImage image, params AzurePipelinesImage[] images) : base(suffix, image, images)
        {
        }

        public bool NuGetAuthenticate { get; set; }

        protected override IEnumerable<AzurePipelinesStep> GetSteps(ExecutableTarget executableTarget, IReadOnlyCollection<ExecutableTarget> relevantTargets)
            => base.GetSteps(executableTarget, relevantTargets).Prepend(new AzurePipelinesNuGetAuthenticateStep());

        public override ConfigurationEntity GetConfiguration(NukeBuild build, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var parameters = GetGlobalParameters(build, relevantTargets).ToArray();

            if (parameters.Length > 0)
            {
                var importSecrets = ImportSecrets;

                ImportSecrets = importSecrets.Concat(parameters.Select(p => p.Name)).ToArray();

                var config = (AzurePipelinesConfiguration)base.GetConfiguration(build, relevantTargets);

                ImportSecrets = importSecrets;

                return new AzurePipelinesVariablesConfiguration
                {
                    VariableGroups = config.VariableGroups,
                    VcsPushTrigger = config.VcsPushTrigger,
                    Stages = config.Stages,
                    Variables = parameters
                };
            }

            return base.GetConfiguration(build, relevantTargets);
        }

        protected virtual IEnumerable<AzurePipelinesVariable> GetGlobalParameters(NukeBuild build, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            return ValueInjectionUtility.GetParameterMembers(build.GetType(), includeUnlisted: false)
                // TODO: except build.ExecutableTargets ?
                .Except(relevantTargets.SelectMany(x => x.Requirements
                    .Where(y => !(y is Expression<Func<bool>>))
                    .Select(y => y.GetMemberInfo())))
                .Where(x => x.DeclaringType != typeof(NukeBuild) || x.Name == nameof(NukeBuild.Verbosity))
                .Select(x => GetParameter(x, build, required: false));
        }

        protected virtual AzurePipelinesVariable GetParameter(MemberInfo member, NukeBuild build, bool required)
        {
            var attribute = member.GetCustomAttribute<ParameterAttribute>();
            var valueSet = ParameterService.GetParameterValueSet(member, build);
            var valueSeparator = attribute.Separator ?? " ";

            // TODO: Abstract AbsolutePath/Solution/Project etc.
            var defaultValue = !member.HasCustomAttribute<SecretAttribute>() ? member.GetValue(build) : default(string);
            // TODO: enumerables of ...
            if (defaultValue != null &&
                (member.GetMemberType() == typeof(AbsolutePath) ||
                 member.GetMemberType() == typeof(Solution) ||
                 member.GetMemberType() == typeof(Project)))
                defaultValue = (UnixRelativePath)GetRelativePath(NukeBuild.RootDirectory, defaultValue.ToString());

            return new AzurePipelinesVariable(ParameterService.GetParameterMemberName(member),
                member.GetMemberType().IsArray && defaultValue is IEnumerable enumerable
                ? enumerable.Cast<object>().Select(x => x.ToString()).Join(valueSeparator)
                : defaultValue?.ToString());
        }
    }
}
