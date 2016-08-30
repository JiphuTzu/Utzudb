using UnityEngine;
using System.Collections.Generic;
namespace Utzudb
{
    /**
	 * @author JiphuTzu
	 */
    public class DataBaseVO
    {
        private string _id;
        private string _name;

        public virtual string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public DataBaseVO(string id)
        {
            _id = id;
        }

        public virtual void FromDictionary(Dictionary<string, object> data)
        {
            if (data.ContainsKey("name")) _name = (string)data["name"];
        }
        public virtual Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("id", _id);
            data.Add("name", name);
            return data;
        }
    }
}