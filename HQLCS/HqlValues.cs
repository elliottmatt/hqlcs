using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlValues : HqlRecord
    {
        public HqlValues(HqlFieldGroup fieldsImpacted, HqlRecord record)
        {
            _fieldsImpacted = fieldsImpacted;
            _values1 = _fieldsImpacted.GetValues(record);
        }

        public HqlValues(HqlFieldGroup fieldsImpacted, object[] values)
        {
            _fieldsImpacted = fieldsImpacted;
            _values1 = values;
        }

        public HqlValues(HqlFieldGroup fieldsImpacted, HqlValues values1, HqlValues values2)
        {
            _fieldsImpacted = fieldsImpacted;
            SetValues(values1, values2);
        }

        public override object GetValue(HqlToken t)
        {
            object o;
            if (HasValue(t, out o))
                return o;
            throw new Exception("Unknown type in Values");
        }

        public override object GetValue(HqlField field)
        {
            object o;
            if (HasValue(field, out o))
                return o;
            throw new Exception(String.Format("Attempted to retrieve |{0}| in Values", field.ToString()));
        }

        public bool HasValue(HqlToken t, out object o)
        {
            if (t.NeedsEvaluation)
                return HasValue(t.Field, out o);
            else
            {
                o = t.EvaluatedData;
                return true;
            }
        }

        public bool HasValue(HqlField field, out object o)
        {
            for (int i = 0; i < _fieldsImpacted.Count; ++i)
            {
                if (_fieldsImpacted[i].Equals(field))
                {
                    o = GetValue(i);
                    return true;
                }
            }
            
            o = null;
            return false;
        }

        public void IncreaseUsedCount()
        {
            _countUsed++;
        }

        private object[] Values
        {
            get
            {
                if (_values2 == null)
                    return _values1;
                else
                {
                    object[] _values = new object[_values1.Length + _values2.Length];
                    int counter = 0;

                    for (int i = 0; i < _values1.Length; ++i)
                        _values[counter + i] = _values1[i];
                    counter = _values1.Length;

                    for (int i = 0; i < _values2.Length; ++i)
                        _values[counter + i] = _values2[i];

                    return _values;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _fieldsImpacted.Count; ++i)
            {
                if (i > 0)
                    sb.Append(" ");
                sb.Append(String.Format("[{0} => |{1}|]", _fieldsImpacted[i].ToString(), GetValue(i).ToString()));
            }
            return sb.ToString();
        }

        private void SetValues(HqlValues values1, HqlValues values2)
        {
            _values1 = (values1 == null ? null : values1.Values);
            _values2 = (values2 == null ? null : values2.Values);
        }

        private object GetValue(int n)
        {
            if (n < _values1.Length)
            {
                return _values1[n];
            }
            return _values2[n - _values1.Length];
        }

        public HqlFieldGroup FieldsImpacted
        {
            get { return _fieldsImpacted; }
        }

        public int CountUsed
        {
            get { return _countUsed; }
            //set { _countUsed = value; }
        }

        public bool IsUsed
        {
            get { return (CountUsed > 0); }
        }

        public void Cleanup()
        {
            _fieldsImpacted.Cleanup();
            _values1 = null;
            _values2 = null;
        }

        int _countUsed;
        object[] _values1;
        object[] _values2;
        HqlFieldGroup _fieldsImpacted;
    }
}
