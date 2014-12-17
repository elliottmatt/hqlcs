using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    public enum HqlFieldType
    {
        STAR,
        FIELDNUM,
        FIXEDWIDTH,
        LITERAL_STRING,
        FUNCTION,
        SCALAR,
        LITERAL_INT,
        LITERAL_FLOAT,
        ROWNUM,
        NULL,
        FILENAME,
    }

    class HqlField
    {
        ///////////////////////
        // Static Functions
        public static HqlField CreateObject(HqlToken token)
        {
            switch (token.WordType)
            {
                case HqlWordType.FUNCTION:
                case HqlWordType.FIELD:
                case HqlWordType.SCALAR:
                    return token.Field;
                case HqlWordType.FLOAT:
                    return new HqlField(HqlFieldType.LITERAL_FLOAT, (decimal)token.Parsed);
                case HqlWordType.INT:
                    return new HqlField(HqlFieldType.LITERAL_INT, (Int64)token.Parsed);
                case HqlWordType.LITERAL_STRING:
                    return new HqlField(HqlFieldType.LITERAL_STRING, (string)token.Parsed);
                default:
                    throw new Exception("Cannot create FIELD of token type");
            }
        }

        ///////////////////////
        // Constructors

        //private HqlField(HqlField field, bool hidden)
        //{
        //    Init(field._type);
        //    _start = field._start;
        //    _length = field._length;
        //    _name = field._name;
        //    _dec = field._dec;
        //    _func = field._func;
        //    _scalar = field._scalar;
        //    _direction = field._direction;
        //    _tableReference = field._tableReference;
        //    _fieldRename = field._fieldRename; 
        //}

        public HqlField(HqlFieldType type) // for STAR
        {
            if (type != HqlFieldType.STAR && type != HqlFieldType.ROWNUM && type != HqlFieldType.NULL && type != HqlFieldType.FILENAME)
                throw new Exception("Instance is for STAR only");
            Init(type);
        }
        public HqlField(HqlFieldType type, int fieldNum) // for FIELDNUM, decimal
        {
            if (type != HqlFieldType.FIELDNUM)
                throw new Exception("Instance is for FIELDNUM only");
            Init(type);
            _start = fieldNum;
        }
        public HqlField(HqlFieldType type, Int64 IntValue) // for INTVALUE, decimal
        {
            if (type != HqlFieldType.LITERAL_INT)
                throw new Exception("Instance is for IntValue only");
            Init(type);
            _start = IntValue;
        }
        public HqlField(HqlFieldType type, decimal decimalvalue) // for LITERAL_FLOAT
        {
            if (type != HqlFieldType.LITERAL_FLOAT)
                throw new Exception("Instance is for FLOAT only");
            Init(type);
            _dec = decimalvalue;
        }
        public HqlField(HqlFieldType type, int start, int length, string name) // for FIXEDWIDTH
        {
            if (type != HqlFieldType.FIXEDWIDTH)
                throw new Exception("Instance is for FIXEDWIDTH only");
            Init(type);
            _start = start;
            _length = length;
            _name = name;
        }
        public HqlField(HqlFieldType type, string literal) // for LITERAL
        {
            if (type != HqlFieldType.LITERAL_STRING)
                throw new Exception("Instance is for LITERAL only");
            Init(type);
            _name = literal;
        }
        public HqlField(HqlFieldType type, HqlFunction func) // for FUNCTION
        {
            if (type != HqlFieldType.FUNCTION)
                throw new Exception("Instance is for FUNCTION only");
            Init(type);
            _func = func;
        }
        public HqlField(HqlFieldType type, HqlScalar scalar) // for SCALAR
        {
            if (type != HqlFieldType.SCALAR)
                throw new Exception("Instance is for SCALAR only");
            Init(type);
            _scalar = scalar;
        }

        private void Init(HqlFieldType type)
        {
            _type = type;
            _direction = HqlKeyword.ASCENDING;
            _tableReference = String.Empty;
            _fieldRename = null;
            _start = -1;
            _length = -1;
            _printResult = true;
        }

        ///////////////////////
        // Overridden functions

        public override string ToString()
        {
            return GetFullName();
        }

        ///////////////////////
        // Public 

        public bool ContainsField(HqlField field)
        {
            if (this.Equals(field))
                return true;

            switch (this.FieldType)
            {
                case HqlFieldType.SCALAR:
                    return Scalar.ContainsField(field);
                case HqlFieldType.FUNCTION:
                    return Func.ContainsField(field);
            }
            return false;
        }
        public bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            if (this.FieldType == HqlFieldType.ROWNUM)
            {
                if (OnlyWithoutTableReference)
                {
                    if (TableReference.Length == 0)
                        return true;
                    else
                        return false;
                }

                return true;
            }

            switch (this.FieldType)
            {
                case HqlFieldType.SCALAR:
                    return Scalar.ContainsRownum(OnlyWithoutTableReference);
                case HqlFieldType.FUNCTION:
                    return Func.ContainsRownum(OnlyWithoutTableReference);
            }
            return false;
        }
                
        public bool Equals(HqlField field)
        {
            if (this.FieldType != field.FieldType)
            {
                // hack this is an optimization
                if (FieldType == HqlFieldType.LITERAL_STRING && field.FieldType == HqlFieldType.STAR && Name.Equals("*"))
                    return true;
                if (field.FieldType == HqlFieldType.LITERAL_STRING && this.FieldType == HqlFieldType.STAR && field.Name.Equals("*"))
                    return true;
                // end hack this is an optimization

                return false;
            }

            // this is safe because if neither have them, they will be both string.empty
            if (this.FieldType != HqlFieldType.SCALAR && this.FieldType != HqlFieldType.FUNCTION)
            {
                if (!field.TableReference.Equals(TableReference))
                    return false;
            }

            switch (_type)
            {
                case HqlFieldType.ROWNUM:
                case HqlFieldType.FILENAME:
                case HqlFieldType.STAR:
                    return true;
                case HqlFieldType.FIELDNUM:
                    return (Fieldnum == field.Fieldnum);
                case HqlFieldType.FIXEDWIDTH:
                    return (Name.Equals(field.Name) && Start == field.Start && Length == field.Length);
                case HqlFieldType.LITERAL_STRING:
                    return Name.Equals(field.Name);
                case HqlFieldType.FUNCTION:
                    return Func.Equals(field.Func);
                case HqlFieldType.SCALAR:
                    return Scalar.Equals(field.Scalar);
                case HqlFieldType.LITERAL_INT:
                    return this.IntValue == field.IntValue;
                default:
                    throw new Exception("Unknown type of Field");
            }

            return false;
        }

        public string GetFieldName()
        {
            string table = GetTableReferenceString();

            switch (_type)
            {
                case HqlFieldType.STAR:
                    return table + "*";
                case HqlFieldType.FIELDNUM:
                    return table + String.Format("field{0}", _start + 1);
                case HqlFieldType.FIXEDWIDTH:
                    return table + String.Format("{0}({1}, {2})", _name, _start + 1, _length);
                case HqlFieldType.LITERAL_STRING:
                    return String.Format("'{0}'", _name);
                case HqlFieldType.FUNCTION:
                    return Func.GetFieldName();
                case HqlFieldType.SCALAR:
                    return Scalar.GetFieldName();
                case HqlFieldType.LITERAL_INT:
                    return IntValue.ToString();
                case HqlFieldType.LITERAL_FLOAT:
                    return DecimalValue.ToString();
                case HqlFieldType.ROWNUM:
                    return table + "rownum";
                case HqlFieldType.FILENAME:
                    return table + "filename";
            }
            return "UNKNOWN_FIELD";
        }

        public string GetTableReferenceString()
        {
            if (HasTableReference)
                return TableReference + ".";
            return String.Empty;
        }

        public string GetFullName()
        {
            switch (_type)
            {
                case HqlFieldType.STAR:
                case HqlFieldType.FIELDNUM:
                case HqlFieldType.FIXEDWIDTH:
                case HqlFieldType.LITERAL_STRING:
                case HqlFieldType.LITERAL_INT:
                case HqlFieldType.LITERAL_FLOAT:
                case HqlFieldType.ROWNUM:
                case HqlFieldType.FILENAME:
                    return GetFieldName();
                case HqlFieldType.FUNCTION:
                    return Func.GetFullName();
                case HqlFieldType.SCALAR:
                    return Scalar.GetFullName();
                case HqlFieldType.NULL:
                    return "NULL";
            }
            return "UNKNOWN_FIELD";
        }

        public object GetValue(HqlRecord record)
        {
            switch (FieldType)
            {
                case HqlFieldType.FIELDNUM:
                case HqlFieldType.FIXEDWIDTH:
                case HqlFieldType.STAR:
                case HqlFieldType.ROWNUM:
                case HqlFieldType.FILENAME:
                    return record.GetValue(this);
                case HqlFieldType.SCALAR:
                    return Scalar.Evaluate(record);
                case HqlFieldType.FUNCTION:
                        return Func.GetValue(record);
                case HqlFieldType.LITERAL_STRING:
                    return Name;
                case HqlFieldType.LITERAL_INT:
                    return IntValue;
                case HqlFieldType.LITERAL_FLOAT:
                    return DecimalValue;
                default:
                    throw new Exception("Unknown HqlField type");
            }
        }

        public object GetFunctionValue(HqlRecord record)
        {
            switch (FieldType)
            {
                case HqlFieldType.FIELDNUM:
                case HqlFieldType.FIXEDWIDTH:
                case HqlFieldType.LITERAL_STRING:
                case HqlFieldType.STAR:
                case HqlFieldType.FUNCTION:
                case HqlFieldType.LITERAL_INT:
                case HqlFieldType.LITERAL_FLOAT:
                    return GetValue(record);
                case HqlFieldType.SCALAR:
                    {
                        // This works because this function is only called when HasFunction is true.
                        // If I'm in here and it goes to scalar, it REALLY wants the function value
                        // this is for the groupby
#if DEBUG
                        if (Func == null)
                            throw new Exception("Unexpected null function");
#endif
                        //return Func.GetFunctionValue(record);
                        return Func.GetValue(record);
                    }
                default:
                    throw new Exception("Unknown HqlField type");
            }
        }

        //public string EvaluateFunction(ArrayList list)
        //{
        //    HqlCalc calc = _func.CreateCalcObject();
        //    for (int i = 0; i < list.Count; ++i)
        //    {
        //        HqlTable ht = (HqlTable)list[i];
        //        calc.Add(ht[_func.GetFieldName()]);
        //    }
        //    return calc.GetPrintValue();
        //}

        public string GetHeaderValue()
        {
            if (HasFieldRename)
                return FieldRename;
            else
                return GetFullName();
        }

        public void Cleanup()
        {
            if (_func != null)
                _func.Cleanup();
            if (_scalar != null)
                _scalar.Cleanup();
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        public bool HasFunction
        {
            get
            {
                if (_type == HqlFieldType.FUNCTION)
                    return true;
                else if (_type == HqlFieldType.SCALAR)
                    return Scalar.HasFunction;
                return false;
            }
        }

        public bool HasScalar
        {
            get
            {
                if (_type == HqlFieldType.SCALAR)
                    return true;
                else if (_type == HqlFieldType.FUNCTION)
                    return Func.HasScalar;
                return false;
            }
        }

        public HqlField FinalField
        {
            get
            {
                HqlField f = this;
                for (; f.HasFunction; )
                    f = f.Func.Field;
                for (; f.HasScalar; )
                    f = f.Scalar.Field;

                return f;
            }
        }

        public bool IsLiteralType
        {
            get
            {
                switch (FieldType)
                {
                    case HqlFieldType.LITERAL_FLOAT:
                    case HqlFieldType.LITERAL_INT:
                    case HqlFieldType.LITERAL_STRING:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool PrintResult
        {
            get { return _printResult; }
            set { _printResult = value; }
        }

        public bool HasTableReference
        {
            get { return _tableReference.Length > 0; }
        }

        public bool HasFieldRename
        {
            get { return (_fieldRename != null); }
        }

        public bool ContainsTableReference(string tableReference)
        {
            if (_type == HqlFieldType.SCALAR)
                return Scalar.ContainsTableReference(tableReference);
            return tableReference.Equals(TableReference);
        }

        ///////////////////////
        // Getters/Setters

        public HqlFunction Func
        {
            get
            {
                if (_type == HqlFieldType.SCALAR)
                    return Scalar.Field.Func;

                return _func;
            }
        }

        public HqlScalar Scalar
        {
            get { return _scalar; }
        }

        public HqlFieldType FieldType
        {
            get { return _type; }
        }

        public int Fieldnum
        {
            get { return (int)(Int32)_start; }
        }

        public int Start
        {
            get { return (int)(Int32)_start; }
        }

        public int Length
        {
            get { return _length; }
        }

        public string Name
        {
            get { return _name; }
        }

        public decimal DecimalValue
        {
            get { return _dec; }
        }

        public Int64 IntValue
        {
            get { return _start; }
            //set { _start = value; }
        }

        internal HqlKeyword Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public string FieldRename
        {
            get { return _fieldRename; }
            set { _fieldRename = value; }
        }

        public string TableReference
        {
            get
            {
                if (_type == HqlFieldType.SCALAR)
                    return Scalar.Field.TableReference;

                return _tableReference;
            }
            set { _tableReference = value; }
        }

        ///////////////////////
        // Variables

        HqlFieldType _type;
        Int64 _start;
        int _length;
        string _name;
        decimal _dec;
        HqlFunction _func;
        HqlScalar _scalar;
        HqlKeyword _direction;
        string _tableReference;
        string _fieldRename; // used in header as the header, used in output as the original fieldname for the {replace}
        bool _printResult;
    }
}
