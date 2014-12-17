using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlWhere
    {
        ///////////////////////
        // Static Functions
        
        static public string NAME_WHERE = "WHERE";
        static public string NAME_ONJOIN = "ONJOIN";
        static public string NAME_HAVING = "HAVING";

        ///////////////////////
        // Constructors

        public HqlWhere(string name)
        {
            _q = new ArrayList();
            _name = name;
        }

        ///////////////////////
        // Overridden functions

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int depth = 0;
            int indent = 3;
            for (int i = 0; i < _q.Count; ++i)
            {
                object o = _q[i];
                if (o is HqlCompareToken)
                {
                    sb.Append(new String(' ', depth * indent) + ((HqlCompareToken)o).ToString() + System.Environment.NewLine);
                }
                else if (o is HqlToken)
                {
                    HqlToken t = ((HqlToken)o);
                    if (t.Keyword == HqlKeyword.CLOSEDPAREN)
                        depth--;
                    sb.Append(new String(' ', depth * indent) + t.Data + System.Environment.NewLine);
                    if (t.Keyword == HqlKeyword.OPENPAREN)
                        depth++;
                }
                else
                {
                    sb.Append(new String(' ', depth * indent) + "What is " + o.GetType() + "????" + System.Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        ///////////////////////
        // Public 
    
        public virtual void Parse(HqlTokenProcessor processor)
        {
            Parse(processor, HqlKeyword.WHERE, HqlKeyword.WHERE, null);
        }

        public void Parse(HqlTokenProcessor processor, HqlKeyword firstToken, HqlKeyword thisKeyword, HqlClause select)
        {
            HqlToken token;
            int openParen = 0;

            // first thing should be WHERE
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != firstToken)
                return;

            // now pull back things until you get to EOF or GROUP
            processor.MoveNextToken();
            for (int i = 0; ; i++)
            {
                if (!ParseCompare(processor, (i > 0), ref openParen, true, thisKeyword, select))
                    break;
            }

            if (openParen != 0)
                throw new Exception("Unbalanced parens");
        }

        public bool Evaluate(HqlRecord record)
        {
            int counter = 0;
            if (_q == null || _q.Count == 0)
                return true;

            bool? res = HqlEvaluator.Evaluate(record, _q, ref counter);
            return HqlEvaluator.EvalNullableBool(res, false);
        }

        public bool EvaluateJoin(HqlValues values, bool returnOnNull)
        {
            // I need the comparer to know if it's part of this table
            // then compare if against a literal and otherwise return true
            // example: if look for table (a)
            // 1) (a.field1 = 'bob') <= actually evaluate
            // 2) (a.field1 = b.field1) <= return true because we need to save this record for
            //    later evaluation
            // 3) (b.field1 = 'joe') <= return false because i don't care about this record
            // 4) (b.field1 is NULL) <= return true because I can't evaluate this here

            int counter = 0;
            if (_q == null || _q.Count == 0)
                return true;

            bool? res = HqlEvaluator.EvaluateJoin(values, _q, ref counter);
            //return EvalNullableBool(res, true);
            return HqlEvaluator.EvalNullableBool(res, returnOnNull);
        }

        public void CreateSortFields(HqlValuesComparer list)
        {
            HqlEvaluator.CreateSortFields(list, _q);
        }

        public void Cleanup()
        {
            for (int i = 0; i < _q.Count; ++i)
            {
                Object o = _q[i];
                if (o is HqlToken)
                {
                    ((HqlToken)o).Cleanup();
                }
                else if (o is HqlCompareToken)
                {
                    ((HqlCompareToken)o).Cleanup();
                }
                else
                {
                    throw new Exception("Unknown object in list");
                }
            }
                
            _q.Clear();
        }

        public bool ContainsField(HqlField field)
        {
            for (int i = 0; i < _q.Count; ++i)
            {
                Object o = _q[i];
                if (o is HqlToken)
                {
                    HqlToken token = (HqlToken)o;
                    if (token.Field != null && token.Field.ContainsField(field))
                        return true;
                }
                else if (o is HqlCompareToken)
                {
                    HqlCompareToken ct = (HqlCompareToken)o;
                    if (ct.ContainsField(field))
                        return true;
                }
                else
                {
                    throw new Exception("Unknown object in list");
                }
            }

            return false;
        }

        public bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            for (int i = 0; i < _q.Count; ++i)
            {
                Object o = _q[i];
                if (o is HqlToken)
                {
                    HqlToken token = (HqlToken)o;
                    if (token.Field != null && token.Field.ContainsRownum(OnlyWithoutTableReference))
                        return true;
                }
                else if (o is HqlCompareToken)
                {
                    HqlCompareToken ct = (HqlCompareToken)o;
                    if (ct.ContainsRownum(OnlyWithoutTableReference))
                        return true;
                }
                else
                {
                    throw new Exception("Unknown object in list");
                }
            }

            return false;
        }


        ///////////////////////
        // Private

        protected bool ParseCompare(HqlTokenProcessor processor, bool andorAllowed, ref int openParen, bool closedAllowed, HqlKeyword thisKeyword, HqlClause select)
        {
#if DEBUG
            if (thisKeyword != HqlKeyword.WHERE && thisKeyword != HqlKeyword.HAVING && thisKeyword != HqlKeyword.ON)
                throw new Exception("Cannot access ParseCompare with invalid keyword");
#endif
            HqlToken token = processor.GetToken();

            if (processor.MatchesEndOfProcessing())
                return false;

            if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN)
            {
                if (!closedAllowed)
                    throw new Exception("Empty Parens found");

                openParen--;
                return false;
            }

            if (token.WordType == HqlWordType.KEYWORD &&
                    (
                        (
                            thisKeyword == HqlKeyword.ON &&
                            (
                                token.Keyword == HqlKeyword.WHERE || 
                                token.Keyword == HqlKeyword.INNER ||
                                token.Keyword == HqlKeyword.RIGHT_OUTER ||
                                token.Keyword == HqlKeyword.LEFT_OUTER ||
                                token.Keyword == HqlKeyword.FULL_OUTER ||
                                token.Keyword == HqlKeyword.WHERE ||
                                token.Keyword == HqlKeyword.GROUPBY ||
                                token.Keyword == HqlKeyword.HAVING ||
                                token.Keyword == HqlKeyword.ORDERBY ||
                                token.Keyword == HqlKeyword.WITH
                            )
                        )
                        ||
                        (
                            thisKeyword == HqlKeyword.WHERE &&
                            (
                                token.Keyword == HqlKeyword.INNER ||
                                token.Keyword == HqlKeyword.RIGHT_OUTER ||
                                token.Keyword == HqlKeyword.LEFT_OUTER ||
                                token.Keyword == HqlKeyword.FULL_OUTER ||
                                token.Keyword == HqlKeyword.WHERE ||
                                token.Keyword == HqlKeyword.GROUPBY ||
                                token.Keyword == HqlKeyword.HAVING ||
                                token.Keyword == HqlKeyword.ORDERBY ||
                                token.Keyword == HqlKeyword.WITH
                            )
                        )
                        ||
                        (
                            thisKeyword == HqlKeyword.HAVING &&
                            (
                                token.Keyword == HqlKeyword.ORDERBY ||
                                token.Keyword == HqlKeyword.WITH
                            )
                        )
                    )
                )
            {
                return false;
            }

            if (token.WordType == HqlWordType.KEYWORD && (token.Keyword == HqlKeyword.AND || token.Keyword == HqlKeyword.OR))
            {
                if (!andorAllowed)
                    throw new Exception("Unexpected AND/OR");

                _q.Add(token);
                processor.MoveNextToken();
                return true;
            }

            if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN)
            {
                openParen++;

                _q.Add(token);
                processor.MoveNextToken();

                for (int i = 0; ; i++)
                {
                    if (!ParseCompare(processor, (i > 0), ref openParen, (i > 0), thisKeyword, select))
                        break;
                }

                token = processor.GetToken();
                if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.CLOSEDPAREN)
                    throw new Exception(String.Format("Unbalanced Parens in {0}", _name));

                _q.Add(token);
                processor.MoveNextToken();
            }
            else
            {
                HqlToken token1 = token;
                if (token1.WordType == HqlWordType.UNKNOWN && !processor.CheckForTokenReference(ref token1))
                {
                    throw new Exception(String.Format("Unknown {0} reference to {1}", _name, token1.Data));
                }
                
                // HACK - sort of a hack this says if I can't find this field
                // then add it to the select but make it not print out
                HqlEvaluator.VerifyFieldPresence(select, token1);
                // END HACK

                processor.MoveNextToken();
                HqlToken compare = processor.GetToken();

                // this could be
                // - a COMPARE such as |field > 0|
                // - an IN such as |field in ('A', 'B')|
                // - an IN such as |field in (select item from bob)|
                if (compare.WordType == HqlWordType.KEYWORD && compare.Keyword == HqlKeyword.COMPARE)
                {
                    processor.MoveNextToken();
                    HqlToken token2 = processor.GetToken();
                    if (token2.WordType == HqlWordType.UNKNOWN && !processor.CheckForTokenReference(ref token2))
                    {
                        throw new Exception(String.Format("Unknown WHERE reference to {0}", token2.Data));
                    }

                    // HACK - sort of a hack this says if I can't find this field
                    // then add it to the select but make it not print out
                    HqlEvaluator.VerifyFieldPresence(select, token2);
                    // END HACK

                    HqlCompareToken comparetoken = new HqlCompareToken(token1, compare, token2);
                    _q.Add(comparetoken);

                    processor.MoveNextToken();
                }
                else if (compare.WordType == HqlWordType.KEYWORD && (compare.Keyword == HqlKeyword.IN || compare.Keyword == HqlKeyword.NOT_IN))
                {
                    // now need to check for values or select
                    processor.MoveNextToken();
                    HqlToken paren = processor.GetToken();
                    if (paren.WordType != HqlWordType.KEYWORD || paren.Keyword != HqlKeyword.OPENPAREN)
                        throw new Exception(String.Format("Expected an open paren after IN in {0}", _name));

                    ArrayList compareValues = new ArrayList();
                    bool ExpectedValues = true;
                    for (int count = 0; ; count++)
                    {
                        processor.MoveNextToken();
                        HqlToken val = processor.GetToken();

                        if (val.WordType == HqlWordType.KEYWORD && val.Keyword == HqlKeyword.CLOSEDPAREN)
                            break;

                        if (val.WordType == HqlWordType.KEYWORD && val.Keyword == HqlKeyword.COMMA)
                        {
                            ExpectedValues = true;
                            continue;
                        }

                        if (val.WordType == HqlWordType.KEYWORD && val.Keyword == HqlKeyword.SELECT)
                        {
                            ExpectedValues = false;
                            if (count != 0)
                                throw new Exception("You can only put a nested query in an IN statement if it is the only value present");

                            HqlTokenProcessor.GetResultOfNestedQueryForInStatement(processor, compareValues);
                        }
                        else if (
                            val.WordType == HqlWordType.TEXT
                            || val.WordType == HqlWordType.INT
                            || val.WordType == HqlWordType.FLOAT
                            || val.WordType == HqlWordType.LITERAL_STRING
                            || val.WordType == HqlWordType.FIELD
                            || val.WordType == HqlWordType.SCALAR
                            )
                        {
                            ExpectedValues = false;
                            compareValues.Add(val);

                            // HACK - sort of a hack this says if I can't find this field
                            // then add it to the select but make it not print out
                            HqlEvaluator.VerifyFieldPresence(select, val);
                            // END HACK
                        }
                        else
                        {
                            throw new Exception("Found invalid value in IN clause");
                        }
                    }

                    if (ExpectedValues)
                        throw new Exception("Expected additional values in IN clause");

                    if (compareValues.Count == 0)
                        throw new Exception("No values present in IN clause");

                    HqlCompareToken comparetoken = new HqlCompareToken(token1, compare, compareValues);
                    _q.Add(comparetoken);
                    processor.MoveNextToken();
                }
                else
                {
                    throw new Exception(String.Format("Unknown {0} compare", _name));
                }
            }

            return true;
        }

        ///////////////////////
        // Fields

        public ArrayList EvaluationCriteria
        {
            get { return _q; }
        }

        ///////////////////////
        // Getters/Setters

        public int Count
        {
            get { return _q.Count; }
        }

        ///////////////////////
        // Variables

        protected ArrayList _q;
        protected string _name;
    }
}
