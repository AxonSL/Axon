using Axon.Client.Event.Args;
using Axon.Client.Event.Handlers;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Axon.Client.Components;

[RegisterTypeInIl2Cpp]
public class CreditsHookComponent : MonoBehaviour
{
    public static CreditsHookComponent Singleton;
    public CreditsHookComponent(IntPtr intPtr) : base(intPtr) {}
        
    public CreditsHookComponent(){}
    private bool _creditsReadyToUse;
    private bool _objectsCreated;
    public bool IsCreditsHookReadyToUse => _creditsReadyToUse;

    private readonly Dictionary<string, CreditsCategoryInfo> _categoryInfos = new Dictionary<string, CreditsCategoryInfo>();

    private GameObject _titleGameObject;
    private GameObject _roleGameObject;
        
    private void OnEnable()
    {
        Singleton = this;
        _creditsReadyToUse = false;
        foreach (CreditsCategoryInfo category in _categoryInfos.Values)
        {
            category.GameObjects = new CreditsCategoryGameObjects();
        }
        _objectsCreated = _categoryInfos.Values.Count == 0;
    }

    private void OnDisable()
    {
        foreach (CreditsCategoryInfo category in _categoryInfos.Values)
        {
            category.GameObjects = new CreditsCategoryGameObjects();
        }
    }

    private void Update()
    {
        if (!_creditsReadyToUse)
        {
            GameObject content = GameObject.Find("/New Main Menu/Credits/root/Content");
            if (content != null)
            {
                if (content.transform.childCount >= 310)
                {
                    _titleGameObject = GameObject.Find("/New Main Menu/Credits/root/Content/CreditsCategory(Clone)");
                    _roleGameObject = GameObject.Find("/New Main Menu/Credits/root/Content/CreditsElement with Role(Clone)/");
                    _creditsReadyToUse = true;
                    MenuHandler.CreditsHook.Raise(new CreditHookEventArg(this));
                }
            }
        }
            
        if (_creditsReadyToUse && !_objectsCreated)
        {
            foreach (CreditsCategoryInfo cat in _categoryInfos.Values)
            {
                cat.GameObjects.CategoryObject = GenerateCategoryObject(cat.CategoryName);
                foreach (UserInfo user in cat.UserEntries)
                {
                    cat.GameObjects.EntryObjects.Add(GenerateEntryObject(user.Username, user.Role, user.Color));
                }
            }

            _objectsCreated = true;
            SortElements();
        }
    }

    private void SortElements()
    {
        int counter = 2;
        foreach (CreditsCategoryInfo cat in _categoryInfos.Values)
        {
            cat.GameObjects.CategoryObject.transform.SetSiblingIndex(counter++);
            foreach (GameObject entry in cat.GameObjects.EntryObjects)
            {
                entry.transform.SetSiblingIndex(counter++);
            }
        }
    }
        
    public CreditsCategoryInfo GetCreditsCategory(string catName)
    {
        if (!_creditsReadyToUse)
            return null;

        return _categoryInfos.ContainsKey(catName) ? _categoryInfos[catName] : null;
    }

    public bool CreateCreditsCategory(string catName)
    {
        if (!_creditsReadyToUse)
            return false;

        if (_categoryInfos.ContainsKey(catName))
            return false;

        CreditsCategoryInfo catInfo = new CreditsCategoryInfo
        {
            CategoryName = catName, GameObjects ={
                CategoryObject = GenerateCategoryObject(catName)
            }
        };

        _categoryInfos.Add(catName, catInfo);
            
        SortElements();
            
        return true;
    }
        
    private GameObject GenerateCategoryObject(string catName)
    {
        GameObject result = Instantiate(_titleGameObject, _titleGameObject.transform.parent, true);
        result.GetComponent<TMP_Text>().text = catName;

        return result;
    }

    public bool CreateCreditsEntry(string username, string role, string category, Color color)
    {
        if (!_creditsReadyToUse)
            return false;

        if (!_categoryInfos.ContainsKey(category))
            return false;

        CreditsCategoryInfo cat = _categoryInfos[category];

        cat.GameObjects.EntryObjects.Add(GenerateEntryObject(username, role, color));
            
        cat.UserEntries.Add(new UserInfo()
        {
            Username = username,
            Role = role,
            Color = color
        });
            
        SortElements();
            
        return true;
    }
        
    private GameObject GenerateEntryObject(string username, string role, Color color)
    {
        GameObject result = Instantiate(_roleGameObject, _roleGameObject.transform.parent, true);

        result.transform.GetChild(0).GetComponent<TMP_Text>().text = username;
        result.transform.GetChild(1).GetComponent<Image>().color = color;
        result.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = role;

        return result;
    }
}
    
public class UserInfo
{
    public string Username;
    public string Role;
    public Color Color;
}

public class CreditsCategoryGameObjects
{
    public GameObject CategoryObject;
    public List<GameObject> EntryObjects = new List<GameObject>();
}

public class CreditsCategoryInfo
{
    public string CategoryName;
    public List<UserInfo> UserEntries = new List<UserInfo>();
    public CreditsCategoryGameObjects GameObjects = new CreditsCategoryGameObjects();
}
    
public class CreditColors
{
    public static readonly Color Yellow = new Color(0.8471f, 0.5922f, 0.1882f, 1);
    public static readonly Color Red = new Color(0.8196f, 0.1333f, 0.1333f, 1);
    public static readonly Color Purple = new Color(0.098f, 0.0235f, 0.4275f, 1);
    public static readonly Color CrabPink = new Color(0.9765f, 0.4078f, 0.3294f, 1);
    public static readonly Color DevBlue = new Color(0.14f, 0.41f, 1);
}