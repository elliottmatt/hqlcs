using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlLine : HqlRecord, IDisposable
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlLine(string line, string delim, long rownum, string sourceName)
        {
            _line = line;
            _delim = delim;
            _rownum = rownum;
            _sourceName = sourceName;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public override object GetValue(HqlField field)
        {
            switch (field.FieldType)
            {
                case HqlFieldType.FIELDNUM:
                    return GetValue(field.Fieldnum);
                case HqlFieldType.FIXEDWIDTH:
                    return GetValue(field.Start, field.Length);
                case HqlFieldType.STAR:
                    return GetEntireLine();
                case HqlFieldType.FUNCTION:
                case HqlFieldType.SCALAR:
                    return field.GetValue(this);
                case HqlFieldType.LITERAL_INT:
                case HqlFieldType.LITERAL_FLOAT:
                case HqlFieldType.LITERAL_STRING:
                    return field.GetValue(this);
                case HqlFieldType.ROWNUM:
                    return _rownum;
                case HqlFieldType.FILENAME:
                    return _sourceName;
                default:
                    throw new Exception("Unknown HqlField type");
            }
        }

        public void Cleanup()
        {
            if (_fields != null)
            {
                for (int i = 0; i < _fields.Length; ++i)
                {
                    _fields[i] = null;
                }
                _fields = null;
            }
        }
        
        ///////////////////////
        // Private

        private void Parse()
        {
            _fields = _line.Split(new string[] { _delim }, StringSplitOptions.None);
        }

        private string GetEntireLine()
        {
            return _line;
        }

        private string GetValue(int fieldnum)
        {
            if (fieldnum >= FieldCount)
            {
                // TODO, this is an error due to that field not existing
                return String.Empty;
            }

            return this[fieldnum];
        }

        private string GetValue(int start, int length)
        {
            if (_line.Length < start)
                return String.Empty;
            else if (_line.Length < start + length)
                return _line.Substring(start);
            else
                return _line.Substring(start, length);
        }

        void IDisposable.Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        public string this[int n]
        {
            get
            {
                if (_fields == null) { Parse(); }
                return _fields[n];
            }
        }

        public int FieldCount
        {
            get
            {
                if (_fields == null) { Parse(); }
                return _fields.Length;
            }
        }

        public bool IsBlank
        {
            get
            {
                return (_line.Length == 0);
            }
        }

        public string Line
        {
            get { return _line; }
        }

        ///////////////////////
        // Variables

        long _rownum;
        string _sourceName;
        string _line;
        string[] _fields;
        string _delim;
    }
}
