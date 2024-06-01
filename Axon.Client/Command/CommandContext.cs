using System.Linq;

namespace Axon.Client.Command;

public class CommandContext
{
    public string Command {  get; private set; }
    public string[] Arguments { get; private set; }
    public string FullCommand { get; private set; }

    public CommandContext(string command)
    {
        var allArgs = command.Split(' ');
        Command = allArgs[0];
        Arguments = allArgs.Skip(1).ToArray();
        FullCommand = command;
    }
}
