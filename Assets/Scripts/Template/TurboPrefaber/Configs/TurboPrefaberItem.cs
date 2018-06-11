using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

using Object = UnityEngine.Object;

#if UNITY_EDITOR

using UnityEditor;

#endif

[CreateAssetMenu(menuName = "Configs/TurboPrefaber item", fileName = "NewTurboPrefaberItem")]
public class TurboPrefaberItem : ScriptableObject
{
    [Serializable]
    public class ChildInfo
    {
        public Object prefab;
        public string nonPrefabName;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public string tag;
        public int layer;

        public ChildInfo[] children;
    }

    public ChildInfo root;

    private int loadedCount;
    private int loadTotal;

    public delegate bool FrameLoadCounter();

    public FrameLoadCounter LoadingLimiter;
    public Action<IEnumerator> LoadingHandler;
    public Action<int, int> LoadingProgress;

    public void Read(Transform rootTransform, bool monitorProgress = false, bool dontTranslateRoot = true)
    {
        if (rootTransform.childCount > 0)
        {
            /*
             *    return;
             */
            Debug.Log("<color=blue><i>Иерархия не пуста, распаковываю.</i></color>");
        }

        if (monitorProgress)
        {
            if (LoadingProgress != null)
                LoadingProgress(0, int.MaxValue);

            loadedCount = 0;
            loadTotal = CountRecursive(root);

            LoadingHandler(ReadNext(rootTransform, root, dontTranslateRoot));
        }
        else
        {
            ReadInternal(rootTransform, root, dontTranslateRoot);
        }
    }

    private void ReadInternal(Transform tr, ChildInfo ci, bool dontTranslate = false)
    {
        if (!dontTranslate)
        {
            tr.localPosition = ci.position;
            tr.localRotation = ci.rotation;
            tr.localScale = ci.scale;
        }

        tr.gameObject.layer = ci.layer;
        tr.tag = ci.tag;

        if (ci.prefab) return;

        tr.gameObject.name = ci.nonPrefabName;

        for (int i = 0; i < ci.children.Length; i++)
        {
            Transform ntr = null;
            if (!ci.children[i].prefab)
            {
                ntr = new GameObject().transform;
                ntr.SetParent(tr);
            }
            else
            {
#if UNITY_EDITOR
                ntr = (Transform)PrefabUtility.InstantiatePrefab(ci.children[i].prefab);
                ntr.SetParent(tr);
#else
				ntr = (Transform) Instantiate(ci.children[i].prefab, tr);
#endif
            }
            ReadInternal(ntr, ci.children[i]);
        }
    }

    private int CountRecursive(ChildInfo ci)
    {
        int sum = 0;

        if (ci.prefab)
            return 0;

        for (int i = 0; i < ci.children.Length; i++)
        {
            if (ci.children[i].prefab)
            {
                sum += 1;
            }
            sum += CountRecursive(ci.children[i]);
        }

        return sum;
    }

    private IEnumerator ReadNext(Transform tr, ChildInfo ci, bool dontTranslate = false)
    {
        if (!dontTranslate)
        {
            tr.localPosition = ci.position;
            tr.localRotation = ci.rotation;
            tr.localScale = ci.scale;
        }
        tr.gameObject.layer = ci.layer;
        tr.tag = ci.tag;

        if (ci.prefab) yield break;

        tr.gameObject.name = ci.nonPrefabName;

        for (int i = 0; i < ci.children.Length; i++)
        {
            Transform ntr = null;
            if (!ci.children[i].prefab)
            {
                ntr = new GameObject().transform;
                ntr.SetParent(tr);
            }
            else
            {
#if UNITY_EDITOR
                ntr = (Transform)PrefabUtility.InstantiatePrefab(ci.children[i].prefab);
                ntr.SetParent(tr);
#else
				ntr = (Transform) Instantiate(ci.children[i].prefab, tr);
#endif

                if (LoadingProgress != null)
                    LoadingProgress(++loadedCount, loadTotal);
            }

            IEnumerator recursive = ReadNext(ntr, ci.children[i]);
            LoadingHandler(recursive);
            if (LoadingLimiter == null || LoadingLimiter())
            {
                yield return null;
            }
        }
    }

#if UNITY_EDITOR

    public bool Write(Transform rootTransform, bool applyPrefabs)
    {
        var prefsList = applyPrefabs ? new List<GameObject>() : null;
        var res = WriteInternal(rootTransform, root, true, prefsList);

        if (applyPrefabs)
        {
            for (var i = 0; i < prefsList.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Применяем префабы...", (i + 1) + "/" + prefsList.Count, (float)i / (prefsList.Count - 1));
                PrefabUtility.ReplacePrefab(prefsList[i], PrefabUtility.GetPrefabParent(prefsList[i]), ReplacePrefabOptions.ConnectToPrefab);
            }

            EditorUtility.ClearProgressBar();
        }

        if (res) EditorUtility.SetDirty(this);
        return res;
    }

    private bool WriteInternal(Transform tr, ChildInfo ci, bool isRoot, List<GameObject> prefabsToApply = null)
    {
        ci.position = tr.localPosition;
        ci.rotation = tr.localRotation;
        ci.scale = tr.localScale;
        ci.tag = tr.tag;
        ci.layer = tr.gameObject.layer;

        if (!isRoot)
        {
            var error = false;
            var msg = "";

            switch (PrefabUtility.GetPrefabType(tr))
            {
                case PrefabType.PrefabInstance:
                    ci.prefab = PrefabUtility.GetPrefabParent(tr);

                    if (prefabsToApply != null)
                    {
                        var go = tr.gameObject;
                        if (!prefabsToApply.Any(x => PrefabUtility.GetPrefabParent(x) == PrefabUtility.GetPrefabParent(go)))
                            prefabsToApply.Add(go);
                    }

                    return true;

                case PrefabType.MissingPrefabInstance:
                    msg = "Для этого объекта отсутствует префаб";
                    error = true;
                    break;

                case PrefabType.DisconnectedPrefabInstance:
                    msg = "Broken префабы не поддерживаются";
                    error = true;
                    break;

                case PrefabType.ModelPrefabInstance:
                    msg = "Model префабы не поддерживаются";
                    error = true;
                    break;

                case PrefabType.DisconnectedModelPrefabInstance:
                    msg = "Model префабы не поддерживаются";
                    error = true;
                    break;
            }

            if (error)
            {
                EditorUtility.DisplayDialog("Ошибка!", msg + " ( GameObject: \"" + tr.gameObject.name + "\")", "Понятно", "");
                Selection.activeObject = tr.gameObject;

                return false;
            }
        }

        if (!isRoot && tr.gameObject.GetComponents<Component>().Any(x => !(x is Transform)))
        {
            EditorUtility.DisplayDialog("Ошибка!", "На НЕ ПРЕФАБАХ не должно быть никаких компонентов кроме трансформа ( GameObject: \""
                + tr.gameObject.name + "\")",
                "Понятно", "");

            Selection.activeObject = tr.gameObject;

            return false;
        }

        ci.nonPrefabName = tr.gameObject.name;
        ci.children = new ChildInfo[tr.childCount];

        for (int i = 0; i < tr.childCount; i++)
        {
            ci.children[i] = new ChildInfo();
            if (!WriteInternal(tr.GetChild(i), ci.children[i], false, prefabsToApply)) return false;
        }

        return true;
    }

#endif
}