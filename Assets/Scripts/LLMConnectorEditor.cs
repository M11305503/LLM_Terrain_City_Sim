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

        // 請求 LLM 參數包

        if (GUILayout.Button("請求 LLM 參數包"))
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
