using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EndZone))]
public class EndZoneEditorInspector : Editor {
    SerializedProperty state;

    SerializedProperty radius;

    SerializedProperty time;

    private void OnEnable() {
        state = serializedObject.FindProperty("triggerState");
        radius = serializedObject.FindProperty("radius");
        time = serializedObject.FindProperty("timeToComplete");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EndZone endzone = (EndZone)target;

        EditorGUILayout.PropertyField(state);

        switch (endzone.triggerState) {
            case EndZone.EndConditions.RADIUS:
                EditorGUILayout.PropertyField(radius);
                break;
            case EndZone.EndConditions.FIRETRUCK:
                EditorGUILayout.PropertyField(time);
                break;
            case EndZone.EndConditions.TIMER_END:
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
