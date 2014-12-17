using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlResultRow
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        //public HqlResultRow(HqlFieldGroup group1, HqlFieldGroup group2)
        //{
        //    Init(group1, group2, null);
        //}

        public HqlResultRow(HqlFieldGroup group1, HqlFieldGroup group2, HqlFieldGroup group3)
        {
            Init(group1, group2, group3);
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public object GetValue(int n)
        {
            object o = this[n];

            if (o is HqlCalc)
                return ((HqlCalc)o).GetValue();
            else if (o is string)
                return o;

            throw new Exception(String.Format("Unknown GetValue type of |{0}|", o.GetType().ToString()));
        }

        ///////////////////////
        // Private

        private void Init(HqlFieldGroup group1, HqlFieldGroup group2, HqlFieldGroup group3)
        {
            _valuesSet = false;

            int countGroup1 = (group1 == null) ? 0 : group1.Count;
            int countGroup2 = (group2 == null) ? 0 : group2.Count;
            int countGroup3 = (group3 == null) ? 0 : group3.Count;

            HqlField[] fields1 = (group1 == null) ? null : group1.Fields;
            HqlField[] fields2 = (group2 == null) ? null : group2.Fields;
            HqlField[] fields3 = (group3 == null) ? null : group3.Fields;

            _calc = new HqlCalc[countGroup1 + countGroup2 + countGroup3];
            _grouped = new object[countGroup1 + countGroup2 + countGroup3];

            int start = 0;
            for (int i = 0; i < countGroup1; ++i)
            {
                HqlField f = fields1[i];
                if (f.HasFunction)
                    _calc[i + start] = f.Func.CreateCalcObject();
            }
            start += countGroup1;

            for (int i = 0; i < countGroup2; ++i)
            {
                HqlField f = fields2[i];
                if (f.HasFunction)
                    _calc[i + start] = f.Func.CreateCalcObject();
            }
            start += countGroup2;

            for (int i = 0; i < countGroup3; ++i)
            {
                HqlField f = fields3[i];
                if (f.HasFunction)
                    _calc[i + start] = f.Func.CreateCalcObject();
            }
            //start += countGroup3;
        }

        ///////////////////////
        // Fields

        public bool ValuesSet
        {
            get { return _valuesSet; }
            set { _valuesSet = value; }
        }

        ///////////////////////
        // Getters/Setters

        public object this[int i]
        {
            get
            {
                if (_calc[i] != null)
                    return _calc[i];
                return _grouped[i];
            }

            set
            {
                if (_calc[i] != null)
                    _calc[i].Add(value);
                _grouped[i] = value;
            }
        }

        ///////////////////////
        // Variables

        private bool _valuesSet;
        private object[] _grouped;
        private HqlCalc[] _calc;
    }
}
