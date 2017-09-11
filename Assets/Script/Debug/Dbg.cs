#if UNITY_EDITOR
using UnityEditor;
#endif

public class Dbg
{
#if UNITY_EDITOR
    [MenuItem("Tools/Clear Console %#c")]
#endif

    public static void ClearConsole()
    {
#if UNITY_EDITOR
        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
#endif
    }
}