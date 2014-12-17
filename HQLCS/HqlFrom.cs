using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlFrom
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlFrom()
        {
            //_sources = null; // unnessecary
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public void PostCompile(HqlWith settings)
        {
            _settings = settings;
            for (int i = 0; i < _sources.Length; ++i)
            {
                _sources[i].Settings = settings;
            }
        }

        public void Parse(HqlTokenProcessor processor)
        {
            HqlToken token;
            bool FileExpected = true;
            HqlKeyword joinType;

            // first thing should be from
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.FROM)
                throw new Exception("Expected FROM");

            bool ReadNextToken = true;
            // now pull back things until you get to EOF or WHERE
            for (; ; )
            {
                joinType = HqlKeyword.UNKNOWN;

                if (ReadNextToken)
                    processor.MoveNextToken();
                ReadNextToken = true;

                token = processor.GetToken();
                if (processor.MatchesEndOfProcessing())
                    break;

                if (token.WordType == HqlWordType.KEYWORD &&
                        (
                            token.Keyword == HqlKeyword.WHERE ||
                            token.Keyword == HqlKeyword.GROUPBY ||
                            token.Keyword == HqlKeyword.HAVING ||
                            token.Keyword == HqlKeyword.ORDERBY
                        )
                    )
                {
                    break;
                }

                if (token.WordType == HqlWordType.KEYWORD &&
                        (
                            token.Keyword == HqlKeyword.FULL_OUTER ||
                            token.Keyword == HqlKeyword.LEFT_OUTER ||
                            token.Keyword == HqlKeyword.RIGHT_OUTER ||
                            token.Keyword == HqlKeyword.INNER
                        )
                    )
                {
                    joinType = token.Keyword;

                    processor.MoveNextToken();
                    token = processor.GetToken();
                    if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.JOIN)
                        throw new Exception("Expected JOIN keyword");
                    FileExpected = true;

                    processor.MoveNextToken();
                    token = processor.GetToken();
                }

                // else time to process it!
                if (!FileExpected)
                    break;

                FileExpected = false;

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STDIN)
                {
                    AddStdin();
                }
                else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.DUAL)
                {
                    AddDual();
                }
                else if (token.WordType == HqlWordType.TEXT || token.WordType == HqlWordType.LITERAL_STRING)
                {
                    AddFile(token.Data);
                }
                else if (token.WordType == HqlWordType.STREAM)
                {
                    HqlDataSource source = new HqlDataSource(HqlDataSourceType.STREAM, (System.IO.StreamReader)token.Parsed);
                    AddSource(source);
                }
                else if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.OPENPAREN)
                {
                    // NESTED QUERY!
                    HqlStream cs = new HqlStream(processor.GetRemainingSql(false), ")");
                    string remainingSql = cs.RemainingSql;
                    processor.SetRemainingSql(remainingSql);
                    processor.MoveNextToken();
                    token = processor.GetToken();
                    if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.CLOSEDPAREN)
                        throw new Exception("Nested query SQL did not return correctly");

                    AddTextReader(cs);
                }
                else
                {
                    throw new Exception("Unknown type of FROM clause");
                }

                // look to process SKIP
                for (;;)
                {
                    bool breakOut = false;

                    if (ReadNextToken)
                    {
                        processor.MoveNextToken();
                        token = processor.GetToken();
                    }

                    if (_sources != null && _sources.Length > 0 && (token.WordType == HqlWordType.UNKNOWN || token.WordType == HqlWordType.KEYWORD))
                    {
                        HqlToken option;
                        switch (token.Data.ToUpper())
                        {
                            case "SKIP":
                                {
                                    option = processor.GetOptionData(token.Data);
                                    if (option.WordType != HqlWordType.INT || option.ParsedAsInt <= 0)
                                        throw new Exception("Skip count must be greater than zero");
                                    CurrentSource.SkipRecords = option.ParsedAsInt;
                                    ReadNextToken = true;
                                    break;
                                }
                            case "DELIMITER":
                            case "DELIM":
                                {
                                    option = processor.GetOptionData(token.Data);
                                    if (option.WordType != HqlWordType.TEXT && option.WordType != HqlWordType.LITERAL_STRING)
                                        throw new Exception(String.Format("Expected a valid delimiter after {0}", token.Data));
                                    CurrentSource.Delimiter = HqlTokenProcessor.CleanupDelimiter(option.Data);
                                    ReadNextToken = true;
                                    break;
                                }
                            case "AS":
                                {
                                    option = processor.GetOptionData(token.Data);
                                    if (option.WordType != HqlWordType.UNKNOWN)
                                        throw new Exception("Expected a table reference name following AS");
                                    CurrentSource.TableReference = option.Data;
                                    break;
                                }
                            default:
                                breakOut = true;
                                break;

                        }
                    }
                    else
                    {
                        breakOut = true;
                    }
                    
                    if (breakOut)
                    {
                        ReadNextToken = false;
                        break;
                    }
                }


                if (joinType != HqlKeyword.UNKNOWN)
                {
                    HqlDataSource src = LeftSideOfJoin;
                    src.JoinType = joinType;

                    if (ReadNextToken)
                    {
                        processor.MoveNextToken();
                        token = processor.GetToken();
                    }

                    if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.ON)
                        throw new Exception("Expected ON following a table");

                    src.InitJoining();
                    src.OnJoin.Parse(processor, HqlKeyword.ON, HqlKeyword.ON, null);
                    ReadNextToken = false;
                }
            }

            if (FileExpected)
                throw new Exception("Expected additional FROM fields");
        }

        public HqlLine ReadLine()
        {
            HqlDataSource src = _sources[0];
            HqlLine l = src.ReadLine();
            return l;
        }

        public bool GetEndOfStream(int SourceNum)
        {
            if (_sources == null || SourceNum > _sources.Length)
                throw new Exception("ReadLine sourcenum is too large");
            return _sources[SourceNum].EndOfStream;
        }

        public void Cleanup()
        {
            if (_settings != null)
                _settings.Cleanup();
            for (int i = 0; i < _sources.Length; ++i)
            {
                _sources[i].Cleanup();
            }
        }

        public bool ContainsTableReference(string tableReference)
        {
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].HasTableReference && this[i].TableReference.Equals(tableReference))
                {
                    return true;
                }
            }
            return false;
        }

        ///////////////////////
        // Private

        private void AddFile(string name)
        {
            HqlDataSource source = new HqlDataSource(HqlDataSourceType.FILE, name);
            AddSource(source);
        }

        private void AddStdin()
        {
            HqlDataSource source = new HqlDataSource(HqlDataSourceType.STDIN, String.Empty);
            AddSource(source);
        }

        private void AddDual()
        {
            HqlDataSource source = new HqlDataSource(HqlDataSourceType.DUAL, String.Empty);
            AddSource(source);
        }

        private void AddTextReader(HqlStream stream)
        {
            HqlDataSource source = new HqlDataSource(HqlDataSourceType.HQLQUERY, stream);
            AddSource(source);
        }

        private void AddSource(HqlDataSource source)
        {
            if (_sources == null)
            {
                _sources = new HqlDataSource[1];
                _sources[0] = source;
            }
            else
            {
                HqlDataSource[] newfields = new HqlDataSource[_sources.Length + 1];
                for (int i = 0; i < _sources.Length; ++i)
                    newfields[i] = _sources[i];
                newfields[_sources.Length] = source;
                _sources = newfields;
            }
        }

        ///////////////////////
        // Fields

        public int Count
        {
            get
            {
                if (_sources == null) return 0;
                return _sources.Length;
            }
        }

        public bool EndOfStream
        {
            get { return GetEndOfStream(0); } 
        }

        public Int64 TotalRowNum
        {
            get
            {
                Int64 total = 0;
                for (int i = 0; i < _sources.Length; ++i)
                {
                    total += _sources[i].RowNum;
                }
                return total;
            }
        }

        public Int64 RowNum
        {
            get
            {
                return _sources[0].RowNum;
            }
        }

        private HqlDataSource CurrentSource
        {
            get
            {
                if (_sources.Length == 0)
                    throw new Exception("Undefined source");

                return _sources[_sources.Length - 1];
            }
        }

        private HqlDataSource LeftSideOfJoin
        {
            get
            {
                if (_sources.Length <= 1)
                    throw new Exception("Undefined source");

                return _sources[_sources.Length - 2];
            }
        }
        
        ///////////////////////
        // Getters/Setters

        public HqlDataSource this[int i]
        {
            get { return _sources[i]; }
        }

        ///////////////////////
        // Variables

        HqlDataSource[] _sources;
        HqlWith _settings;
    }
}
