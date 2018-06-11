using UnityEngine;
using UnityEditor;
using System.IO;
 
public static class ScriptableObjectUtility
{
	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static T CreateAsset<T> (string path, string name) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();

		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/" + name + ".asset");
 
		AssetDatabase.CreateAsset (asset, assetPathAndName);
 
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();

		return asset;
	}
}