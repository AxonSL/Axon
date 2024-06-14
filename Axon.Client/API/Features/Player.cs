using System.Collections.Generic;
using System.Collections.ObjectModel;
using Il2Cpp;
using Il2CppInventorySystem.Items;
using Il2CppUtils.NonAllocLINQ;
using UnityEngine;

namespace Axon.Client.API.Features;

public class Player
{
    public static ReadOnlyCollection<Player> PlayerList { get; internal set; } = new(new List<Player>());
    public static ReadOnlyDictionary<GameObject, Player> GameObjectToPlayer { get; internal set; } = new(new Dictionary<GameObject, Player>());

    public Player(GameObject gameObject) : this(ReferenceHub.GetHub(gameObject)) { }
    public Player(ReferenceHub hub)
    {
        ReferenceHub = hub;

        var list = new List<Player>(PlayerList)
        {
            this
        };
        PlayerList = new(list);

        var dic = new Dictionary<GameObject, Player>(GameObjectToPlayer);
        dic[hub.gameObject] = this;
        GameObjectToPlayer = new(dic);
    }

    public ReferenceHub ReferenceHub;

    public Vector3 Position => ReferenceHub.gameObject.transform.position;
    public Transform Transform => ReferenceHub.gameObject.transform;


    public Il2CppSystem.Collections.Generic.List<ItemBase> Inventory
    {
        get
        {
            var itemBases = new Il2CppSystem.Collections.Generic.List<ItemBase>();
            foreach (var userInventoryItem in ReferenceHub.inventory.UserInventory.Items)
            {
                itemBases.Add(userInventoryItem.Value);
            }
            return itemBases;
        }
    }

    public static Player Get(GameObject gameObject)
    {
        if (gameObject == null)
            return null;
        Player player;
        if (Player.GameObjectToPlayer.TryGetValue(gameObject, out player))
            return player;
        return player;
    }
}