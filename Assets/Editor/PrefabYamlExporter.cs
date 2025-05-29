using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Reflection;

public class PrefabYamlExporter
{
    [MenuItem("Tools/Export Selected Prefab to YAML")]
    static void ExportSelectedPrefab()
    {
        var selected = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(selected);
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefabRoot == null)
        {
            Debug.LogWarning("⚠️ 選択中のオブジェクトがプレハブではありません。");
            return;
        }

        StringBuilder sb = new StringBuilder();
        AppendGameObjectYAML(prefabRoot, 0, sb);

        string exportPath = $"Assets/PrefabExport_{prefabRoot.name}.yaml";
        File.WriteAllText(exportPath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"✅ Prefab exported to: {exportPath}");
    }

    static void AppendGameObjectYAML(GameObject obj, int indent, StringBuilder sb)
    {
        string ind = new string(' ', indent * 2);
        sb.AppendLine($"{ind}- GameObject: {obj.name}");

        Transform tf = obj.transform;
        sb.AppendLine($"{ind}  Transform:");
        sb.AppendLine($"{ind}    Position: [{tf.localPosition.x:F2}, {tf.localPosition.y:F2}, {tf.localPosition.z:F2}]");
        sb.AppendLine($"{ind}    Rotation: [{tf.localEulerAngles.x:F2}, {tf.localEulerAngles.y:F2}, {tf.localEulerAngles.z:F2}]");
        sb.AppendLine($"{ind}    Scale:    [{tf.localScale.x:F2}, {tf.localScale.y:F2}, {tf.localScale.z:F2}]");

        Component[] components = obj.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null || comp is Transform) continue;
            System.Type type = comp.GetType();
            sb.AppendLine($"{ind}  Component: {type.Name}");

            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (var field in fields)
                {
                    object value = field.GetValue(comp);
                    string valStr = value != null ? value.ToString() : "null";
                    sb.AppendLine($"{ind}    {field.Name}: {valStr}");
                }
            }
        }

        // 子要素の再帰処理（プレハブでも階層を保持）
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            AppendGameObjectYAML(obj.transform.GetChild(i).gameObject, indent + 1, sb);
        }
    }
}
