using System.Collections.Generic;
using Il2Cpp;
using Il2CppInventorySystem.Items;
using Il2CppUtils.NonAllocLINQ;
using UnityEngine;

namespace Axon.Client.API.Features;
public class Player
{

    public Player(GameObject gameObject)
    {
        ReferenceHub = ReferenceHub.GetHub(gameObject);
        List.Add(this);
        Dictionary.Add(gameObject, this);
    }
    public Player()
    {

    }
    public readonly static List<Player> List = new List<Player>();
    public readonly ReferenceHub ReferenceHub;

    public Vector3 Position => ReferenceHub.gameObject.transform.position;
    public Transform Transform => ReferenceHub.gameObject.transform;

    public Camera Camera => ReferenceHub.PlayerCameraReference.gameObject.GetComponent<Camera>();
    public Il2CppSystem.Collections.Generic.List<ItemBase> Inventory
    {
        get
        {
            var itemBases = new Il2CppSystem.Collections.Generic.List<ItemBase>();
            ReferenceHub.inventory.UserInventory.Items.ForEach();   
        } 
    }

    public static Dictionary<GameObject, Player> Dictionary = new Dictionary<GameObject, Player>();

    public static Player Get(GameObject gameObject)
    {
        if (gameObject == (Object) null)
            return null;
        Player player;
        if (Player.Dictionary.TryGetValue(gameObject, out player))
            return player;
        return player;
    }

}