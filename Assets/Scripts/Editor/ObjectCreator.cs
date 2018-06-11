using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ObjectCreator : MonoBehaviour {



    [MenuItem("Assets/Create/My Scriptable Object")]
    public static void CreateShopItem()
    {
        ShopItem asset = ScriptableObject.CreateInstance<ShopItem>();

        AssetDatabase.CreateAsset(asset, "Assets/shopItemNew.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
    [MenuItem("Assets/Create/Weapon")]
    public static void CreateShopItemWeapon()
    {
        ShopItemWeapon asset = ScriptableObject.CreateInstance<ShopItemWeapon>();

        AssetDatabase.CreateAsset(asset, "Assets/Prefabs/Weapons/shopItemWeaponNew.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
