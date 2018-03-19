using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//============================================================
//@author	JiphuTzu
//@create	07/28/2017
//@company	FaymAR
//
//@description:
//============================================================
namespace Utzu
{
    [CustomPropertyDrawer(typeof(CustomLabelAttribute))]
    public class CustomLabelDrawer : PropertyDrawer
    {
        private static GUIContent _content;
        private GUIContent content
        {
            get
            {
                if (_content == null) _content = new GUIContent();
                return _content;
            }
        }
        private static GUIStyle _style;
        private GUIStyle style
        {
            get
            {
                if (_style == null)
                {
                    _style = EditorStyles.textArea;
                }
                return _style;
            }
        }
        private const string _RANGE = "Range";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Debug.Log(">>>>>>>"+property.name);
            //check if the propery we want to draw should be enabled
            if (!GetConditionResult(property)) return;
            CustomLabelAttribute attribute = (CustomLabelAttribute)base.attribute;
            //Debug.Log(attribute.label + "::" + property.propertyType + " == " + attribute.cdt);
            if (!string.IsNullOrEmpty(attribute.label)) label.text = attribute.label;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    if (attribute.min != 0 || attribute.max != 0)
                        EditorGUI.Slider(position, property, attribute.min, attribute.max, label);
                    else
                        EditorGUI.PropertyField(position, property, label, true);
                    break;
                case SerializedPropertyType.Integer:
                    if (attribute.min != 0 || attribute.max != 0)
                        EditorGUI.IntSlider(position, property, (int)attribute.min, (int)attribute.max, label);
                    else
                        EditorGUI.PropertyField(position, property, label, true);
                    break;
                case SerializedPropertyType.Generic:
                    if (property.type == _RANGE)
                        DrawRange(position, property, label, attribute);
                    else
                        EditorGUI.PropertyField(position, property, label, true);
                    break;
                case SerializedPropertyType.String:
                    if (attribute.cdt == CustomDrawerType.TextArea)
                        DrawTextArea(position, property, label);
                    else
                        EditorGUI.PropertyField(position, property, label, true);
                    break;
                case SerializedPropertyType.Enum:
                    if (attribute.cdt == CustomDrawerType.MultiEnum)
                        DrawMultiEnum(position, property, label);
                    else
                        EditorGUI.PropertyField(position, property, label, true);
                    break;
                default:
                    EditorGUI.PropertyField(position, property, label, true);
                    break;
            }
        }
        private void DrawMultiEnum(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            int value = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
            if (EditorGUI.EndChangeCheck())
                property.intValue = value;
        }
        private void DrawTextArea(Rect position, SerializedProperty property, GUIContent label)
        {
            //EditorGUI.BeginProperty(position, label, property);
            //position.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(position, label);
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;
            //position.width = EditorGUIUtility.fieldWidth;//-position.width-20;
            EditorGUI.BeginChangeCheck();
            string info = EditorGUI.TextArea(position, property.stringValue, style);
            if(EditorGUI.EndChangeCheck())
                property.stringValue = info;
            //EditorGUI.EndProperty();
            //EditorGUI.TextArea(position,label)
        }
        private void DrawRange(Rect position, SerializedProperty property, GUIContent label, CustomLabelAttribute attribute)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            float min = property.FindPropertyRelative("min").floatValue;
            float max = property.FindPropertyRelative("max").floatValue;
            EditorGUI.MinMaxSlider(position, label, ref min, ref max, attribute.min, attribute.max);
            //
            position.width -= EditorGUIUtility.labelWidth + 20;
            position.width /= 2;
            position.x += EditorGUIUtility.labelWidth;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginChangeCheck();
            min = EditorGUI.FloatField(position, min);
            if (EditorGUI.EndChangeCheck())
                property.FindPropertyRelative("min").floatValue = min;
            position.x += position.width + 20;
            EditorGUI.BeginChangeCheck();
            max = EditorGUI.FloatField(position, max);
            if (EditorGUI.EndChangeCheck())
                property.FindPropertyRelative("max").floatValue = max;
            position.x -= 15;
            EditorGUI.LabelField(position, "~");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //The property is not being drawn. We want to undo the spacing added before and after the property
            if (!GetConditionResult(property)) return -EditorGUIUtility.standardVerticalSpacing;
            // range property need twice space
            if (property.type == _RANGE) return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            //text area，auto height
            if (property.propertyType == SerializedPropertyType.String)
            {
                CustomLabelAttribute attribute = (CustomLabelAttribute)base.attribute;
                if (attribute.cdt == CustomDrawerType.TextArea)
                {
                    content.text = property.stringValue;
                    return style.CalcHeight(_content, EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 30);
                }
            }
            return EditorGUI.GetPropertyHeight(property, label);
        }

        private bool GetConditionResult(SerializedProperty property)
        {
            CustomLabelAttribute attribute = (CustomLabelAttribute)base.attribute;
            if (attribute.conditionFields == null || attribute.conditionFields.Length == 0) return true;
            for (int i = 0; i < attribute.conditionFields.Length; i++)
            {
                object value = (attribute.conditionValues != null && attribute.conditionValues.Length > i) ? attribute.conditionValues[i] : null;
                string cooperator = (attribute.cooperators != null && attribute.cooperators.Length > i) ? attribute.cooperators[i] : null;
                if (!GetconditionResult(property, attribute.conditionFields[i], value, cooperator)) return false;
            }
            return true;
        }
        private bool GetconditionResult(SerializedProperty property, string field, object value, string cooperator)
        {
            if (string.IsNullOrEmpty(field)) return true;
            //Look for the sourcefield within the object that the property belongs to
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(field);
            if (sourcePropertyValue == null)
            {
                Debug.LogWarning("Attempting to use a CustomLabelAttribute but no matching conditionField found in object: " + field);
                return true;
            }
            //Debug.Log(">>>" + sourcePropertyValue.propertyType);
            switch (sourcePropertyValue.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return Match(sourcePropertyValue.boolValue, GetBool(value), cooperator);
                case SerializedPropertyType.Float:
                    return Match(sourcePropertyValue.floatValue, GetFloat(value), cooperator);
                case SerializedPropertyType.Integer:
                    return Match(sourcePropertyValue.intValue, GetInt(value), cooperator);
                case SerializedPropertyType.String:
                    return Match(sourcePropertyValue.stringValue, GetString(value), cooperator);
                case SerializedPropertyType.ObjectReference:
                    return Match(sourcePropertyValue.objectReferenceValue, (UnityEngine.Object)value, cooperator);
                case SerializedPropertyType.Enum:
                    return Match(sourcePropertyValue.intValue, GetInt(value),
                                cooperator == null ? "=" : cooperator);
                default:
                    return true;
            }
        }
        private bool GetBool(object value)
        {
            if (value == null) return true;
            return Convert.ToBoolean(value);
        }
        private float GetFloat(object value)
        {
            if (value == null) return 0;
            return Convert.ToSingle(value);
        }
        private int GetInt(object value)
        {
            if (value == null) return 0;
            return Convert.ToInt32(value);
        }
        private string GetString(object value)
        {
            if (value == null) return string.Empty;
            return value.ToString();
        }
        private bool Match(bool value1, bool value2, string cooperator)
        {
            switch (cooperator)
            {
                case "!=":
                    return value1 != value2;
                //case "=":
                //return value1==value2;
                default:
                    return value1 == value2;
            }
        }
        ///"=","!=",">",">=","<","<="
        private bool Match(float value1, float value2, string cooperator)
        {
            switch (cooperator)
            {
                case "=":
                    return value1 == value2;
                case ">":
                    return value1 > value2;
                case ">=":
                    return value1 >= value2;
                case "<":
                    return value1 < value2;
                case "<=":
                    return value1 <= value2;
                //case "!=":
                //return value1 != value2;
                default:
                    return value1 != value2;
            }
        }
        private bool Match(int value1, int value2, string cooperator)
        {
            switch (cooperator)
            {
                case "=":
                    return value1 == value2;
                case ">":
                    return value1 > value2;
                case ">=":
                    return value1 >= value2;
                case "<":
                    return value1 < value2;
                case "<=":
                    return value1 <= value2;
                case "&":
                    return (value1 & value2) == value1;
                case "&=":
                    //Debug.Log(value1+" & "+value2+" >> "+(value1&value2));
                    return (value1 & value2) == value2;
                case "|":
                    return (value1 | value2) > 0;
                //case "!=":
                //return value1 != value2;
                default:
                    return value1 != value2;
            }
        }
        private bool Match(string value1, string value2, string cooperator)
        {
            switch (cooperator)
            {
                case "=":
                    return string.Equals(value1, value2);
                //case "!=":
                //return !string.Equals(value1, value2);
                default:
                    return !string.Equals(value1, value2);
            }
        }
        private bool Match(UnityEngine.Object value1, UnityEngine.Object value2, string cooperator)
        {
            switch (cooperator)
            {
                case "=":
                    return value1 == value2;
                default:
                    return value1 != value2;
            }
        }
    }
}