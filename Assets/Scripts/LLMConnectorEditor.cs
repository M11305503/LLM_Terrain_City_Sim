using UnityEditor;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

[CustomEditor(typeof(LLMConnector))]
public class LLMConnectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LLMConnector llmcon = (LLMConnector)target;

        GUILayout.Space(10);

        // 叫― LLM 把计

        if (GUILayout.Button("叫― LLM 把计"))
        {
            llmcon.RequestLLMParams();

            EditorUtility.SetDirty(target);
            MarkSceneDirty();
        }
    }

    private void MarkSceneDirty()
    {
        if (!UnityEngine.Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
