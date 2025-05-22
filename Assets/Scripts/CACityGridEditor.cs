using UnityEditor;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

[CustomEditor(typeof(CACityGrid))]
public class CACityGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CACityGrid caGrid = (CACityGrid)target;

        GUILayout.Space(10);

        // 初始化按鈕
        if (GUILayout.Button("🔄 重新初始化 City Grid"))
        {
            caGrid.InitCityTypeGrid();
            caGrid.SetInitialSeeds();

            // 自動觸發建築重建
            BuildingPlacer placer = FindObjectOfType<BuildingPlacer>();
            if (placer != null) placer.PlaceBuildings();

            EditorUtility.SetDirty(target);
            MarkSceneDirty();
        }

        // 細胞自動機演化按鈕
        if (GUILayout.Button("▶ 細胞自動機演化（Step）"))
        {
            caGrid.StepAutomaton();

            // 自動觸發建築重建
            BuildingPlacer placer = FindObjectOfType<BuildingPlacer>();
            if (placer != null) placer.PlaceBuildings();

            EditorUtility.SetDirty(target);
            MarkSceneDirty();
        }

        if (GUILayout.Button("自動演化10步"))
        {
            caGrid.AutoStep10();

            // 自動觸發建築重建
            BuildingPlacer placer = FindObjectOfType<BuildingPlacer>();
            if (placer != null) placer.PlaceBuildings();

            EditorUtility.SetDirty(target);
            MarkSceneDirty();
        }

        if (GUILayout.Button("用途統計"))
        {
            caGrid.LogCellTypeStats();

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
