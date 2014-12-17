using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlOrderBy : HqlClause, IComparer<HqlKey>
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlOrderBy() : base(null)
        {            
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public override void Parse(HqlTokenProcessor processor)
        {
            HqlToken token;
            bool FieldExpected = true;

            // first thing should be orderby
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.ORDERBY)
                return;

            // now pull back things until you get to HAVING or EOL
            for (; ; )
            {
                processor.MoveNextToken();
                token = processor.GetToken();
                if (processor.MatchesEndOfProcessing())
                    return;

                if (token.WordType == HqlWordType.KEYWORD && (token.Keyword == HqlKeyword.ASCENDING || token.Keyword == HqlKeyword.DESCENDING))
                {
                    _fieldgroup.SetPreviousDirection(token.Keyword);
                    continue;
                }

                if (token.WordType == HqlWordType.KEYWORD && 
                        (
                            token.Keyword == HqlKeyword.WITH ||
                            token.Keyword == HqlKeyword.HAVING
                        )
                    )
                {
                    break;
                }

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.COMMA)
                {
                    FieldExpected = true;
                    continue;
                }

                // else time to process it!
                if (!FieldExpected)
                    throw new Exception("Found additional ORDER-BY fields not expected");

                FieldExpected = false;

                // * = everything
                // FIELD = field<NUM> such as field1, field2, field10
                // FIXEDWIDTH = NAME(S, L) | NAME(S,L) such as bob(1, 5) or susan(10,5)

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    throw new Exception("Cannot ORDER-BY STAR");
                }
                else if (token.WordType == HqlWordType.FIELD)
                {
                    AddField(token.Field);
                }
                else if (token.WordType == HqlWordType.SCALAR)
                {
                    AddField(token.Field);
                }
                else if (token.WordType == HqlWordType.ROWNUM)
                {
                    AddField(token.Field);
                }
                else
                {
                    throw new Exception("Cannot determine data in ORDER-BY clause");
                }
            }

            if (FieldExpected)
                throw new Exception("Expected additional GROUPBY fields");
        }

        /// <summary>
        /// Same as GetValues() except it erases the values of the functions
        /// because the Key in the array cannot have the value since it constantly changes.
        /// I need a place holder in the final answer though due to sorting though
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public object[] GetKeyValues(HqlRecord record)
        {
            object[] o = _fieldgroup.GetValues(record);
            for (int i = 0; i < _fieldgroup.Count; ++i)
            {
                if (_fieldgroup[i].HasFunction)
                {
                    o[i] = null;
                }
            }
            
            return o;
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        public int Compare(HqlKey a, HqlKey b)
        {
            int x = 0;
            for (int i = 0; i < _fieldgroup.Count; i++)
            {
                x = a.CompareTo(b, i);
                if (_fieldgroup[i].Direction == HqlKeyword.DESCENDING)
                    x = 0 - x;
                if (x != 0) return x;
            }

            if (x == 0)
            {
                int ahash = a.GetHashCode();
                int bhash = b.GetHashCode();
                if (ahash < bhash)
                    return -1;
                else if (ahash > bhash)
                    return 1;
                return 0;
            }

            return x;
        }
                
        ///////////////////////
        // Getters/Setters

        ///////////////////////
        // Variables

    }
}