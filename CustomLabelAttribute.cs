using UnityEngine;
//============================================================
//@author	JiphuTzu
//@create	07/28/2017
//@company	FaymAR
//
//@description:
//============================================================
[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property |
    System.AttributeTargets.Class | System.AttributeTargets.Struct)]
public class CustomLabelAttribute : PropertyAttribute
{
    public string label;
    //The name of the field that will be in control
    public string[] conditionFields;
    public object[] conditionValues;
    public string[] cooperators;
    public float min = 0;
    public float max = 0;
    public CustomDrawerType cdt = CustomDrawerType.Default;
    public CustomLabelAttribute(string label)
    {
        this.label = label;
    }

    public CustomLabelAttribute(string label, string conditionField) : this(label)
    {
        this.conditionFields = new string[]{conditionField};
    }
    public CustomLabelAttribute(string label, string[] conditionFields) : this(label)
    {
        this.conditionFields = conditionFields;
    }

    public CustomLabelAttribute(string label, string conditionField, object conditionValue) : this(label, conditionField)
    {
        this.conditionValues = new object[]{conditionValue};
    }
    // public CustomLabelAttribute(string label, string[] conditionFields, object[] conditionValues) : this(label, conditionFields)
    // {
    //     this.conditionValues = conditionValues;
    // }
    ///
    ///cooperator "=","!=",">",">=","<","<="
    ///
    public CustomLabelAttribute(string label, string conditionField, object conditionValue, string cooperator) : this(label, conditionField, conditionValue)
    {
        this.cooperators = new string[]{cooperator};
    }
    public CustomLabelAttribute(string label, string[] conditionFields, object[] conditionValues, string[] cooperators) : this(label, conditionFields)
    {
        this.conditionValues = conditionValues;
        this.cooperators = cooperators;
    }
    /// for enum only
    public CustomLabelAttribute(string label, CustomDrawerType type) : this(label)
    {
        this.cdt = type;
    }
    /// for enum only
    public CustomLabelAttribute(string label, CustomDrawerType type, string conditionField) : this(label, type)
    {
        this.conditionFields = new string[]{conditionField};
    }
    public CustomLabelAttribute(string label, CustomDrawerType type, string[] conditionFields) : this(label, type)
    {
        this.conditionFields = conditionFields;
    }
    /// for enum only
    public CustomLabelAttribute(string label, CustomDrawerType type, string conditionField, object conditionValue) : this(label, type, conditionField)
    {
        this.conditionValues = new object[]{conditionValue};
    }
    // public CustomLabelAttribute(string label, CustomDrawerType type, string[] conditionFields, object[] conditionValues) : this(label, type, conditionFields)
    // {
    //     this.conditionValues = conditionValues;
    // }
    /// for enum only
    public CustomLabelAttribute(string label, CustomDrawerType type, string conditionField, object conditionValue, string cooperator) : this(label, type, conditionField, conditionValue)
    {
        this.cooperators = new string[]{cooperator};
    }
    public CustomLabelAttribute(string label, CustomDrawerType type, string[] conditionFields, object[] conditionValues, string[] cooperators) : this(label, type, conditionFields)
    {
        this.conditionValues = conditionValues;
        this.cooperators = cooperators;
    }
    //for float and int only
    public CustomLabelAttribute(string label, float min, float max) : this(label)
    {
        this.min = Mathf.Min(min, max);
        this.max = Mathf.Max(min, max);
    }
    //for float and int only
    public CustomLabelAttribute(string label, float min, float max, string conditionField) : this(label, min, max)
    {
        this.conditionFields = new string[]{conditionField};
    }
    public CustomLabelAttribute(string label, float min, float max, string[] conditionFields) : this(label, min, max)
    {
        this.conditionFields = conditionFields;
    }
    //for float and int only
    public CustomLabelAttribute(string label, float min, float max, string conditionField, object conditionValue) : this(label, min, max, conditionField)
    {
        this.conditionValues = new object[]{conditionValue};
    }
    // public CustomLabelAttribute(string label, float min, float max, string[] conditionFields, object[] conditionValues) : this(label, min, max, conditionFields)
    // {
    //     this.conditionValues = conditionValues;
    // }
    //for float and int only
    public CustomLabelAttribute(string label, float min, float max, string conditionField, object conditionValue, string cooperator) : this(label, min, max, conditionField, conditionValue)
    {
        this.cooperators = new string[]{cooperator};
    }
    public CustomLabelAttribute(string label, float min, float max, string[] conditionFields, object[] conditionValues, string[] cooperators) : this(label, min, max, conditionFields)
    {
        this.conditionValues = conditionValues;
        this.cooperators = cooperators;
    }
}
public enum CustomDrawerType{
    Default,
    MultiEnum,
    TextArea
}
[System.Serializable]
public struct Range
{
    [CustomLabel("最小值")]
    public float min;
    [CustomLabel("最大值")]
    public float max;
    public float length { get { return max - min; } }
    public float value { get { return Random.Range(min, max); } }
    public float GetValue(float t)
    {
        return Mathf.Lerp(min, max, t);
    }
    public bool Contains(float value)
    {
        return value>=min&&value<=max;
        //return Mathf.Clamp(value, min, max) == value;
    }
    private const string _RangeFormat = "[Range({0:F2}~{1:F2})]";
    public override string ToString(){
        return string.Format(_RangeFormat,min,max);
    }
    public static float operator *(Range r, float t)
    {
        //return Mathf.Lerp(r.min, r.max, t);
        return Mathf.LerpUnclamped(r.min, r.max, t);
    }
}