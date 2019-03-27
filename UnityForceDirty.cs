using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Syy.Tools
{
    public static class UnityForceDirty
    {
        [MenuItem("Tools/UnityForceDirty/SelectFolderOrAssets")]
        public static void ForceDirtyBySelectFolderOrAssets()
        {
            var targets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets);

            if (targets.Length == 0)
            {
                EditorUtility.DisplayDialog("UnityForceDirty", "Please select at least one", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Start UnityForceDirty?", "It can take several hours to complete, depending on the size of your project. Target is select folder or assets", "Start", "Cancel"))
            {
                return;
            }

            Impl(targets);
            EditorUtility.DisplayDialog("UnityForceDirty", "Complete!", "OK");
        }

        [MenuItem("Tools/UnityForceDirty/SelectComponent")]
        public static void ForceDirtyBySelectComponent()
        {
            var types = Selection.objects
                .Select(value => value as MonoScript)
                .Where(value => value != null)
                .Select(value => value.GetClass())
                .Where(value => value.IsSubclassOf(typeof(MonoBehaviour))).ToArray();

            if (types.Length == 0)
            {
                EditorUtility.DisplayDialog("UnityForceDirty", "Please select at least one component script", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Start UnityForceDirty?", "It can take several hours to complete, depending on the size of your project. Target is assets referencing the selected component", "Start", "Cancel"))
            {
                return;
            }

            var targets = new List<UnityEngine.Object>(256);
            var guids = AssetDatabase.FindAssets("t:GameObject");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assets = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (types.Any(type => assets.GetComponent(type) != null))
                {
                    targets.Add(assets);
                }
            }
            Impl(targets);
            EditorUtility.DisplayDialog("UnityForceDirty", "Complete!", "OK");
        }

        static void Impl(IEnumerable<UnityEngine.Object> targets)
        {
            var max = (float)targets.Count();
            var current = 1;
            foreach (var target in targets)
            {
                EditorUtility.DisplayProgressBar("UnityForceDirty", "SetDirty..." + current + "/" + max, current / max);
                EditorUtility.SetDirty(target);
                Debug.Log($"{target.GetType().Name} - {target.name}");
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
    }
}
