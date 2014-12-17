using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlKey
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlKey(object[] o)
        {
            _o = o;
            _hash = null;
        }

        ///////////////////////
        // Overridden functions

        public override int GetHashCode()
        {
            if (_hash != null && _hash != 0)
                return _hash.Value;

            int h1 = HqlUniqueHash.CalculateHashCode(this);
            _hash = h1;

            return _hash.Value;
        }
       
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _o.Length; ++i)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(_o[i].ToString());
            }
            return sb.ToString();
        }

        public string ToHashString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _o.Length; ++i)
            {
                if (i > 0)
                    sb.Append("\x01");
                sb.Append((_o[i] == null) ? "null" : _o[i].ToString());
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is HqlKey)
            {
                return Equals((HqlKey)obj);
            }
            return false;
        }

        ///////////////////////
        // Public 

        public bool Equals(HqlKey key)
        {
            if (_o.Length != key._o.Length)
                return false;
        
            if (_hash.HasValue && key._hash.HasValue)
            {
                if (GetHashCode() != key.GetHashCode())
                    return false;
            }

            for (int i = 0; i < _o.Length; ++i)
            {
                if (_o[i] == null)
                {
                    if (key._o[i] != null)
                        return false;
                }
                else if (key._o[i] == null)
                {
                    return false;
                }
                else if (!_o[i].Equals(key._o[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public object GetValue(int n)
        {
            object o = this[n];

            if (o is HqlCalc)
            {
                HqlCalc c = (HqlCalc)o;
                return c.GetValue();
            }
            else if (o is string)
            {
                string s = (string)o;
                return s;
            }

            throw new Exception(String.Format("Unknown GetValue type of |{0}|", o.GetType().ToString()));
        }

        public int CompareTo(HqlKey bb, int n)
        {
            if (_o[n] == null && bb._o[n] == null)
                return 0;
            else if (_o[n] == null)
                return -1;
            else if (bb._o[n] == null)
                return 1;

            HqlCompareToken.MakeAppropriateObjects(ref _o[n], ref bb._o[n]);

            if (_o[n] is string && bb._o[n] is string)
                return String.Compare((string)_o[n], (string)bb._o[n], StringComparison.CurrentCulture);
            if (_o[n] is Int64 && bb._o[n] is Int64)
                return ((Int64)_o[n]).CompareTo((Int64)bb._o[n]);
            if (_o[n] is int && bb._o[n] is int)
                return ((int)_o[n]).CompareTo((int)bb._o[n]);
            if (_o[n] is decimal && bb._o[n] is decimal)
                return ((decimal)_o[n]).CompareTo((decimal)bb._o[n]);
            return String.Compare(_o[n].ToString(), bb._o[n].ToString(), StringComparison.CurrentCulture);
        }
        
        ///////////////////////
        // Private


        ///////////////////////
        // Fields

        public object this[int n]
        {
            get { return _o[n]; }
            set
            {
                _o[n] = value;
                //_hash = 0;
            }
        }

        ///////////////////////
        // Getters/Setters       

        ///////////////////////
        // Variables

        private object[] _o;
        private int? _hash;
    }
}
