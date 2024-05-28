using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Command;

public interface IAxonCommand
{
    public CommandResult Execute(CommandContext context);
}
