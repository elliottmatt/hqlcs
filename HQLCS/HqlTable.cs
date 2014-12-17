using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    //enum HqlTableType
    //{
    //    FIELD_TABLE,
    //    GROUPBY_TABLE,
    //}

    class HqlTable
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlTable(IComparer<HqlKey> comp)
        {
            //if (type != HqlTableType.GROUPBY_TABLE)
            //    throw new Exception("Initializer for GROUPBY_TABLE only");
            //_type = type;
            _comp = comp;
            _ht = new Dictionary<HqlKey, object>();
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public void Set(HqlKey key, HqlResultRow row)
        {
            _ht[key] = row;
        }

        //public bool HasField(string fieldname)
        //{
        //    return _ht.Contains(fieldname);
        //}

        public IDictionaryEnumerator GetEnumeratorForTable()
        {
            return _ht.GetEnumerator();
        }

        public IEnumerator GetSortedEnumeratorForTable()
        {
            return _keys.GetEnumerator();
        }

        public HqlResultRow Find(HqlKey key)
        {
            if (_ht.Contains(key))
                return (HqlResultRow)_ht[key];
            return null;
        }

        public void Sort()
        {
//#if DEBUG
//            if (_type != HqlTableType.GROUPBY_TABLE)
//                throw new Exception("Cannot SORT unless it is a GROUPBY");
//#endif
            Dictionary<HqlKey, object> tb = (Dictionary<HqlKey, object>)_ht; 
            _keys = new HqlKey[tb.Keys.Count];
            tb.Keys.CopyTo(_keys, 0);
            Array.Sort<HqlKey>(_keys, _comp);
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        //public HqlTableType TableType
        //{
        //    get { return _type; }
        //}

        public object this[object key]
        {
            get { return _ht[key]; }
            //set { _ht[key] = value; }
        }
        
        ///////////////////////
        // Getters/Setters

        ///////////////////////
        // Variables

        IComparer<HqlKey> _comp;
        //HqlTableType _type;
        System.Collections.IDictionary _ht;
        HqlKey[] _keys;
    }
}
