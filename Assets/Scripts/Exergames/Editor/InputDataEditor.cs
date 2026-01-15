using Newtonsoft.Json;
using OVRSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputDataContainer))]
[CanEditMultipleObjects]
[InitializeOnLoad]
public class InputDataEditor : Editor
{
    private SerializedProperty _inputData;
    private SerializedProperty _parametersJson;
    private SerializedProperty _useType;
    private SerializedProperty _language;

    private bool _initCalled;

    void Awake()
    {
        _initCalled = false;

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    void OnDestroy()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnEnable()
    {
        _inputData = serializedObject.FindProperty("InputData");
        _parametersJson = serializedObject.FindProperty("Parameters");
        _useType = serializedObject.FindProperty("UseType");
        _language = serializedObject.FindProperty("Language");

        if (_parametersJson.stringValue == "") {
            UpdateParameters();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_inputData);
        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_parametersJson);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Update parameters")) {
            UpdateParameters();
        }

        EditorGUILayout.Separator();

        serializedObject.Update();
        EditorGUILayout.PropertyField(_language);
        if (EditorApplication.isPlaying) {
            EditorGUI.BeginDisabledGroup(true);
        }
        EditorGUILayout.PropertyField(_useType);
        if (EditorApplication.isPlaying) {
            EditorGUI.EndDisabledGroup();
        }
        serializedObject.ApplyModifiedProperties();

        InputParameters inputParameters = JsonConvert.DeserializeObject<InputParameters>(_inputData.stringValue);
        
