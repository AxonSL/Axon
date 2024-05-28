using Axon.Client.Meta;
using Il2CppGameCore;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityEngine;

namespace Axon.Client.Command;

public static class CommandHandler
{
    public static ReadOnlyDictionary<AxonCommandAttribute, IAxonCommand> Commands { get; private set; } = new(new Dictionary<AxonCommandAttribute, IAxonCommand>());

    internal static void Init()
    {
        MetaAnalyzer.OnMeta.Subscribe(OnMeta);
    }

    private static void OnMeta(MetaEvent ev)
    {
        var info = ev.GetAttribute<AxonCommandAttribute>();
        if (info == null) return;
        if (!ev.Is<IAxonCommand>()) return;

        var cmd = ev.CreateAs<IAxonCommand>();
        var dic = new Dictionary<AxonCommandAttribute, IAxonCommand>(Commands);
        dic[info] = cmd;
        Commands = new(dic);
    }

    public static bool OnCommand(string command)
    {
        var context = new CommandContext(command);
        foreach(var cmd in Commands)
        {
            var info = cmd.Key;
            if (string.Equals(info.Name, context.Command, StringComparison.OrdinalIgnoreCase))
            {
                ExecuteCommand(context, cmd.Value);
                return true;
            }

            foreach(var alias in info.Aliase)
            {
                if (string.Equals(alias, context.Command, StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteCommand(context, cmd.Value);
                    return true;
                }
            }
        }
        return false;
    }

    private static void ExecuteCommand(CommandContext context, IAxonCommand command)
    {
        var result = command.Execute(context);
        //TODO: Implement more Statuses
        Il2CppGameCore.Console.AddLog(result.Message, Color.gray, false, Il2CppGameCore.Console.ConsoleLogType.Log);
    }
}
