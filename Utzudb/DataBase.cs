using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// 数据库
/// 提供以下数据操作方法
/// 1.update
/// 2.add
/// 3.remove
/// 4.reset
/// 5.select
/// 6.getItemById
/// @author JiphuTzu
/// </summary>
namespace Utzudb
{

    public class DataBase<T> : Inquiry<T> where T : DataBaseVO
    {
        protected Quiry<T> map;
        private string _key;

        public DataBase(string key) : base()
        {
            _key = key;
            map = new Quiry<T>();
        }
        public T this[int index]
        {
            get
            {
                return map[index];
            }
        }

        /// <summary>
        /// 更新数据
        /// 如果只包含id，则remove指定id的数据
        /// 如果已包含id的数据，则reset数据
        /// 否则add数据
        /// </summary>
        /// <param name="">.</param>
        public T Update(Dictionary<string, object> data)
        {
            string id = GetDataId(data);
            if (string.IsNullOrEmpty(id) == true) return null;
            if (GetById(id) != null)
            {
                if (data == null)
                {
                    return Remove(id);
                }
                return Reset(id, data);
            }
            return Add(id, data);
        }

        /// <summary>
        /// 添加一条数据记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual T Add(string id, Dictionary<string, object> data)
        {
            //throw new Error("需要在子类中重写add函数");
            return null;
        }

        /// <summary>
        /// 删除一条数据记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T Remove(string id)
        {
            for (int i = 0; i < map.Count; i++)
            {
                DataBaseVO vo = map[i];
                if (vo.id == id)
                {
                    map.RemoveAt(i);
                    return vo as T;
                }
            }
            return null;
        }
        /// <summary>
        /// 重置一条数据记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
		public virtual T Reset(string id, Dictionary<string, object> data)
        {
            //throw new Error("需要在子类中重写reset函数" + id);
            return null;
        }

        /// <summary>
        ///查询
        /// </summary>
        /// <param name="param">属性名</param>
        /// <param name="op">判断条件</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public Inquiry<T> Select(string param, string op, object value)
        {
            return map.Select(param, op, value);
        }
        public Inquiry<T> Select(Comparutzu<T> comparison)
        {
            return map.Select(comparison);
        }
        public T Find(Comparutzu<T> comparison)
        {
            return map.Find(comparison);
        }
        //final 
        public T Find(string param, string op, object value)
        {
            return map.Find(param, op, value);
        }
        public string GetDataId(Dictionary<string, object> data)
        {
            if (data == null || data.ContainsKey(_key) == false) return "";
            string id = data[_key].ToString();
            data.Remove(_key);
            return id;
        }

        //final 
        public T GetById(string id)
        {
            if (string.IsNullOrEmpty(id) == true) return null;
            for (int i = 0; i < map.Count; i++)
            {
                T vo = map[i];
                if (vo.id == id) return vo;
            }
            return null;
        }

        /// <summary>
        /// 从本地读取数据
        /// </summary>
        public virtual void ReadFromLocal()
        {
        }

        /// <summary>
        /// 将数据写到本地
        /// </summary>
        public virtual void WriteToLocal()
        {
        }
        public List<T> ToList()
        {
            return map.ToList();
        }
        public List<T> ToList(IComparer<T> comparer)
        {
            return map.ToList(comparer);
        }
        public List<T> ToList(Comparison<T> comparison)
        {
            return map.ToList(comparison);
        }

        public int Count
        {
            get
            {
                return map.Count;
            }
        }
        protected List<U> Convert<U>(List<object> data)
        {
            List<U> res = new List<U>();
            for (int i = 0; i < data.Count; i++)
            {
                res.Add((U)data[i]);
            }
            return res;
        }
        protected Vector3 Convert(List<object> data)
        {
            if (data.Count != 3) return Vector3.zero;
            return new Vector3((float)(double)data[0], (float)(double)data[1], (float)(double)data[2]);
        }
    }
}