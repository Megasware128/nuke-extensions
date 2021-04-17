using System;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Megasware128.Nuke.Extensions.Configuration
{
    public class AzurePipelinesVariablesConfiguration : AzurePipelinesConfiguration
    {
        public AzurePipelinesVariable[] Variables { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            if (VariableGroups.Length is 0)
            {
                using (writer.WriteBlock("variables:"))
                {
                    Variables.ForEach(x => writer.WriteLine($"- {x.Name}: {x.Value}"));
                    writer.WriteLine();
                }

                base.Write(writer);
            }
            else
            {
                var variableGroups = VariableGroups;
                VariableGroups = Array.Empty<string>();

                using (writer.WriteBlock("variables:"))
                {
                    variableGroups.ForEach(x => writer.WriteLine($"- group: {x}"));
                    Variables.ForEach(x =>
                    {
                        using (writer.WriteBlock($"- name: {x.Name}"))
                        {
                            writer.WriteLine($"value: {x.Value}");
                        }
                    });
                    writer.WriteLine();
                }

                base.Write(writer);

                VariableGroups = variableGroups;
            }
        }
    }
}
