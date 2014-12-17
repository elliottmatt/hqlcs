using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlGroupBy : HqlClause
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlGroupBy() : base(null)
        {
            //_data = null; // unnecessary
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public override void Parse(HqlTokenProcessor processor)
        {
            HqlToken token;
            bool FieldExpected = true;

            // first thing should be groupby
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.GROUPBY)
                return;

            // now pull back things until you get to HAVING or ORDER BY or EOL
            for (; ; )
            {
                processor.MoveNextToken();
                token = processor.GetToken();
                if (processor.MatchesEndOfProcessing())
                    return;

                if (token.WordType == HqlWordType.KEYWORD &&
                    (
                        token.Keyword == HqlKeyword.HAVING ||
                        token.Keyword == HqlKeyword.ORDERBY ||
                        token.Keyword == HqlKeyword.WITH
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
                    throw new Exception("Found additional GROUPBY fields not expected");

                FieldExpected = false;

                // * = everything
                // FIELD = field<NUM> such as field1, field2, field10
                // FIXEDWIDTH = NAME(S, L) | NAME(S,L) such as bob(1, 5) or susan(10,5)

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    throw new Exception("Cannot GROUPBY STAR");
                }
                else if (token.WordType == HqlWordType.FIELD)
                {
                    AddField(token.Field);
                }
                else if (token.WordType == HqlWordType.SCALAR)
                {
                    AddField(token.Field);
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    HqlField f = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                    AddField(f);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && processor.CheckForTokenReference(ref token))
                {
                    AddField(token.Field);
                }
                else
                {
                    throw new Exception("Cannot determine data in GROUPBY clause");
                }
            }

            if (FieldExpected)
                throw new Exception("Expected additional GROUPBY fields");
        }

        public void InitializeForStoringData(IComparer<HqlKey> comp)
        {
            //_data = new HqlTable(HqlTableType.GROUPBY_TABLE, comp);
            _data = new HqlTable(comp);
        }

        public HqlResultRow FindResultRow(HqlKey key)
        {
            return Table.Find(key);
        }

        public void SetResultRow(HqlKey key, HqlResultRow row)
        {
            Table.Set(key, row);
        }

        ///////////////////////
        // Private        
        
        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters
        
        public HqlTable Table
        {
            get { return _data; }
        }

        ///////////////////////
        // Variables

        HqlTable _data;
    }
}
