using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Axon.Client.Meta;
using MelonLoader;
using static Il2CppSubtitles.SubtitleCategory;
using UnityEngine;
using Axon.Client.AssetBundle;

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
        var bundle = AssetBundleManager.AssetBundles.First().Value;
        var asset = bundle.LoadAsset<GameObject>("Assets/v1.prefab");
        var obj = GameObject.Instantiate(asset);
        obj.name = "Axon Asset";
        obj.transform.position = Vector3.zeroVector;
        obj.transform.localScale = Vector3.one;
        MelonLogger.Msg("Axon command executed!");
        return "You are running Axon version: " + AxonMod.AxonVersion;
    }
}
