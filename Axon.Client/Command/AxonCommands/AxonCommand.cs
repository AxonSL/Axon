using Axon.Shared.Meta;
using MelonLoader;

namespace Axon.Client.Command.AxonCommands;

[Automatic]
[AxonCommand(
    Name = "Axon",
    Aliase = new[] { "ax" },
    Description = "Default Axon command"
    )]
public class AxonCommand : IAxonCommand
{
    public CommandResult Execute(CommandContext _)
    {
        MelonLogger.Msg("Axon command executed!");
        return "You are running Axon version: " + AxonMod.AxonVersion;
    }
}
