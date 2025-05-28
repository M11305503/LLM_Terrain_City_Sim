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

        // �ШD LLM �Ѽƥ]

        if (GUILayout.Button("�ШD LLM �Ѽƥ]"))
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
