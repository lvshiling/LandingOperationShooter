using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    #region SINGLETON

    private static ShopManager _instance;

    public static ShopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ShopManager>();
            }
            return _instance;
        }
    }

    #endregion SINGLETON

    [SerializeField]
    public List<ShopItemWeapon> shopItems;

    [SerializeField]
    public List<SkillItem> skillItems;

    [SerializeField]
    private List<ShopItemObject> shopItemObjects;

    [SerializeField]
    private List<SkillItemObject> shopSkillObjects;

    [SerializeField]
    private ShopItemObject prefabObj;

    [SerializeField]
    private SkillItemObject prefabSkillObj;

    [SerializeField]
    private Transform contentParentItems;

    [SerializeField]
    private Transform contentParentSkills;

    [SerializeField]
    private ItemSkillWindow itemSkillWindow;

    private WeaponGD[] weaponGDs;
    private SkillGD[] skillGDs;
    private SkillGD[] skillWeaponGDs;
    private UpgradeCosts[] upgradeCosts;
    private UpgradeCosts[] upgradeCostsItem;

    public bool isHaveItemUpgrade;

    public ShopItem selectedWeapon;
    public ShopItem selectedWeapon2;

    [Header("количество оружия типа1(пистолеты)")]
    public int countWeapon1 = 2;//количество оружия типа1(пистолеты)

    // Use this for initialization
    private void Start()
    {
        LoadFromCloud();
    }

    #region Main

    private void Load()
    {
        shopItems.ForEach(x => x.Load());
        skillItems.ForEach(x => x.Load());
        shopItems.ForEach(x => x.skills.ForEach(y => y.Load(x.ID)));
    }

    private void Save()
    {
        shopItems.ForEach(x => x.Save());
        shopItems.ForEach(x => x.skills.ForEach(y => y.Save(x.ID)));
        skillItems.ForEach(x => x.Save());
    }

    private void Init()
    {
        shopItemObjects = new List<ShopItemObject>();
        if (shopItems.Where(x => x.isSelected == true).FirstOrDefault() == null)
        {
            shopItems.Where(x => x.cost == 0).FirstOrDefault().isSelected = true;
        }
        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItemObjects.Add(Instantiate(prefabObj, contentParentItems));
            if (shopItems[i].cost == 0)
            {
                shopItems[i].isBuy = true;
            }
            shopItemObjects[i].Init(shopItems[i].itemName, shopItems[i].ID, shopItems[i].isBuy, shopItems[i].isSelected, OnBuy, OnSelect, OnUpgradeItem, shopItems[i].sprite, shopItems[i].cost, isHaveItemUpgrade);

            if (i < countWeapon1 && shopItems[i].isSelected)//!
            {
                selectedWeapon = shopItems[i];
            }
            if (i >= countWeapon1 && shopItems[i].isSelected)
            {
                selectedWeapon2 = shopItems[i];
            }
        }
        prefabObj.gameObject.SetActive(false);

        shopSkillObjects = new List<SkillItemObject>();
        for (int i = 0; i < skillItems.Count; i++)
        {
            if (skillItems[i].currentValue < upgradeCosts.Length)
            {
                skillItems[i].cost = upgradeCosts[skillItems[i].currentValue].cost;
            }
            shopSkillObjects.Add(Instantiate(prefabSkillObj, contentParentSkills));
            shopSkillObjects[i].Init(skillItems[i].itemName, skillItems[i].ID, OnUpgrade, skillItems[i].sprite, skillItems[i].cost, skillItems[i].maxCount, skillItems[i].currentValue);
        }
        prefabSkillObj.gameObject.SetActive(false);
        itemSkillWindow.Init(OnUpgradeItemSkill, shopItems[0], upgradeCostsItem);
    }

    private void UpdateDataObject()
    {
        if (shopItemObjects != null && shopItemObjects.Count > 0)
        {
            for (int i = 0; i < shopItemObjects.Count; i++)
            {
                shopItemObjects[i].Init(shopItems[i].isBuy, shopItems[i].isSelected, isHaveItemUpgrade);
            }
        }
    }

    private void UpdateDataSkill()
    {
        if (shopSkillObjects != null && shopSkillObjects.Count > 0)
        {
            for (int i = 0; i < shopSkillObjects.Count; i++)
            {
                shopSkillObjects[i].Init(skillItems[i].maxCount, skillItems[i].currentValue, skillItems[i].cost);
            }
        }
    }

    private void OnBuy(int id)
    {
        ShopItem item = shopItems.Where(x => x.ID == id).FirstOrDefault();
        if (GameController.Instance.moneyPlayer >= item.cost)
        {
            SndManager.Instance.Play("weaponBuy");
            GameController.Instance.moneyPlayer -= item.cost;
            CanvasManager.Instance.UpdateScreenMoney(GameController.Instance.moneyPlayer);
            item.isBuy = true;

            if (id <= countWeapon1)
            {
                MadeSelectOff();
                item.isSelected = true;
                selectedWeapon = item;
            }
            else
            {
                MadeSelectOff2();
                item.isSelected = true;
                selectedWeapon2 = item;
            }

            UpdateDataObject();
            Save();
            AnalyticsWizard.Instance.BuyItem(1, item.ID);
        }
        else
        {
            CanvasManager.Instance.OpenScreen(MenusAtGame.NoMoneyUI);
        }
    }

    private void OnUpgradeItem(int id)
    {
        CanvasManager.Instance.OpenScreen(MenusAtGame.UpgradeWeapon);
        itemSkillWindow.Init(OnUpgradeItemSkill, shopItems.Where(x => x.ID == id).FirstOrDefault(), upgradeCostsItem);
    }

    private void OnUpgradeItemSkill(int id, int idSkill)
    {
        SkillItem item = shopItems.Where(x => x.ID == id).FirstOrDefault().skills.Where(x => x.ID == idSkill).FirstOrDefault();
        if (GameController.Instance.moneyPlayer >= item.cost)
        {
            GameController.Instance.moneyPlayer -= item.cost;
            CanvasManager.Instance.UpdateScreenMoney(GameController.Instance.moneyPlayer);
            item.currentValue++;
            if (item.currentValue < upgradeCosts.Length)
            {
                item.cost = upgradeCosts[item.currentValue].cost;
            }
            itemSkillWindow.UpdateData(shopItems.Where(x => x.ID == id).FirstOrDefault(), upgradeCostsItem);
            Save();
            AnalyticsWizard.Instance.BuyUpgrade(2, item.ID, (float)(item.currentValue - 1) / (float)(item.maxCount - 1));
        }
        else
        {
            CanvasManager.Instance.OpenScreen(MenusAtGame.NoMoneyUI);
        }
    }

    private void OnUpgrade(int id)
    {
        SkillItem item = skillItems.Where(x => x.ID == id).FirstOrDefault();
        if (GameController.Instance.moneyPlayer >= item.cost)
        {
            GameController.Instance.moneyPlayer -= item.cost;
            CanvasManager.Instance.UpdateScreenMoney(GameController.Instance.moneyPlayer);
            item.currentValue++;
            if (item.currentValue < upgradeCosts.Length)
            {
                item.cost = upgradeCosts[item.currentValue].cost;
            }
            UpdateDataSkill();
            Save();
            AnalyticsWizard.Instance.BuyUpgrade(1, item.ID, (float)(item.currentValue - 1) / (float)(item.maxCount - 1));
        }
        else
        {
            CanvasManager.Instance.OpenScreen(MenusAtGame.NoMoneyUI);
        }
    }

    private void MadeSelectOff()
    {
        //shopItems.ForEach(x => x.isSelected = false);
        for (int i = 0; i < countWeapon1; i++)
        {
            shopItems[i].isSelected = false;
        }
    }

    private void MadeSelectOff2()
    {
        //shopItems.ForEach(x => x.isSelected = false);
        for (int i = countWeapon1; i < 6; i++)
        {
            shopItems[i].isSelected = false;
        }
    }

    private void OnSelect(int id)
    {
        ShopItem item = shopItems.Where(x => x.ID == id).FirstOrDefault();
        if (item.isBuy)
        {
            if (id <= countWeapon1)
            {
                MadeSelectOff();
                item.isSelected = true;
                selectedWeapon = item;
            }
            else
            {
                MadeSelectOff2();
                item.isSelected = true;
                selectedWeapon2 = item;
            }

            UpdateDataObject();
            Save();
        }
    }

    public void TabSwitch(int i)
    {
        CanvasManager.Instance.SwitchPanel(i);
    }

    #endregion Main

    #region LoadFromCloud

    private void LoadFromCloud()
    {
        GBNHZinit.AddOnLoadListener(OnLoad);
    }

    private void OnLoad(bool isOk)
    {
        weaponGDs = GBNCloud.ReadSheet<WeaponGD>(GBNHZinit.CloudBalanceId, "WeaponShop");
        skillGDs = GBNCloud.ReadSheet<SkillGD>(GBNHZinit.CloudBalanceId, "SkillShop");
        skillWeaponGDs = GBNCloud.ReadSheet<SkillGD>(GBNHZinit.CloudBalanceId, "SkillShopItem");
        upgradeCosts = GBNCloud.ReadSheet<UpgradeCosts>(GBNHZinit.CloudBalanceId, "UpgradeCost");
        upgradeCostsItem = GBNCloud.ReadSheet<UpgradeCosts>(GBNHZinit.CloudBalanceId, "UpgradeCostItem");
        CanvasManager.Instance.OpenScreen(MenusAtGame.MainMenuUI);
        if (weaponGDs != null && weaponGDs.Length > 0 && skillGDs != null && skillGDs.Length > 0)
        {
            UpdateData();
        }
        else
        {
#if !FINAL_VERSION
            Debug.Log("Zero cloud");
#endif
        }

        Load();
        Init();
    }

    private void UpdateData()
    {
        for (int i = 0; i < weaponGDs.Length; i++)
        {
            ShopItemWeapon item = new ShopItemWeapon();
            if (shopItems != null && i < shopItems.Count && shopItems[i] != null)
            {
                item = shopItems[i] as ShopItemWeapon;
            }
            else
            {
#if UNITY_EDITOR
                ShopItemWeapon asset = ScriptableObject.CreateInstance<ShopItemWeapon>();

                AssetDatabase.CreateAsset(asset, "Assets/Prefabs/Weapons/" + weaponGDs[i].itemName + ".asset");
                AssetDatabase.SaveAssets();
                item = asset;
                shopItems.Add(asset);
#endif
            }
            item.itemName = weaponGDs[i].itemName;
            item.ID = weaponGDs[i].id;
            item.cost = weaponGDs[i].cost;
            item.damage = weaponGDs[i].damage;
            item.accuracy = weaponGDs[i].accuracy;
            item.ammoCount = weaponGDs[i].ammoCount;
            item.reloadTime = weaponGDs[i].reloadTime;
            if (item.skills == null || item.skills.Count < 1)
            {
                item.skills = new List<SkillItem>();
            }
            for (int j = 0; j < skillWeaponGDs.Length; j++)
            {
                SkillItem itemSkill = new SkillItem();
                if (item.skills != null && j < item.skills.Count && item.skills[j] != null)
                {
                    itemSkill = skillItems[j] as SkillItem;
                }
                else
                {
#if UNITY_EDITOR
                    SkillItem asset = ScriptableObject.CreateInstance<SkillItem>();

                    AssetDatabase.CreateAsset(asset, "Assets/Prefabs/SkillsWeapons/" + skillWeaponGDs[j].itemName + item.itemName + ".asset");
                    AssetDatabase.SaveAssets();
                    itemSkill = asset;
                    item.skills.Add(asset);
#endif
                }
                itemSkill.itemName = skillWeaponGDs[j].itemName;
                itemSkill.ID = skillWeaponGDs[j].id;
                itemSkill.maxCount = skillWeaponGDs[j].maxCount;
                itemSkill.currentValue = skillWeaponGDs[j].minCount;
                itemSkill.cost = upgradeCosts[itemSkill.currentValue].cost;
            }
        }

        for (int i = 0; i < skillGDs.Length; i++)
        {
            SkillItem item = new SkillItem();
            if (skillItems != null && i < skillItems.Count && skillItems[i] != null)
            {
                item = skillItems[i] as SkillItem;
            }
            else
            {
#if UNITY_EDITOR
                SkillItem asset = ScriptableObject.CreateInstance<SkillItem>();

                AssetDatabase.CreateAsset(asset, "Assets/Prefabs/Skills/" + skillGDs[i].itemName + ".asset");
                AssetDatabase.SaveAssets();
                item = asset;
                skillItems.Add(asset);
#endif
            }
            item.itemName = skillGDs[i].itemName;
            item.ID = skillGDs[i].id;
            item.maxCount = skillGDs[i].maxCount;
            item.currentValue = skillGDs[i].minCount;
            item.cost = upgradeCosts[item.currentValue].cost;
        }
    }

    public SkillItem GetSkillByName(string ID)
    {
        return skillItems.Where(x => x.stringId == ID).FirstOrDefault();
    }

    public float GetSkillValueByName(string ID)
    {
        SkillItem temp = skillItems.Where(x => x.stringId == ID).FirstOrDefault();
        return temp.lvlValues[temp.currentValue];
    }

#if UNITY_EDITOR

    [MenuItem("Tools/ClearPP")]
    public static void ClearPP()
    {
        PlayerPrefs.DeleteAll();
        ShopManager.Instance.shopItems.ForEach(x =>
        {
            x.isBuy = false; EditorUtility.SetDirty(x);
        });
        ShopManager.Instance.shopItems.ForEach(x => x.skills.ForEach(y => { y.currentValue = 1; EditorUtility.SetDirty(y); }));
        ShopManager.Instance.skillItems.ForEach(x =>
        {
            x.currentValue = 1;
            EditorUtility.SetDirty(x);
        });

        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Убрать Raycast c Text")]
    public static void RaycastOff()
    {
        for (int i = 0; i < CanvasManager.Instance.screens.Count; i++)
        {
            Text[] buttons = CanvasManager.Instance.screens[i].GetComponentsInChildren<Text>(true);
            foreach (Text txt in buttons)
            {
                txt.raycastTarget = false;
            }
        }
    }

#endif

    #endregion LoadFromCloud
}