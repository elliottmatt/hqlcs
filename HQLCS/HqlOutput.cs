using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlOutput : HqlClause
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlOutput(string filename, HqlWith settings) : base(settings)
        {
            _filename = filename;
        }

        ///////////////////////
        // Overridden functions

        public override void Parse(HqlTokenProcessor processor)
        {
            throw new Exception("Unneeded function! Handled in HqlWith");
        }

        ///////////////////////
        // Public 

        public new string EvaluateGroupBy(int start, HqlResultRow row)
        {
            StringBuilder sb = new StringBuilder(_filename);
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                string result = HqlSelect.EvaluateGroupByField(_fieldgroup[i], start + i, row).ToString();
                sb.Replace("{" + _fieldgroup[i].FieldRename + "}", result);
            }

            return sb.ToString();
        }

        public new string Evaluate(HqlRecord record)
        {
            StringBuilder sb = new StringBuilder(_filename);
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                string result = _fieldgroup[i].GetValue(record).ToString();
                sb.Replace("{" + _fieldgroup[i].FieldRename + "}", result);
            }

            return sb.ToString();
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters
                
        ///////////////////////
        // Variables

        string _filename;
    }
}