        ShowInputParametersGUI(inputParameters);
        ShowInputButtonsGUI(inputParameters);
    }

    private void ShowInputParametersGUI(InputParameters inputParameters)
    {
        var properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(_parametersJson.stringValue);

        var categories = GetInputParameterCategories(inputParameters.inputParameterCategories, inputParameters.inputParameters);

        foreach (var inputParameterCategory in categories) {
            EditorGUILayout.Space();

            string categoryText = TranslationSystem.Translate(inputParameterCategory.text, (Language) _language.enumValueFlag);
            EditorGUILayout.LabelField(categoryText, EditorStyles.boldLabel);

            foreach (var inputParameter in inputParameters.inputParameters) {
                if (inputParameter.isHomeUseOnly == true &&
                    (ExergameUseType) _useType.enumValueFlag != ExergameUseType.HomeUse) {
                    continue;
                }

                var categoryName = GetCategoryName(inputParameter);

                if (categoryName != inputParameterCategory.name) {
                    continue;
                }

                if (EditorApplication.isPlaying == true &&
                    (_useType.enumValueFlag == (int) ExergameUseType.HomeUse ||
                    inputParameter.runtimeUpdate == null ||
                    inputParameter.runtimeUpdate == false))
                {

                    EditorGUI.BeginDisabledGroup(true);
                }

                if (inputParameter.type == "int") {

                    if (inputParameter.minValue == null || inputParameter.maxValue == null) {
                        string text = TranslationSystem.Translate(inputParameter.text, (Language)_language.enumValueFlag);
                        string finalText = TranslationSystem.Translate(inputParameter.finalText, (Language)_language.enumValueFlag);

                        string fieldText = text + (finalText?.Length > 0 ?
                            " (" + finalText + ")" : "");

                        properties[inputParameter.name] =
                            EditorGUILayout.DelayedIntField(fieldText,
                            int.Parse(properties[inputParameter.name])).ToString();
                    }

                    if (inputParameter.minValue != null && inputParameter.maxValue != null) {
                        string text = TranslationSystem.Translate(inputParameter.text, (Language)_language.enumValueFlag);
                        string finalText = TranslationSystem.Translate(inputParameter.finalText, (Language)_language.enumValueFlag);

                        string fieldText = text + (finalText?.Length > 0 ?
                            " (" + finalText + ")" : "");

                        properties[inputParameter.name] =
                            EditorGUILayout.IntSlider(
                                fieldText,
                                int.Parse(properties[inputParameter.name]),
                                int.Parse(inputParameter.minValue),
                                int.Parse(inputParameter.maxValue)
                            ).ToString();
                    }
                }

                if (inputParameter.type == "bool") {
                    properties[inputParameter.name] =
                        EditorGUILayout.Toggle(
                            TranslationSystem.Translate(inputParameter.text, (Language)_language.enumValueFlag),
                            bool.Parse(properties[inputParameter.name])
                        ).ToString();
                }

                if (inputParameter.type == "list") {
                    string[] optionsList = inputParameter.value.Select(
                        ip => TranslationSystem.Translate (ip.text, (Language)_language.enumValueFlag)
                    ).ToArray();

                    int prevSelectedOption = inputParameter.value.FindIndex(
                        ip => ip.value == properties[inputParameter.name]
                    );

                    int selectedOption =
                        EditorGUILayout.Popup(
                            TranslationSystem.Translate(inputParameter.text, (Language)_language.enumValueFlag),
                            prevSelectedOption,
                            optionsList
                        );

                    properties[inputParameter.name] =
                        inputParameter.value[selectedOption].value;
                }

                if (inputParameter.type == "ivec2") {

                    float minValue = float.Parse(properties[inputParameter.minName]);
                    float maxValue = float.Parse(properties[inputParameter.maxName]);

                    string text = TranslationSystem.Translate(inputParameter.text, (Language)_language.enumValueFlag);
                    string finalText = TranslationSystem.Translate(inputParameter.finalText, (Language)_language.enumValueFlag);

                    string fieldText = text + (finalText?.Length > 0 ?
                        " (" + finalText + ")" : "");

                    EditorGUILayout.MinMaxSlider(
                        fieldText,
                        ref minValue,
                        ref maxValue,
                        int.Parse(inputParameter.minValue),
                        int.Parse(inputParameter.maxValue)
                    );

                    properties[inputParameter.minName] = minValue.ToString();
                    properties[inputParameter.maxName] = maxValue.ToString();
                }

                if (EditorApplication.isPlaying == true &&
                    (_useType.enumValueFlag == (int)ExergameUseType.HomeUse ||
                    inputParameter.runtimeUpdate == null ||
                    inputParameter.runtimeUpdate == false))
                {

                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        properties["duration"] = "0";

        _parametersJson.stringValue = JsonConvert.SerializeObject(properties);
        serializedObject.ApplyModifiedProperties();

        OnInputUpdate(_parametersJson.stringValue);
    }

    private List<InputParameterCategory> GetInputParameterCategories(
        List<InputParameterCategory> inputParameterCategories,
        List<InputParameter> inputParameters)
    {
        List<InputParameterCategory> categories = new List<InputParameterCategory> ();
        HashSet<string> categoryNames = new HashSet<string> ();

        if (inputParameterCategories != null) {
            foreach(var inputParameterCategory in inputParameterCategories) {
                categories.Add(inputParameterCategory);
                categoryNames.Add(inputParameterCategory.name);
            }
        }

        foreach(var inputParameter in inputParameters) {
            var categoryName = GetCategoryName(inputParameter);

            if (categoryNames.Contains(categoryName)) {
                continue;
            }

            categoryNames.Add(categoryName);
            categories.Add(new InputParameterCategory() { 
                name = categoryName,
                text = TranslationSystem.CreateTranslatableText(categoryName)
            });
        }

        categories = categories.OrderBy(c =>
            c.name == "Implicit" ? 0 :
            c.name == "General" ? 1 : 2)
            .ThenBy(c => c.name)
            .ToList();

        return categories;
    }

    private string GetCategoryName(InputParameter inputParameter)
    {
        if (inputParameter.isImplicit == true) {
            return "Implicit";
        }

        if (inputParameter.categoryName == null) {
            return "General";
        }

        return inputParameter.categoryName;
    }

    private void ShowInputButtonsGUI(InputParameters inputParameters)
    {
        if (inputParameters.buttons == null) {
            return;
        }

        if (EditorApplication.isPlaying == false) {
            return;
        }

        if (_useType.enumValueFlag == (int)ExergameUseType.HomeUse) {
            return;
        }

        foreach (var button in inputParameters.buttons) {
            if (GUILayout.Button(button.text)) {
                OnCustomEvent(button.name);
            }
        }

        if (GUILayout.Button("Stop Session")) {
            OnStopSession();
        }
    }

    private void UpdateParameters()
    {
        InputParameters inputParameters = JsonConvert.DeserializeObject<InputParameters>(_inputData.stringValue);
        Dictionary<string, string> properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(_parametersJson.stringValue);

        if (properties == null) {
            properties = new Dictionary<string, string>();
        }

        foreach (var inputParameter in inputParameters.inputParameters) {

            if (inputParameter.type == "int") {
                properties[inputParameter.name] = inputParameter.defaultValue;
            }

            if (inputParameter.type == "bool") {
                properties[inputParameter.name] = inputParameter.defaultValue;
            }

            if (inputParameter.type == "list") {
                properties[inputParameter.name] = inputParameter.defaultValue;
            }

            if (inputParameter.type == "ivec2") {
                properties[inputParameter.minName] = inputParameter.minDefaultValue;
                properties[inputParameter.maxName] = inputParameter.maxDefaultValue;
            }
        }

        var needDeletion = new List<string>();

        foreach(var property in properties) {

            var exists = inputParameters.inputParameters.Any(
                ip => ip.name == property.Key
                    || ip.maxName == property.Key
                    || ip.minName == property.Key
            );

            if (exists == false) {
                needDeletion.Add(property.Key);
            }
        }

        foreach(var needDeletionElement in needDeletion) {
            properties.Remove(needDeletionElement);
        }

        properties["duration"] = "0";

        _parametersJson.stringValue = JsonConvert.SerializeObject(properties);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) {
            return;
        }

        _inputData = serializedObject.FindProperty("InputData");
        _parametersJson = serializedObject.FindProperty("Parameters");
        _useType = serializedObject.FindProperty("UseType");

        OnInit(_parametersJson.stringValue);
    }

    private void OnInit(string json)
    {
        if (EditorApplication.isPlaying == false) {
            return;
        }

        if (_initCalled == true) {
            return;
        }

        var sessionContainerGO = GameObject.Find("SessionContainer");

        if (sessionContainerGO == null) {
            Debug.LogError("Error!");
            return;
        }

        if ((ExergameUseType) _useType.enumValueFlag == ExergameUseType.ClinicalUse) {
            sessionContainerGO.SendMessage("OnInit", json);
        }

        if ((ExergameUseType) _useType.enumValueFlag == ExergameUseType.HomeUse) { 
            sessionContainerGO.SendMessage("OnInitHomeUse", json);
        }

        _initCalled = true;
    }

    private void OnCustomEvent(string eventName)
    {
        if (EditorApplication.isPlaying == false) {
            return;
        }

        if (_initCalled == false) {
            return;
        }

        var sessionContainerGO = GameObject.Find("SessionContainer");

        if (sessionContainerGO == null)
        {
            Debug.LogError("Error!");
            return;
        }

        sessionContainerGO.SendMessage("OnCustomEvent", eventName);
    }

    private void OnStopSession()
    {
        if (EditorApplication.isPlaying == false) {
            return;
        }

        if (_initCalled == false) {
            return;
        }

        var sessionContainerGO = GameObject.Find("SessionContainer");

        if (sessionContainerGO == null) {
            Debug.LogError("Error!");
            return;
        }

        sessionContainerGO.SendMessage("OnStop");
    }

    private void OnInputUpdate(string json)
    {
        if (EditorApplication.isPlaying == false) {
            return;
        }

        if (_initCalled == false) {
            return;
        }

        var sessionContainerGO = GameObject.Find("SessionContainer");

        if (sessionContainerGO == null) {
            Debug.LogError("Error!");
            return;
        }

        sessionContainerGO.SendMessage("OnUpdate", json);
    }
}

