using System.Collections.Generic;
using System.Linq;
using Megasware128.Nuke.Extensions.Configuration;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Execution;

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
    }
}
