using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(GazeInteractable))]
public class GazeInteractableEditor : Editor
{
    protected SerializedProperty _isSelectable;
    protected SerializedProperty _onSelected;
    protected SerializedProperty _onHover;
    protected SerializedProperty _onUnhover;
    protected SerializedProperty _onOver;

    protected FieldInfo _onSelectedField;
    //protected FieldInfo _onHoverField;
    //protected FieldInfo _onUnhoverField;
    //protected FieldInfo _onUnhoverField;

    void OnEnable()
    {
        //_isSelectable = serializedObject.FindProperty("_isSelectable");
        _onSelected = serializedObject.FindProperty("_onSelected");
        _onHover = serializedObject.FindProperty("_onHover");
        _onUnhover = serializedObject.FindProperty("_onUnhover");
        _onOver = serializedObject.FindProperty("_onOver");

        // Use reflection to get the field info for the protected field
        _onSelectedField = typeof(GazeInteractable).GetField("_onSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        //_onHoverField = typeof(GazeInteractable).GetField("_onHover", BindingFlags.NonPublic | BindingFlags.Instance);
        //_onUnhoverField = typeof(GazeInteractable).GetField("_onUnhoverField", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //EditorGUILayout.PropertyField(_isSelectable);
        EditorGUILayout.PropertyField(_onSelected);
        EditorGUILayout.PropertyField(_onHover);
        EditorGUILayout.PropertyField(_onUnhover);
        EditorGUILayout.PropertyField(_onOver);
        serializedObject.ApplyModifiedProperties();
        
        if (EditorApplication.isPlaying) {
            if (GUILayout.Button("Stop Session")) {
                // Retrieve the actual GazeInteractable instance
                GazeInteractable gazeInteractable = (GazeInteractable)target;

                // Use reflection to get the value of the protected field
                UnityEvent onSelectedEvent = (UnityEvent)_onSelectedField.GetValue(gazeInteractable);

                // Invoke the UnityEvent
                onSelectedEvent.Invoke();
            }
        }
    }
}
