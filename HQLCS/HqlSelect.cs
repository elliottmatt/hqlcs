using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlSelect : HqlClause
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlSelect(HqlWith settings) : base (settings)
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

            // first thing should be select
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.SELECT)
                throw new Exception("Expected SELECT");

            // now pull back things until you get to FROM
            for (; ; )
            {
                processor.MoveNextToken();
                token = processor.GetToken();
                if (processor.MatchesEndOfProcessing())
                    throw new Exception("Reached EOL in SELECT clause");

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.FROM)
                    break;

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.COMMA)
                {
                    FieldExpected = true;
                    continue;
                }

                if (_fieldgroup != null && _fieldgroup.Count > 0 && token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.AS)
                {
                    processor.MoveNextToken();
                    token = processor.GetToken();

                    if (token.WordType != HqlWordType.TEXT && token.WordType != HqlWordType.UNKNOWN)
                        throw new Exception("Expected a table reference name following AS");

                    _fieldgroup.SetPreviousFieldRename(token.Data);
                    continue;
                }

                // else time to process it!
                if (!FieldExpected)
                    throw new Exception("Found additional SELECT fields not expected");

                FieldExpected = false;

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    AddField(new HqlField(HqlFieldType.STAR));
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
                else if (token.WordType == HqlWordType.ROWNUM)
                {
                    AddField(token.Field);
                }
                else
                {
                    throw new Exception("Cannot determine data in SELECT clause");
                }
            }

            if (FieldExpected)
                throw new Exception("Expected additional SELECT fields");
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters
    }
}
