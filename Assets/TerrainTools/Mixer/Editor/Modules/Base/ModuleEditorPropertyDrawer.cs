using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.TerrainTools.Mixer
{
    // see https://docs.unity3d.com/2020.1/Documentation/ScriptReference/PropertyDrawer.html
    [CustomPropertyDrawer(typeof(ModuleEditor))]
    public class ModuleEditorPropertyDrawer : PropertyDrawer
    {
        private static float LABEL_WIDTH = 80;
        private float DETAIL_PADDING = 40f;
        private float CONTROL_PADDING = 4f;

        private GUIContent customLabel = null;

        private float lineCount = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            lineCount = 1f;

            if (customLabel == null)
            {
                string name = property.FindPropertyRelative("name").stringValue;
                string description = property.FindPropertyRelative("description").stringValue;

                customLabel = EditorGUIUtility.TrTextContent( name, description);
            }

            SerializedProperty brushActive = property.FindPropertyRelative("active");
            SerializedProperty brushSize = property.FindPropertyRelative("brushSize");
            SerializedProperty brushStrength = property.FindPropertyRelative("brushStrength");

            Rect originalPosition = position;

            // we use a constant label width for now
            // float labelWidth = Mathf.Max(GUI.skin.label.CalcSize(customLabel).x) + 4.0f;
            float labelWidth = LABEL_WIDTH;

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, customLabel, property);

            // active
            float width;
            {
                width = 20;
                var activeRect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
                position.x += width;

                // Draw fields - pass GUIContent.none to each so they are drawn without labels
                EditorGUI.PropertyField(activeRect, brushActive, GUIContent.none);
            }

            // Draw label
            GUIStyle style = brushActive.boolValue ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), customLabel, style);
            position.x += labelWidth;


            if (brushActive.boolValue && brushSize != null && brushStrength != null)
            {
                float halfPanelWidth = (position.width - labelWidth - 20) / 2; // control space is split in half on the panel

                // size
                {
                    var sizeRect = new Rect(position.x, position.y, halfPanelWidth, EditorGUIUtility.singleLineHeight);

                    // simple slider; deactivated for now
                    /* 
                    //EditorGUI.PropertyField(sizeRect, brushSize, GUIContent.none);
                    EditorGUI.Slider(sizeRect, brushSize, 0, 300, GUIContent.none);
                    */

                    // using minmax slider
                    {
                        float textfieldSize = 30f;
                        Rect sliderControlRect = new Rect(sizeRect.x, sizeRect.y, halfPanelWidth - textfieldSize - CONTROL_PADDING, EditorGUIUtility.singleLineHeight);
                        Rect sliderTextfieldRect = new Rect(sizeRect.x + halfPanelWidth - textfieldSize, sliderControlRect.y, textfieldSize, EditorGUIUtility.singleLineHeight);

                        float minValue = 0f;
                        float maxValue = brushSize.floatValue;
                        float minLimit = 0f;
                        float maxLimit = 300f;

                        // show minmax slider
                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUI.MinMaxSlider(sliderControlRect, ref minValue, ref maxValue, minLimit, maxLimit);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            brushSize.floatValue = maxValue;
                        }

                        // show also a textfield
                        EditorGUI.PropertyField(sliderTextfieldRect, brushSize, GUIContent.none);
                    }
                }

                // strength
                {
                    position.x += halfPanelWidth;
                    position.x += CONTROL_PADDING;

                    var strengthRect = new Rect(position.x, position.y, halfPanelWidth, EditorGUIUtility.singleLineHeight);

                    //EditorGUI.PropertyField(strengthRect, brushStrength, GUIContent.none);
                    EditorGUI.Slider(strengthRect, brushStrength, 0, 100, GUIContent.none);
                }

                // additional properties, implemented in the subclasses
                {
                    // draw background
                    /*
                    DrawDetailBox(originalPosition);
                    */

                    AddDetailProperties(originalPosition, property);
                }
            }

            EditorGUI.EndProperty();

        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * lineCount;
        }

        public virtual void AddDetailProperties( Rect position, SerializedProperty property) 
        {

        }
        
        public void AddFloatField( Rect position, SerializedProperty floatProperty, float labelWidth, GUIContent label) {

            position.x += DETAIL_PADDING;
            position.y += EditorGUIUtility.singleLineHeight * lineCount;

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.x += labelWidth;

            float width = position.width - labelWidth - DETAIL_PADDING;

            Rect rect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, floatProperty, GUIContent.none);

            lineCount++;

        }

        public void AddSlider( Rect position, SerializedProperty floatProperty, float minValue, float maxValue, float labelWidth, GUIContent label) {

            position.x += DETAIL_PADDING;
            position.y += EditorGUIUtility.singleLineHeight * lineCount;

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.x += labelWidth;

            float width = position.width - labelWidth - DETAIL_PADDING + CONTROL_PADDING;

            Rect rect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Slider(rect, floatProperty, minValue, maxValue, GUIContent.none);

            lineCount++;

        }

        /*
        public virtual float GetDetailHeight()
        {
            return 0f;
        }

        public void DrawDetailBox( Rect position)
        {
            float height = GetDetailHeight();

            if (height == 0)
                return;

            Rect rect = new Rect( position);
            rect.x += DETAIL_PADDING;
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.width -= DETAIL_PADDING;
            rect.height = height;

            GUI.Box(rect, GUIContent.none, MixerStyles.detailPanelStyle);
        }
        */
    }
}
