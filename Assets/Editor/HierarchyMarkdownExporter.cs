using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System.Reflection;

public class HierarchyYamlExporter : EditorWindow
{
    [MenuItem("Tools/Export Hierarchy to YAML")]
    static void ExportHierarchy()
    {
        StringBuilder sb = new StringBuilder();

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            AppendGameObjectYAML(root, 0, sb);
        }

        string path = "Assets/HierarchyExport.yaml";
        File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("✅ Exported to " + path);
    }

    static void AppendGameObjectYAML(GameObject obj, int indent, StringBuilder sb)
    {
        string ind = new string(' ', indent * 2);
        sb.AppendLine($"{ind}- GameObject: {obj.name}");

        // Transform情報
        Transform tf = obj.transform;
        sb.AppendLine($"{ind}  Transform:");
        sb.AppendLine($"{ind}    Position: [{tf.localPosition.x:F2}, {tf.localPosition.y:F2}, {tf.localPosition.z:F2}]");
        sb.AppendLine($"{ind}    Rotation: [{tf.localEulerAngles.x:F2}, {tf.localEulerAngles.y:F2}, {tf.localEulerAngles.z:F2}]");
        sb.AppendLine($"{ind}    Scale:    [{tf.localScale.x:F2}, {tf.localScale.y:F2}, {tf.localScale.z:F2}]");

        // コンポーネント情報
        Component[] components = obj.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null) continue;

            System.Type type = comp.GetType();
            string typeName = type.Name;
            if (type == typeof(Transform)) continue;

            sb.AppendLine($"{ind}  Component: {typeName}");

            // 代表的な組み込み型の簡易プロパティ
            if (comp is Camera cam)
            {
                sb.AppendLine($"{ind}    FOV: {cam.fieldOfView}");
                sb.AppendLine($"{ind}    ClearFlags: {cam.clearFlags}");
            }
            else if (comp is Light light)
            {
                sb.AppendLine($"{ind}    Type: {light.type}");
                sb.AppendLine($"{ind}    Intensity: {light.intensity}");
            }
            else if (comp is Collider col)
            {
                sb.AppendLine($"{ind}    IsTrigger: {col.isTrigger}");
                sb.AppendLine($"{ind}    Enabled: {col.enabled}");
            }
            else if (comp is Rigidbody rb)
            {
                sb.AppendLine($"{ind}    Mass: {rb.mass}");
                sb.AppendLine($"{ind}    UseGravity: {rb.useGravity}");
            }

            // MonoBehaviour (Script) の publicフィールドを列挙
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

        // 子オブジェクトの再帰処理
        foreach (Transform child in tf)
        {
            AppendGameObjectYAML(child.gameObject, indent + 1, sb);
        }
    }
}
