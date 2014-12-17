using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlWith
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlWith()
        {
            _inDelimiter = "|";
            _outDelimiter = "|";
            //_expectedEndOfQuery = null; // unnecessary
            _sampleRows = null;

            _hasFinalDelimiter = false;
            _outputFilename = null;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public void Parse(HqlTokenProcessor processor)
        {
            HqlToken token;
            HqlToken option;

            // first thing should be from
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.WITH)
                return; // throw new Exception("Expected WITH");

            // now pull back things until you get to EOF
            for (; ; )
            {
                processor.MoveNextToken();
                token = processor.GetToken();
                if (processor.MatchesEndOfProcessing())
                    break;

                if (
                    token.WordType == HqlWordType.UNKNOWN
                    ||
                    token.WordType == HqlWordType.KEYWORD
                    )
                {
                    switch (token.Data.ToUpper())
                    {
                        case "HEADER":
                            {
                                PrintHeader = true;
                                break;
                            }
                        case "PQ":
                        case "PRESERVE-QUOTES":
                        case "PRESERVE_QUOTES":
                            {
                                // TODO, not implemented
                                PreserveQuotes = true;
                                break;
                            }
                        case "TEMPDIR":
                            {
                                option = processor.GetOptionData(token.Data);
                                if (option.WordType != HqlWordType.TEXT && option.WordType != HqlWordType.LITERAL_STRING)
                                    throw new Exception(String.Format("Expected a valid directory after {0}", token.Data));
                                // TODO, save directory
                                break;
                            }
                        case "OD":
                        case "OUT-DELIMITER":
                        case "OUT_DELIMITER":
                            {
                                option = processor.GetOptionData(token.Data);
                                if (option.WordType != HqlWordType.TEXT && option.WordType != HqlWordType.LITERAL_STRING)
                                    throw new Exception(String.Format("Expected a valid delimiter after {0}", token.Data));
                                OutDelimiter = HqlTokenProcessor.CleanupDelimiter(option.Data);
                                break;
                            }
                        case "D":
                        case "DELIM":
                        case "DELIMITER":
                            {
                                option = processor.GetOptionData(token.Data);
                                if (option.WordType != HqlWordType.TEXT && option.WordType != HqlWordType.LITERAL_STRING)
                                    throw new Exception(String.Format("Expected a valid delimiter after {0}", token.Data));
                                InDelimiter = HqlTokenProcessor.CleanupDelimiter(option.Data);
                                break;
                            }
                        case "PFD":
                        case "PRINT-FINAL-DELIMITER":
                        case "PRINT_FINAL_DELIMITER":
                            {
                                HasFinalDelimiter = true;
                                break;
                            }

                        case "OUTPUT":
                            {
                                option = processor.GetOptionData(token.Data);
                                if (option.WordType != HqlWordType.TEXT && option.WordType != HqlWordType.LITERAL_STRING)
                                    throw new Exception(String.Format("Expected a filename after {0}", token.Data));
                                OutputFilename = option.Data;
                                ProcessOutFilename(processor);
                                break;
                            }
                        //case "SKIP":
                        //    {
                        //        option = GetOptionData(processor, token.Data);
                        //        if (option.WordType != HqlWordType.INT || (int)option.Parsed <= 0)
                        //            throw new Exception(String.Format("Expected a greater than zero integer after {0}", token.Data));
                        //        SkipRecords = (Int32)(Int64)option.Parsed;
                        //        break;
                        //    }
                        default:
                            throw new Exception(String.Format("Unknown WITH option of {0}", token.Data.ToString()));
                    }
                    
                }
                else
                {
                    throw new Exception("Unknown type in WITH clause");
                }
            }
        }

        public void Cleanup()
        {
            if (_output != null)
            {
                _output.Cleanup();
                _output = null;
            }
        }

        ///////////////////////
        // Private

        private void ProcessOutFilename(HqlTokenProcessor processor)
        {
            if (OutputFilename == null || OutputFilename.Length == 0)
                throw new Exception("Output filename cannot be empty");
            if (OutputFilename.Contains("{"))
            {
                _output = new HqlOutput(OutputFilename, this);

                int currloc = 0;
                int startloc = 0;
                for (; ; )
                {
                    bool FoundEndBrace = false;
                    currloc = OutputFilename.IndexOf('{', startloc);
                    if (currloc < 0 || currloc >= OutputFilename.Length)
                        break;
                    startloc = currloc;
                    string innerField = String.Empty;
                    for (currloc++; currloc < OutputFilename.Length; currloc++)
                    {
                        if (OutputFilename[currloc] == '}')
                        {
                            // this means that in the string was {} because there is no gap between them for characters
                            if (startloc + 1 == currloc)
                            {
                                throw new Exception(String.Format("Cannot have an empty field in OUTPUT name of |{0}|", OutputFilename));
                            }

                            startloc = currloc;
                            FoundEndBrace = true;
                            break;
                        }
                        innerField += OutputFilename[currloc];
                    }
                    if (!FoundEndBrace)
                    {
                        throw new Exception(String.Format("Need a closing brace in filename |{0}|", OutputFilename));
                    }

                    HqlTokenProcessor newProcessor = new HqlTokenProcessor(innerField, processor);
                    newProcessor.MoveNextToken();
                    HqlToken token = newProcessor.GetToken();
                    if (token.WordType == HqlWordType.UNKNOWN)
                        newProcessor.CheckForTokenReference(ref token);
                    if (token.WordType != HqlWordType.FIELD && token.WordType != HqlWordType.SCALAR)
                        throw new Exception("Info in braces is not a field");
                    token.Field.FieldRename = innerField; // set the original name of the field for the {replace}
                    _output.FieldGroup.AddField(token.Field);
                }
            }
        }

        ///////////////////////
        // Fields

        public bool PrintsToFile
        {
            get { return (_outputFilename != null); }
        }

        ///////////////////////
        // Getters/Setters
        
        public string ExpectedEndOfQuery
        {
            get
            {
                if (_expectedEndOfQuery == null) { return String.Empty; }
                return _expectedEndOfQuery; 
            }
            set { _expectedEndOfQuery = value; }
        }

        public string InDelimiter
        {
            get { return _inDelimiter; }
            set { _inDelimiter = value; }
        }

        public string OutDelimiter
        {
            get { return _outDelimiter; }
            set { _outDelimiter = value; }
        }

        public int SampleRows
        {
            get { return _sampleRows.Value; }
            set { _sampleRows = value; }
        }

        public bool HasSampleRows
        {
            get { return _sampleRows.HasValue; }
        }

        public bool HasFinalDelimiter
        {
            get { return _hasFinalDelimiter; }
            set { _hasFinalDelimiter = value; }
        }

        public string FinalDelimiter
        {
            get { return (HasFinalDelimiter ? OutDelimiter : String.Empty); }
        }

        public bool PrintHeader
        {
            get { return _printHeader; }
            set { _printHeader = value; }
        }

        public string OutputFilename
        {
            get { return _outputFilename; }
            set { _outputFilename = value; }
        }

        public bool PrintCategorizeFilename
        {
            get { return (_output != null); }
        }

        public HqlOutput Output
        {
            get { return _output; }
        }

        public bool PreserveQuotes
        {
            get { return _preserveQuotes; }
            set { _preserveQuotes = value; }
        }

        //public int SkipRecords
        //{
        //    get { return _skipRecords; }
        //    set { _skipRecords = value; }
        //}

        ///////////////////////
        // Variables
        bool _hasFinalDelimiter;

        string _expectedEndOfQuery;
        string _inDelimiter;
        string _outDelimiter;
        int? _sampleRows;
        //int _skipRecords;

        bool _printHeader;
        string _outputFilename;
        bool _preserveQuotes;

        HqlOutput _output;
    }
}
