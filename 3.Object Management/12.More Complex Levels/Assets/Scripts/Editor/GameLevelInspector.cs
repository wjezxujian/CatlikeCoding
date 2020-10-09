using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameLevel))]
public class GameLevelInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameLevel gameLevel = (GameLevel)target;
        if (gameLevel.HasMissingLevelObjects)
        {
            EditorGUILayout.HelpBox("Missing level objects!", MessageType.Error);
            if(GUILayout.Button("Remove Missing Elements"))
            {
                Undo.RecordObject(gameLevel, "Remove Missing Level Objects.");
                gameLevel.RemoveMissingLevelObjects();
            }
        }
    }
}
