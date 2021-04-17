using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;

namespace Megasware128.Nuke.Extensions.Configuration
{
    public class AzurePipelinesNuGetAuthenticateStep : AzurePipelinesStep
    {
        public override void Write(CustomFileWriter writer) => writer.WriteLine("- task: NuGetAuthenticate@0");
    }
}
