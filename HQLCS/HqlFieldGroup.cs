using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlFieldGroup
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors
        
        public HqlFieldGroup() { }

        public HqlFieldGroup(HqlFieldGroup a, HqlFieldGroup b)
        {
            AddField(a);
            AddField(b);
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public void AddField(HqlField field)
        {
            if (_fields == null)
            {
                _fields = new HqlField[1];
                _fields[0] = field;
            }
            else
            {
                HqlField[] newfields = new HqlField[_fields.Length + 1];
                for (int i = 0; i < _fields.Length; ++i)
                    newfields[i] = _fields[i];
                newfields[_fields.Length] = field;
                _fields = newfields;
            }
        }

        public void AddField(HqlFieldGroup group)
        {
            for (int i = 0; i < group.Count; ++i)
            {
                AddField(group[i]);
            }
        }

        public object[] GetValues(HqlRecord record)
        {
            if (_fields == null || _fields.Length == 0)
                return new object[0];

            object[] arr = new object[_fields.Length];
            for (int i = 0; i < _fields.Length; ++i)
            {
                arr[i] = _fields[i].GetValue(record);
            }
            return arr;
        }

        public bool ContainsField(HqlField field)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (field.Equals(this[i]))
                    return true;
            }
            return false;
        }

        public void SetPreviousDirection(HqlKeyword direction)
        {
            if (_fields == null || _fields.Length == 0)
                throw new Exception("No fields saved to set direction");
            if (direction != HqlKeyword.DESCENDING && direction != HqlKeyword.ASCENDING)
                throw new Exception("Direction keyword not given");
            
            _fields[_fields.Length - 1].Direction = direction;
        }

        public void SetPreviousFieldRename(string fieldRename)
        {
            if (_fields == null || _fields.Length == 0)
                throw new Exception("No fields saved to set field name");

            _fields[_fields.Length - 1].FieldRename = fieldRename;
        }

        public void Cleanup()
        {
            for (int i = 0; _fields != null && i < _fields[i].Length; ++i)
            {
                _fields[i].Cleanup();
            }
            _fields = null;
        }

        public void ClearFields()
        {
            Cleanup();
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        public int Count
        {
            get
            {
                if (_fields == null) return 0;
                return _fields.Length;
            }
        }
        
        public HqlField this[int i]
        {
            get { return _fields[i]; }
        }

        public HqlField[] Fields
        {
            get { return _fields; }
        }

        public bool HasFunctions
        {
            get
            {
                for (int i = 0; i < Count; ++i)
                {
                    if (_fields[i].HasFunction)
                        return true;
                }
                return false;
            }
        }

        ///////////////////////
        // Getters/Setters

        ///////////////////////
        // Variables

        private HqlField[] _fields;
    }
}
