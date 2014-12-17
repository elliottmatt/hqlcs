using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    public enum HqlCompareTokenType
    {
        IN,
        NOT_IN,
        SINGLE,
    }

    class HqlCompareToken
    {
        ///////////////////////
        // Static Functions

        static public bool MakeAppropriateObjects(ref object v1, ref object v2)
        {
            if (
                (v1 is Int64 || v1 is Int32 || v1 is string && HqlCategory.IsInt((string)v1)) &&
                (v2 is Int64 || v2 is Int32 || v2 is string && HqlCategory.IsInt((string)v2))
                )
            {
                if (v1 is string) { v1 = Int64.Parse(v1.ToString()); }
                else if (v1 is Int32) { v1 = (Int64)((Int32)v1); }
                if (v2 is string) { v2 = Int64.Parse(v2.ToString()); }
                else if (v2 is Int32) { v2 = (Int64)((Int32)v2); }
                return true;
            }
            else if (
                (v1 is decimal || v1 is string && HqlCategory.IsFloat((string)v1)) &&
                (v2 is decimal || v2 is string && HqlCategory.IsFloat((string)v2))
                )
            {
                if (v1 is string) { v1 = Decimal.Parse(v1.ToString()); }
                if (v2 is string) { v2 = Decimal.Parse(v2.ToString()); }
                return true;
            }
            return false;
        }

        static public bool Evaluate(object v1, object v2, HqlToken compare)
        {
            switch (compare.Data)
            {
                case "not inlike":
                case "not in like":
                case "in not like":
                case "not like":
                    return !Like(v1, v2);
                case "in like":
                case "inlike":
                case "like":
                    return Like(v1, v2);
            }

            MakeAppropriateObjects(ref v1, ref v2);

            switch (compare.Data)
            {
                case "<":
                    return LessThan(v1, v2);
                case ">":
                    return !(LessThan(v1, v2) || Equal(v1, v2));
                case "<=":
                    return LessThan(v1, v2) || Equal(v1, v2);
                case ">=":
                    return !LessThan(v1, v2);
                case "in":
                case "=":
                case "not in":
                case "is":
                    return Equal(v1, v2);
                case "<>":
                case "!=":
                case "is not":
                    return !Equal(v1, v2);
                default:
                    throw new NotSupportedException("Unknown type of compare");
            }
        }

        static public int CompareTo(object v1, object v2)
        {
            MakeAppropriateObjects(ref v1, ref v2);

            if (v1 is string && v2 is string)
                return String.Compare((string)v1, (string)v2, StringComparison.CurrentCulture);
            else if (v1 is Int64 && v2 is Int64)
                return ((Int64)v1).CompareTo((Int64)v2);
            else if (v1 is int && v2 is int)
                return ((int)v1).CompareTo((int)v2);
            else if (v1 is decimal && v2 is decimal)
                return ((decimal)v1).CompareTo((decimal)v2);
            else if (v1 is string)
                return String.Compare((string)v1, v2.ToString(), StringComparison.CurrentCulture);
            else if (v2 is string)
                return String.Compare(v1.ToString(), (string)v2, StringComparison.CurrentCulture);
            else
                return String.Compare(v1.ToString(), v2.ToString(), StringComparison.CurrentCulture);
        }

        static private bool LessThan(object v1, object v2)
        {
            if (v1 is string && v2 is string)
                return String.Compare((string)v1, (string)v2, StringComparison.CurrentCulture) < 0;
            else if (v1 is Int64 && v2 is Int64)
                return (Int64)v1 < (Int64)v2;
            else if (v1 is int && v2 is int)
                return (int)v1 < (int)v2;
            else if (v1 is decimal && v2 is decimal)
                return (decimal)v1 < (decimal)v2;
            else if (v1 is DateTime && v2 is DateTime)
                return (DateTime)v1 < (DateTime)v2;
            else if (v1 is string)
                return String.Compare((string)v1, v2.ToString(), StringComparison.CurrentCulture) < 0;
            else if (v2 is string)
                return String.Compare(v1.ToString(), (string)v2, StringComparison.CurrentCulture) < 0;
            else
                return String.Compare(v1.ToString(), v2.ToString(), StringComparison.CurrentCulture) < 0;
        }

        static private bool Equal(object v1, object v2)
        {
            if (v1 is string && v2 is string)
                return String.Equals((string)v1, (string)v2, StringComparison.CurrentCulture);
            else if (v1 is Int64 && v2 is Int64)
                return (Int64)v1 == (Int64)v2; 
            else if (v1 is int && v2 is int)
                return (int)v1 == (int)v2;
            else if (v1 is decimal && v2 is decimal)
            {
                //decimal result = (decimal)v1 - (decimal)v2;
                //return (result < 0.00001M && result > -0.00001M);
                return Decimal.Equals((decimal)v1, (decimal)v2);
            }
            else if (v1 is DateTime && v2 is DateTime)
                return (DateTime)v1 == (DateTime)v2;
            else if (v1 is string) // TODO, do I have to print as decimals somehow?
                return String.Equals((string)v1, v2.ToString(), StringComparison.CurrentCulture);
            else if (v2 is string)
                return String.Equals(v1.ToString(), (string)v2, StringComparison.CurrentCulture);
            else
                return String.Equals(v1.ToString(), v2.ToString(), StringComparison.CurrentCulture);
        }

        static private bool Like(object field, object wild)
        {
            if (!(field is string && wild is string))
            {
                // TODO, if they are decimal do I format as decimals?
                field = field.ToString();
                wild = wild.ToString();
            }
            string fieldstr = (string)field;
            string wildstr = (string)wild;

            return (Like(fieldstr, wildstr) == 0);
        }

        static public int Like(string fieldstr, string wildstr)
        {
            int f = 0;
            int w = 0;

            // match them while there isn't a %
            for (; f < fieldstr.Length && w < wildstr.Length && wildstr[w] != '%'; w++, f++)
            {
                if (fieldstr[f] != wildstr[w] && wildstr[w] != '_')
                    return wildstr[w].CompareTo(fieldstr[f]);
            }
            if (f == fieldstr.Length && w == wildstr.Length)
                return 0;
            if (w == wildstr.Length)
                return -1; // return fieldstr.CompareTo(wildstr);

            // eat any contiguous %
            if (wildstr[w] == '%')
            {
                for (w++; w < wildstr.Length && wildstr[w] == '%'; w++) ;
                if (w == wildstr.Length)
                    return 0;
            }
            // look for a match
            for (; f < fieldstr.Length; f++)
            {
                if (Like(fieldstr.Substring(f), wildstr.Substring(w)) == 0)
                    return 0;
            }

            // force all wildcards to go to the beginning
            if (wildstr.StartsWith("%", StringComparison.CurrentCulture))
                return -1;
            return String.Compare(wildstr.Substring(w), fieldstr.Substring(f), StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Created because there were originally more checks done in here
        /// </summary>
        /// <param name="token1"></param>
        /// <param name="token2"></param>
        /// <returns></returns>
        static bool NeedsJoinEvaluation(HqlToken token1, HqlToken token2)
        {
            if (!token1.NeedsEvaluation && !token2.NeedsEvaluation)
                return false;
            return true;
        }

        static private bool ExistsIn(HqlToken t, HqlFieldGroup group)
        {
            if (t.WordType != HqlWordType.FUNCTION && t.WordType != HqlWordType.SCALAR && t.WordType != HqlWordType.FIELD)
                return true;

            for (int i = 0; i < group.Count; ++i)
            {
                if (group[i].Equals(t.Field))
                    return true;
            }

            return false;
        }

        static private bool NotNullAndContainsField(HqlToken t, HqlField field)
        {
            if (t == null || t.Field == null)
                return false;

            return t.Field.ContainsField(field);
        }

        static private bool NotNullAndContainsRownum(HqlToken t, bool OnlyWithoutTableReference)
        {
            if (t == null || t.Field == null)
                return false;

            return t.Field.ContainsRownum(OnlyWithoutTableReference);
        }

        ///////////////////////
        // Constructors

        public HqlCompareToken(HqlToken token1, HqlToken compare, HqlToken token2)
        {
            _type = HqlCompareTokenType.SINGLE;
            _token1 = token1;
            _compare = compare;
            _compare.Data = _compare.Data.ToLower();
            _token2 = token2;
            // _values = null; // unnecessary
        }

        public HqlCompareToken(HqlToken token1, HqlToken compare, ArrayList values)
        {
            if (compare.Keyword == HqlKeyword.IN)
                _type = HqlCompareTokenType.IN;
            else if (compare.Keyword == HqlKeyword.NOT_IN)
                _type = HqlCompareTokenType.NOT_IN;
            else
                throw new ArgumentException("Unknown 'IN' type");
            _token1 = token1;
            _compare = compare;
            _compare.Data = _compare.Data.ToLower();
            // _token2 = null; // unnecessary
            _values = values;
        }

        ///////////////////////
        // Overridden functions

        public override string ToString()
        {
            switch (_type)
            {
                case HqlCompareTokenType.SINGLE:
                    return String.Format("{0} {1} {2}", Token1.ToString(), Compare.Data, Token2.ToString());
                case HqlCompareTokenType.IN:
                    {
                        StringBuilder sb = new StringBuilder("IN (");
                        for (int i = 0; i < _values.Count; ++i)
                        {
                            HqlToken token = (HqlToken)_values[i];
                            if (i > 0)
                                sb.Append(", ");
                            sb.Append(token.ToString());
                        }
                        sb.Append(")");
                        return sb.ToString();
                    }
                default:
                    throw new NotSupportedException("Unknown type of COMPARETOKEN");
            }
        }

        ///////////////////////
        // Public 

        public bool ContainsField(HqlField field)
        {
            if (NotNullAndContainsField(Token1, field))
                return true;
            if (NotNullAndContainsField(Token2, field))
                return true;
            return false;
        }

        public bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            if (NotNullAndContainsRownum(Token1, OnlyWithoutTableReference))
                return true;
            if (NotNullAndContainsRownum(Token2, OnlyWithoutTableReference))
                return true;
            return false;
        }


        public bool ExistsIn(HqlFieldGroup group)
        {
            switch (_type)
            {
                case HqlCompareTokenType.SINGLE:
                    return ExistsInSingle(group);
                case HqlCompareTokenType.IN:
                    return ExistsInValues(group);
                default:
                    throw new NotSupportedException("Unknown type of COMPARETOKEN");
            }
        }

        public bool Evaluate(HqlRecord record)
        {
            switch (_type)
            {
                case HqlCompareTokenType.SINGLE:
                    return HqlEvaluator.EvalNullableBool(EvaluateSingle(record), false);
                case HqlCompareTokenType.IN:
                    return HqlEvaluator.EvalNullableBool(EvaluateInValues(record), false);
                case HqlCompareTokenType.NOT_IN:
                    bool b = HqlEvaluator.EvalNullableBool(EvaluateInValues(record), false); // TODO, should this be true?
                    return !b;
                default:
                    throw new NotSupportedException("Unknown type of COMPARETOKEN");
            }
        }

        public bool? EvaluateJoin(HqlValues values)
        {
            switch (_type)
            {
                case HqlCompareTokenType.SINGLE:
                    return EvaluateJoinSingleValues(values);
                case HqlCompareTokenType.IN:
                    return EvaluateJoinInValues(values);
                case HqlCompareTokenType.NOT_IN:
                    //bool? b = EvaluateJoinInValues(values);
                    //if (b.HasValue) b = !b;
                    //return b;
                default:
                    throw new NotSupportedException("Unknown type of COMPARETOKEN");
            }
        }

        public void Cleanup()
        {
            if (_token1 != null)
            {
                _token1.Cleanup();
                _token1 = null;
            }
            if (_compare != null)
            {
                _compare.Cleanup();
                _compare = null;
            }
            if (_token2 != null)
            {
                _token2.Cleanup();
                _token2 = null;
            }
            for (int i = 0; _values != null && i < _values.Count; ++i)
            {
                object o = _values[i];
                if (o is HqlToken)
                    ((HqlToken)o).Cleanup();
#if DEBUG
                else
                    throw new NotSupportedException("Unknown values object");
#endif
            }
            _values = null;
        }

        ///////////////////////
        // Private

        private bool? EvaluateSingle(HqlRecord record)
        {
            object v1 = record.GetValue(Token1);
            object v2 = record.GetValue(Token2);

            return Evaluate(v1, v2, Compare);
        }

        private bool? EvaluateInValues(HqlRecord record)
        {
            object v1 = record.GetValue(Token1);

            for (int i = 0; i < _values.Count; ++i)
            {
                try
                {
                    HqlToken t = (HqlToken)_values[i];
                    object v2 = record.GetValue(t);

                    if (Evaluate(v1, v2, Compare))
                        return true;
                }
                catch (Exception ex)
                {
                    int stop = 0;
                    throw;
                }
            }
            return false;
        }

        private bool? EvaluateJoinSingleValues(HqlValues values)
        {
            // TODO, i need a "valid" data flag, because if there isn't a field8, then that's a different error
            object v1;
            object v2;

            if (!NeedsJoinEvaluation(Token1, Token2))
                return true;

            if (!values.HasValue(Token1, out v1) || !values.HasValue(Token2, out v2))
                return null;

            return Evaluate(v1, v2, Compare);
        }

        private bool EvaluateJoinInValues(HqlValues values)
        {
            object v1 = values.GetValue(Token1);

            for (int i = 0; i < _values.Count; ++i)
            {
                HqlToken t = (HqlToken)_values[i];
                object v2 = values.GetValue(t);

                if (Evaluate(v1, v2, Compare))
                    return true;
            }
            return false;
        }

        private bool ExistsInValues(HqlFieldGroup group)
        {
            if (!ExistsIn(Token1, group))
                return false;

            for (int i = 0; i < _values.Count; ++i)
            {
                HqlToken t = (HqlToken)_values[i];
                if (!ExistsIn(t, group))
                    return false;
            }
            
            return true;
        }

        private bool ExistsInSingle(HqlFieldGroup group)
        {
            if (!ExistsIn(Token1, group))
                return false;
            if (!ExistsIn(Token2, group))
                return false;
            return true;
        }

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        public HqlToken Token1
        {
            get { return _token1; }
            //set { _token1 = value; }
        }

        public HqlToken Token2
        {
            get { return _token2; }
            //set { _token2 = value; }
        }

        public HqlToken Compare
        {
            get { return _compare; }
            //set { _compare = value; }
        }

        public ArrayList InValues
        {
            get { return _values; }
        }

        public HqlCompareTokenType CompareType
        {
            get { return _type; }
        }

        ///////////////////////
        // Variables

        HqlCompareTokenType _type;
        HqlToken _token1;
        HqlToken _compare;
        HqlToken _token2;
        ArrayList _values;
    }
}
