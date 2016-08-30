using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
/// <summary>
/// @modified by JiphuTzu on 2015/06/15
/// 	添加了部分查询功能int型、bool型和string型
/// 
/// @author JiphuTzu
/// @date 2015/06/01
/// Quiry.
/// </summary>
namespace Utzudb
{
    public class Quiry<T> : List<T>, Inquiry<T> where T : DataBaseVO
    {
        //public Quiry():base() {
        //}

        /// <summary>
        /// 转为List类型
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return this;
        }
        public List<T> ToList(IComparer<T> comparer)
        {
            Sort(comparer);
            //Sort((t1,t2) => { return 1; });
            return this;
        }
        public List<T> ToList(Comparison<T> comparison)
        {
            Sort(comparison);
            return this;
        }

        public Inquiry<T> Select(Comparutzu<T> comparison)
        {
            Quiry<T> quiry = new Quiry<T>();
            int len = Count;
            //Debug.Log ("Select----------->"+param);
            for (int i = 0; i < len; i++)
            {
                T vo = this[i];
                bool r = comparison(vo);
                //Debug.Log ("Select----------->"+vo+"   "+r);
                if (r == true)
                {
                    quiry.Add(vo);
                }
            }
            //Debug.Log ("Select----------->"+param+"   "+quiry.Count);
            return quiry;
        }
        /// <summary>
        /// 查找符合条件的值
        /// </summary>
        /// <param name="param">该属性的值只能为数字或者字符串或布尔值</param>
        /// <param name="op">Op.</param>
        /// <param name="value">Value.</param>
        public Inquiry<T> Select(string param, string op, object value)
        {
            Quiry<T> quiry = new Quiry<T>();
            int len = Count;
            //Debug.Log ("Select----------->"+param);
            for (int i = 0; i < len; i++)
            {
                T vo = this[i];
                bool r = Compare(vo, param, op, value);
                //Debug.Log ("Select----------->"+vo+"   "+r);
                if (r == true)
                {
                    quiry.Add(vo);
                }
            }
            //Debug.Log ("Select----------->"+param+"   "+quiry.Count);
            return quiry;
        }
        public T Find(Comparutzu<T> comparison)
        {
            int len = Count;
            for (int i = 0; i < len; i++)
            {
                T vo = this[i];
                if (comparison(vo) == true)
                {
                    return vo;
                }
            }
            return null;
        }
        /// <summary>
        /// 查找一个符合条件的值
        /// </summary>
        /// <param name="param"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
		public T Find(string param, string op, object value)
        {
            int len = Count;
            for (int i = 0; i < len; i++)
            {
                T vo = this[i];
                if (Compare(vo, param, op, value) == true)
                {
                    return vo;
                }
            }
            return null;
        }

        private bool Compare(T vo, string param, string op, object value)
        {
            Type type = vo.GetType();

            FieldInfo finfo = type.GetField(param);
            if (finfo != null)
            {
                if (CompareType(finfo.FieldType, finfo.GetValue(vo), op, ParseTrueValue(value, vo, type), vo) == false) return false;
                return true;
            }
            else
            {
                PropertyInfo pinfo = type.GetProperty(param);
                if (pinfo != null)
                {
                    if (CompareType(pinfo.PropertyType, pinfo.GetValue(vo, new object[0]), op, ParseTrueValue(value, vo, type), vo) == false) return false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private object ParseTrueValue(object v, object vo, Type type)
        {
            string s = v as string;
            if (s != null && s.StartsWith("@") == true)
            {
                s = s.Substring(1);
                FieldInfo finfo = type.GetField(s);
                if (finfo != null)
                {
                    v = finfo.GetValue(vo);
                }
                else
                {
                    PropertyInfo pinfo = type.GetProperty(s);
                    if (pinfo != null)
                    {
                        v = pinfo.GetValue(vo, new object[0]);
                    }
                    else
                    {
                        v = s;
                    }
                }
            }
            return v;
        }

        private bool CompareType(Type type, object va, string con, object vb, object vo)
        {
            if (type == typeof(int))
            {
                return CompareInt((int)va, con, (int)vb);
            }
            if (type == typeof(long))
            {
                return CompareLong((long)va, con, (long)vb);
            }
            if (type == typeof(float))
            {
                return CompareFloat((float)va, con, (float)vb);
            }
            if (type == typeof(bool))
            {
                return CompareBool((bool)va, con, (bool)vb);
            }
            if (type == typeof(string))
            {
                return CompareString((string)va, con, (string)vb);
            }
            return false;
        }
        private bool CompareString(string va, string con, string vb)
        {
            switch (con)
            {
                case "==":
                    return va == vb;
                case "!=":
                    return va != vb;
                default:
                    return false;
            }
        }
        private bool CompareBool(bool va, string con, bool vb)
        {
            switch (con)
            {
                case "==":
                    return va == vb;
                case "!=":
                    return va != vb;
                default:
                    return false;
            }
        }
        private bool CompareFloat(float va, string con, float vb)
        {
            //Debug.Log ("Condition " + va + " " + con + " " + vb);
            switch (con)
            {
                case ">":
                    return va > vb;
                case ">=":
                    return va >= vb;
                case "<":
                    return va < vb;
                case "<=":
                    return va <= vb;
                case "==":
                    return va == vb;
                case "!=":
                    return va != vb;
                default:
                    return false;
            }
        }
        private bool CompareLong(long va, string con, long vb)
        {
            //Debug.Log ("Condition " + va + " " + con + " " + vb);
            switch (con)
            {
                case ">":
                    return va > vb;
                case ">=":
                    return va >= vb;
                case "<":
                    return va < vb;
                case "<=":
                    return va <= vb;
                case "==":
                    return va == vb;
                case "!=":
                    return va != vb;
                case "&":
                    return (va & vb) > 0;
                case "&=":
                    return (va & vb) == 0;
                case "|":
                    return (va | vb) > 0;
                case "|=":
                    return (va | vb) == 0;
                default:
                    return false;
            }
        }
        private bool CompareInt(int va, string con, int vb)
        {
            //Debug.Log ("Condition " + va + " " + con + " " + vb);
            switch (con)
            {
                case ">":
                    return va > vb;
                case ">=":
                    return va >= vb;
                case "<":
                    return va < vb;
                case "<=":
                    return va <= vb;
                case "==":
                    return va == vb;
                case "!=":
                    return va != vb;
                case "&":
                    return (va & vb) > 0;
                case "&=":
                    return (va & vb) == 0;
                case "|":
                    return (va | vb) > 0;
                case "|=":
                    return (va | vb) == 0;
                default:
                    return false;
            }
        }
    }
}