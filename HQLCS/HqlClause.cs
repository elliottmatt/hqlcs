using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    abstract class HqlClause
    {
        ///////////////////////
        // Static Functions

        static public object EvaluateGroupByField(HqlField f, int num, HqlResultRow row)
        {
            if (f.HasFunction)
            {
                if (f.Scalar != null)
                {
                    object s = ((HqlCalc)row[num]).GetValue();
                    return f.Scalar.Evaluate(s);
                }
                else
                {
                    object s = ((HqlCalc)row[num]).GetPrintValue();
                    return s;
                }
            }
            else
            {
                string s = row[num].ToString();
                return s;
            }
        }

        ///////////////////////
        // Constructors

        public HqlClause(HqlWith settings)
        {
            _fieldgroup = new HqlFieldGroup();
            _settings = settings;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public abstract void Parse(HqlTokenProcessor processor);

        public string Evaluate(HqlRecord record)
        {
            StringBuilder sb = new StringBuilder();
            bool printed = false;
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                if (!_fieldgroup[i].PrintResult)
                    continue;
                object result = _fieldgroup[i].GetValue(record);
                if (printed)
                    sb.Append(_settings.OutDelimiter);
                sb.Append(result.ToString());
                printed = true;
            }
            sb.Append(_settings.FinalDelimiter);

            return sb.ToString();
        }

        public string EvaluateGroupBy(int start, HqlResultRow row)
        {
            StringBuilder sb = new StringBuilder();
            bool printed = false;
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                if (!_fieldgroup[i].PrintResult)
                    continue;
                string s = HqlSelect.EvaluateGroupByField(_fieldgroup[i], start + i, row).ToString();
                if (printed)
                    sb.Append(_settings.OutDelimiter);
                sb.Append(s);
                printed = true;
            }
            sb.Append(_settings.FinalDelimiter);

            return sb.ToString();
        }

        public void AddToResultRow(int start, HqlRecord record, HqlResultRow row)
        {
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                if (_fieldgroup[i].HasFunction)
                {
                    object value = _fieldgroup[i].GetFunctionValue(record);
                    row[start + i] = value; // this is an overloaded function that "adds" to this function or assigns if the first time
                }
                else
                {
                    if (!row.ValuesSet)
                    {
                        // save these!
                        object value = _fieldgroup[i].GetValue(record);
                        row[start + i] = value;
                    }
                }
            }
        }

        public string GetHeaderRow()
        {
            StringBuilder sb = new StringBuilder();
            bool printed = false;
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                if (!_fieldgroup[i].PrintResult)
                    continue;
                object result = _fieldgroup[i].GetHeaderValue();
                if (printed)
                    sb.Append(_settings.OutDelimiter);
                sb.Append(result.ToString());
                printed = true;
            }
            sb.Append(_settings.FinalDelimiter);

            return sb.ToString();
        }

        public object[] GetValues(HqlRecord record)
        {
            return _fieldgroup.GetValues(record);
        }

        public void Cleanup()
        {
            if (_fieldgroup != null)
            {
                _fieldgroup.Cleanup();
                _fieldgroup = null;
            }

            if (_settings != null)
            {
                _settings.Cleanup();
                _settings = null;
            }
        }

        public void AddField(HqlField field)
        {
            _fieldgroup.AddField(field);
        }

        public void VerifyFieldsPresent()
        {
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                VerifyFieldsPresent(_fieldgroup[i]);
            }
        }

        public void VerifyFieldsPresent(HqlField field)
        {
            //switch (field.FieldType)
            //{
            //    case HqlFieldType.LITERAL_FLOAT:
            //    case HqlFieldType.LITERAL_INT:
            //    case HqlFieldType.LITERAL_STRING:
            //    case HqlFieldType.NULL:
            //        break;
            //    case HqlFieldType.FUNCTION:
            //        VerifyFieldsPresent(field.Func);
            //        break;
            //    case HqlFieldType.SCALAR:
            //        VerifyFieldsPresent(field.Scalar);
            //        break;
            //    default:
            //        if (!ContainsField(field))
            //        {
            //            field.PrintResult = false;
            //            AddField(field);
            //        }
            //        break;
            //}

            switch (field.FieldType)
            {
                case HqlFieldType.LITERAL_FLOAT:
                case HqlFieldType.LITERAL_INT:
                case HqlFieldType.LITERAL_STRING:
                case HqlFieldType.NULL:
                    return;
            }

            if (!ContainsField(field))
            {
                field.PrintResult = false;
                AddField(field);
            }

            if (field.FieldType == HqlFieldType.FUNCTION)
                VerifyFieldsPresent(field.Func);
            if (field.FieldType == HqlFieldType.SCALAR)
                VerifyFieldsPresent(field.Scalar);
        }

        ///////////////////////
        // Private

        private void VerifyFieldsPresent(HqlFunction func)
        {
            if (func.HasScalar)
            {
                HqlField field = func.Field;
                VerifyFieldsPresent(field);
            }
        }

        private void VerifyFieldsPresent(HqlScalar scalar)
        {
            if (scalar.HasMultipleFields)
            {
                for (int i = 0; i < scalar.Fields.Length; ++i)
                {
                    VerifyFieldsPresent(scalar.Fields[i]);
                }
            }
            else
            {
                VerifyFieldsPresent(scalar.Field);
            }
        }

        ///////////////////////
        // Fields

        public bool HasFunctions
        {
            get { return _fieldgroup.HasFunctions; }
        }

        public int Count
        {
            get { return _fieldgroup.Count; }
        }

        public int CountPrintedFields
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _fieldgroup.Count; ++i)
                {
                    if (_fieldgroup[i].PrintResult)
                        count++;
                }
                return count;
            }
        }

        public HqlFieldGroup FieldGroup
        {
            get { return _fieldgroup; }
        }

        public bool ContainsField(HqlField field)
        {
            return _fieldgroup.ContainsField(field);
        }

        public void ClearFields()
        {
            _fieldgroup.ClearFields();
        }

        ///////////////////////
        // Getters/Setters

        protected HqlFieldGroup _fieldgroup;
        protected HqlWith _settings;
    }
}
