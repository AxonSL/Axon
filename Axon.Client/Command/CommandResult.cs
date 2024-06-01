using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Command;

public class CommandResult
{
    public CommandStatusCode StatusCode { get; set; }
    public string Message { get; set; }

    public static implicit operator CommandResult(string message) => new CommandResult { StatusCode = CommandStatusCode.Success, Message = message };
}
