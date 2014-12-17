using System;
using System.Collections;
using System.Text;

namespace Hql
{
    class HqlEvaluator
    {
        private HqlEvaluator() { }

        static internal bool MustContainValue(bool? result, string _whereType)
        {
            if (!result.HasValue)
                throw new Exception(String.Format("Failure to evaluating {0} in WHERE clause", _whereType));
            return result.Value;
        }

        static internal bool EvalNullableBool(bool? result, bool defaultvalue)
        {
            if (result.HasValue)
                return result.Value;
            return defaultvalue;
        }

        static internal void CountOrs(ArrayList q, ref int counter, int expectedDim, ref int currentDim, ref int countElements, ref int countOrs, HqlValuesComparer list)
        {
            if (counter > q.Count)
                throw new Exception("Unexpected end of WHERE evaluation");

            int thisDim = currentDim;
            for (; ; )
            {
                if (counter >= q.Count)
                    break;

                Object o = q[counter];
                counter++;
                if (o is HqlToken)
                {
                    // open paren := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN
                    // closed paren := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN
                    // and_or := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AND_OR
                    //
                    HqlToken token = (HqlToken)o;
                    if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN)
                    {
                        currentDim++;
                        CountOrs(q, ref counter, expectedDim, ref currentDim, ref countElements, ref countOrs, list);
                        continue;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN)
                    {
                        return;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OR)
                    {
                        if (thisDim == expectedDim)
                            countOrs++;
                        continue;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AND)
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception("Unknown type of HqlToken in WHERE clause");
                    }
                }
                else if (o is HqlCompareToken)
                {
                    if (thisDim == expectedDim)
                    {
                        countElements++;
                        if (list != null)
                        {
                            list.Add((HqlCompareToken)o);
                        }
                    }

                    continue;
                }
                else
                {
                    throw new Exception("Unknown type of object in WHERE evaluation.");
                }
            }

            return;
        }

        static internal void VerifyFieldPresence(HqlClause select, HqlToken token)
        {
            if (select != null && token.WordType == HqlWordType.FIELD)
            {
                select.VerifyFieldsPresent(token.Field);
                //if (!select.ContainsField(token.Field))
                //{
                //    token.Field.PrintResult = false;
                //    select.AddField(token.Field);
                //}
            }
        }

        static internal bool? Evaluate(HqlRecord record, ArrayList q, ref int counter)
        {
            if (counter > q.Count)
                throw new Exception("Unexpected end of WHERE evaluation");

            bool? result = null;
            for (; ; )
            {
                if (counter >= q.Count)
                    break;

                Object o = q[counter];
                counter++;
                if (o is HqlToken)
                {
                    // open paren := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN
                    // closed paren := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN
                    // and_or := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AND_OR
                    //
                    HqlToken token = (HqlToken)o;
                    if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN)
                    {
#if DEBUG
                        if (result.HasValue)
                            throw new Exception("How did it get a value?");
#endif
                        result = Evaluate(record, q, ref counter);
                        continue;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN)
                    {
                        return result;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OR)
                    {
                        bool b2 = MustContainValue(result, "left OR");
                        bool b1 = MustContainValue(Evaluate(record, q, ref counter), "right OR");
                        result = b1 || b2;
                        continue;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AND)
                    {
                        bool b2 = MustContainValue(result, "left AND");
                        bool b1 = MustContainValue(Evaluate(record, q, ref counter), "right AND");
                        result = b1 && b2;
                        continue;
                    }
                    else
                    {
                        throw new Exception("Unknown type of HqlToken in WHERE clause");
                    }
                }
                else if (o is HqlCompareToken)
                {
                    HqlCompareToken c = (HqlCompareToken)o;
#if DEBUG
                    if (result.HasValue)
                        throw new Exception("How did it get a value?");
#endif
                    result = c.Evaluate(record);

                    continue;
                }
                else
                {
                    throw new Exception("Unknown type of object in WHERE evaluation.");
                }
            }

            return MustContainValue(result, "whole thing");
        }

        static internal bool? EvaluateJoin(HqlValues values, ArrayList q, ref int counter)
        {
            if (counter > q.Count)
                throw new Exception("Unexpected end of WHERE evaluation");

            bool? result = null;
            for (; ; )
            {
                if (counter >= q.Count)
                    break;

                Object o = q[counter];
                counter++;
                if (o is HqlToken)
                {
                    // open paren := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN
                    // closed paren := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN
                    // and_or := token.Type == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AND_OR
                    //
                    HqlToken token = (HqlToken)o;
                    if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN)
                    {
                        result = EvaluateJoin(values, q, ref counter);
                        continue;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN)
                    {
                        return result;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OR)
                    {
                        // null null = true
                        // null false = true
                        // null true = true
                        // false true = true
                        // false false = false
                        // true true = true
                        bool? b1 = EvaluateJoin(values, q, ref counter);
                        if (b1.HasValue && result.HasValue && b1 == false && result == false)
                            result = false;
                        else
                            result = true;
                        //result = MustContainValue(b1, "right OR") || MustContainValue(currentval, "left OR");
                        continue;
                    }
                    else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AND)
                    {
                        // null null = null
                        // null false = false
                        // null true = true
                        // false true = false
                        // false false = false
                        // true true = true
                        bool? b1 = EvaluateJoin(values, q, ref counter);
                        if (!b1.HasValue)
                        {
                            //result = result; // Compiler yells at me so I'm going to comment out
                        }
                        else if (!result.HasValue)
                            result = b1;
                        else
                            result = b1.Value && result.Value;
                        //result = MustContainValue(b1, "right AND") && MustContainValue(currentval, "left AND");
                        continue;
                    }
                    else
                    {
                        throw new Exception("Unknown type of HqlToken in WHERE clause");
                    }
                }
                else if (o is HqlCompareToken)
                {
                    HqlCompareToken c = (HqlCompareToken)o;
                    result = c.EvaluateJoin(values);

                    continue;
                }
                else
                {
                    throw new Exception("Unknown type of object in WHERE evaluation.");
                }
            }

            return result;
        }

        static internal void CreateSortFields(HqlValuesComparer list, ArrayList q)
        {
            int counter = 0;
            int current = 0;
            if (q == null || q.Count == 0)
                return;

            // every level within braces are at the same level, and each nested query gets it's own 
            // example:
            //         dim = 0            dim = 0             dim = 1            dim = 1              dim = 2           dim = 2             dim = 0             dim = 3            dim = 3
            // ON a.field1 = "A" and a.field1 = "B" and (a.field1 = "C" and a.field1 = "D") and (a.field1 = "E" or a.field1 = "F") and a.field1 = "G" and (a.field1 = "H" and a.field1 = "I")");
            // result of above: A, B, D, E            

            // see how many dim can be added
            for (int dim = 0; ; dim++)
            {
                int countElement = 0;
                int countOr = 0;

                counter = 0;
                current = 0;
                HqlEvaluator.CountOrs(q, ref counter, dim, ref current, ref countElement, ref countOr, null);
                if (countElement == 0)
                    break;
                if (countOr == 0)
                {
                    // Add this dim
                    counter = 0;
                    current = 0;
                    CountOrs(q, ref counter, dim, ref current, ref countElement, ref countOr, list);
                }
                else
                {
                    // found ORs in this dim
                    break;
                    // In the above case, I am leaving out the DIM3 from the join but this is a small
                    // percentage case and I'm not really worried it about since if i just get DIM 0 on most queries
                    // that is PLENTY good joining.
                }
            }
        }

    }
}
