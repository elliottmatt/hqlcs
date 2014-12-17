using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    enum HqlJoinMethod
    {
        LINEAR_COMPARE,
        SORT_COMPARE,
        RETURN_ALL
    };

    public enum HqlDataSourceType
    {
        FILE,
        WILDCARD_FILE,
        STDIN,
        DATABASE,
        DUAL,
        HQLQUERY,
        STREAM,
    }

    delegate void ProcessRow(HqlValues values);
    delegate bool HandleRow(HqlValues values);

    class HqlDataSource : IDisposable
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlDataSource(HqlDataSourceType type, string name)
        {
            Init(type);
            _name = name;
        }
        public HqlDataSource(HqlDataSourceType type, TextReader textreader)
        {
            Init(type);
            _textreader = textreader;
        }
        public HqlDataSource(HqlDataSourceType type, StreamReader streamreader)
        {
            Init(type);
            _reader = streamreader;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public HqlLine ReadLine()
        {
            string s = ReadLine_Internal();
            for (; SkipRecords > 0; )
            {
                SkipRecords = SkipRecords - 1;
                s = ReadLine_Internal();
            }
            HqlLine l = new HqlLine(s, Delimiter, RowNum, Name);
            return l;
        }

        public void Open()
        {
            if (_type == HqlDataSourceType.FILE)
            {
                if (_name.Contains("*") || _name.Contains("?"))
                {
                    _type = HqlDataSourceType.WILDCARD_FILE;
                    Open();
                    return;
                }

                try
                {
                    _reader = new StreamReader(_name);
                    _open = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Unable to open HqlDataSource::FILE::|{0}|", _name), ex);
                }
                finally
                {
                    _open = true;
                }
            }
            else if (_type == HqlDataSourceType.WILDCARD_FILE)
            {
                try
                {
                    string path = System.IO.Path.GetDirectoryName(_name);
                    if (path.Length == 0)
                        path = Directory.GetCurrentDirectory();
                    string filename = System.IO.Path.GetFileName(_name);
                    _filenames = Directory.GetFiles(path, filename, SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Unable to list files for HqlDataSource::WILDCARD_FILE::|{0}|", _name), ex);
                }
                finally
                {
                    _open = true;
                }

                OpenNext();
                return;
            }
            else if (_type == HqlDataSourceType.DUAL)
            {
                _open = true;
                _reader = StreamReader.Null;
            }
            else if (_type == HqlDataSourceType.STDIN)
            {
                _open = true;
                _textreader = Console.In;
            }
            else if (_type == HqlDataSourceType.HQLQUERY)
            {
                _open = true;
            }
            else if (_type == HqlDataSourceType.STREAM)
            {
                _open = true;
            }
            else
            {
                throw new Exception("Unimplemented HqlDataSource");
            }
        }

        public void InitJoining()
        {
            // Initialize this field for joining data to it!
            FieldsImpacted = new HqlFieldGroup();
            _onJoin = new HqlWhere(HqlWhere.NAME_ONJOIN);
            Lines = new List<HqlValues>();
        }

        public void LoadFieldReferences(HqlFieldGroup group)
        {
            if (group == null)
                return;

            for (int i = 0; i < group.Count; ++i)
            {
                LoadField(group[i]);
            }
        }

        public void LoadFieldReferences(ArrayList list)
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; ++i)
            {
                object o = list[i];
                if (o is HqlCompareToken)
                {
                    HqlCompareToken comp = (HqlCompareToken)o;
                    LoadToken(comp.Token1);
                    LoadToken(comp.Token2);
                    LoadFieldReferences(comp.InValues);
                }
                else if (o is HqlToken)
                {
                    LoadToken((HqlToken)o);
                }
                else if (o is HqlField)
                {
                    LoadField((HqlField)o);
                }
                else
                {
                    throw new Exception(String.Format("Error, what sort of other data can be found in HqlDataSource:LoadFieldReferences|{0}|", o.GetType()));
                }
            }
        }

        //public HqlValues MergeValues(HqlValues priorvalues, HqlValues thisvalues, ref HqlFieldGroup combinedFields)
        //{
        //    if (combinedFields == null)
        //        combinedFields = new HqlFieldGroup(priorvalues.FieldsImpacted, thisvalues.FieldsImpacted);
        //    return new HqlValues(combinedFields, priorvalues, thisvalues);
        //}

        public bool ReadThroughUsingJoin(HqlDataSource prior, HqlWhere onJoin, HqlWhere where, bool selectRecord)
        {
            // TODO
            // - need to sort the prior how I'm going to join these.
            // - read through current and join them and save to new object
            // - clear all the saved data from the prior object
            //HqlFieldGroup combinedFieldsImpacted = null;
            //HqlValuesComparer comparer = prior.comparer;

            // TODO, sort the prior
            //bool sortedPrior = false;

            for (; ; )
            {
                if (selectRecord && Lines.Count > 0)
                {
                    _resultValues = Lines[0];
                    Lines.RemoveAt(0);

                    if (where.Evaluate(_resultValues))
                    {
                        return true;
                    }
                    continue;
                }

                if (EndOfStream)
                {
                    if (!_completedJoin)
                    {
                        _completedJoin = true;

                        if (prior != null && (prior.JoinType == HqlKeyword.LEFT_OUTER || prior.JoinType == HqlKeyword.FULL_OUTER))
                        {
                            prior.Comparer.JoinMethod = HqlJoinMethod.RETURN_ALL;
                            prior.Comparer.Reset();
                            prior.comparer.SetValues(BlankValues);

                            for (; prior.Comparer.MoveNext(); )
                            {
                                prior.Comparer.Current.IncreaseUsedCount();
                                HqlValues combinedvalues = prior.Comparer.CombinedValues;
                                SaveRowValue(combinedvalues);
                            }

                            continue;
                        }
                    }

                    _resultValues = null;
                    return false;
                }

                //string fromline = ReadLine();
                //if (fromline.Equals(String.Empty) && EndOfStream)
                //    continue;
                //HqlLine line = new HqlLine(fromline, settings, RowNum)
                HqlLine line = ReadLine();
                if (line.IsBlank && EndOfStream)
                    continue;

#if DEBUG
                if (line.Line.StartsWith("1503|706"))
                {
                    int stop = 0;
                }
#endif

                // get the current line of data
                HqlValues thisvalues = new HqlValues(FieldsImpacted, line);

                // does this match the where statement?
                if (JoinType == HqlKeyword.LEFT_OUTER || JoinType == HqlKeyword.FULL_OUTER)
                {
                    // pass through, don't evaluate the ON command because I want this regardless
                }
                else if (prior != null && (prior.JoinType == HqlKeyword.FULL_OUTER || prior.JoinType == HqlKeyword.RIGHT_OUTER))
                {
                    // pass through
                }
                else if (!onJoin.EvaluateJoin(thisvalues, true)) // || !where.EvaluateJoin(thisvalues))
                {
                    // didn't pass through, so continue
                    continue;
                }
                
                // This is here for the first "join"
                // I need to save all the records.
                if (prior == null)
                {
                    SaveRowValue(thisvalues);
                    continue;
                }

                // now time to join the data!
                bool savedThisRecord = false;

                // do preprocessing if this is to be searched through sorting
                if (prior.Comparer == null)
                {
                    if (prior.Lines.Count == 0)
                        throw new Exception("Data integrity issue. Didn't find anything to join with on prior! TODO, this can be valid, need to fix!!");

                    HqlValues priorvalues = prior.Lines[0]; // guaranteed to be there
                    prior.Comparer = new HqlValuesComparer(prior, onJoin, where); //, settings); 
                    prior.Comparer.CombinedFieldsImpacted = new HqlFieldGroup(priorvalues.FieldsImpacted, thisvalues.FieldsImpacted);
                    
                    if (prior.Comparer.CountOfJoinFields == 0)
                    {
                        prior.Comparer.JoinMethod = HqlJoinMethod.LINEAR_COMPARE;
                    }

                    Console.Error.WriteLine(String.Format("Using joining of {0} between ({1}) {2} and ({3}) {4}",  
                        prior.Comparer.JoinMethod.ToString(),
                        prior.TableReference,
                        prior.SourceName,
                        this.TableReference,
                        this.SourceName)
                        );

                    if (prior.Comparer.JoinMethod == HqlJoinMethod.SORT_COMPARE)
                    {
                        prior.Comparer.SortPrior();
                    }
                }

                prior.Comparer.SetValues(thisvalues);
                prior.Comparer.Reset();

                for (; prior.Comparer.MoveNext(); )
                {
                    prior.Comparer.Current.IncreaseUsedCount();

                    HqlValues combinedvalues = prior.Comparer.CombinedValues;

                    SaveRowValue(combinedvalues);
                    savedThisRecord = true;
                }

                if (!savedThisRecord && (prior.JoinType == HqlKeyword.RIGHT_OUTER || prior.JoinType == HqlKeyword.FULL_OUTER))
                {
                    HqlValues combinedvalues = new HqlValues(prior.Comparer.CombinedFieldsImpacted, prior.BlankValues, thisvalues);
                    SaveRowValue(combinedvalues);
                }
            }
        }

        public void DropLoadedData()
        {
            if (_lines != null)
            {
                _lines.Clear();
                _lines = null;
            }
        }

        public void BuildBlankValues()
        {
            string[] blankStrings = new string[_fieldsImpacted.Count];
            for (int i = 0; i < _fieldsImpacted.Count; ++i)
                blankStrings[i] = String.Empty;
            _blankValues = new HqlValues(_fieldsImpacted, blankStrings);
        }

        public void Cleanup()
        {
            DropLoadedData();
            _filenames = null;

            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }
            if (_textreader != null)
            {
                _textreader.Close();
                _textreader.Dispose();
                _textreader = null;
            }            
            if (_resultValues != null)
            {
                _resultValues.Cleanup();
                _resultValues = null;
            }
            if (_blankValues != null)
            {
                _blankValues.Cleanup();
                _blankValues = null;
            }
            if (_onJoin != null)
            {
                _onJoin.Cleanup();
                _onJoin = null;
            }
            if (_fieldsImpacted != null)
            {
                _fieldsImpacted.Cleanup();
                _fieldsImpacted = null;
            }
        }

        void IDisposable.Dispose()
        {
            Cleanup();
        }

        ///////////////////////
        // Private

        private string ReadLine_Internal()
        {
            if (!_open)
                Open();

            if (_reader != null)
            {
                string s = _reader.ReadLine();
                if (s == null)
                    s = String.Empty;
                _rows++;
                return s;
            }
            else if (_textreader != null)
            {
                string s = _textreader.ReadLine();
                if (s == null)
                {
                    s = String.Empty;
                    _textreaderEOF = true;
                }
                //else if (s.Contains("\x04"))
                //{
                //    if (s.Length == 1)
                //        s = String.Empty;
                //    _textreaderEOF = true;
                //}
                _rows++;
                return s;
            }
            else
            {
                throw new Exception("Unexpected type of input!");
            }
        }

        private void SaveRowValue(HqlValues values)
        {
            Lines.Add(values);
        }

        private void LoadToken(HqlToken token)
        {
            if (token == null)
                return;

            switch (token.WordType)
            {
                case HqlWordType.FIELD:
                case HqlWordType.SCALAR:
                    LoadField(token.Field);
                    break;
                case HqlWordType.KEYWORD:
                case HqlWordType.FLOAT:                
                case HqlWordType.INT:
                case HqlWordType.LITERAL_STRING:
                case HqlWordType.TEXT:
                case HqlWordType.NULL:
                    // don't cares
                    break;
                case HqlWordType.FUNCTION:
                default:
                    throw new Exception(String.Format("Error. what sort of other data can be found in HqlDataSource:LoadToken|{0}|", token.WordType));
            }
        }

        private void LoadField(HqlField field)
        {
            if (field == null)
                return;

            if (!field.ContainsTableReference(TableReference))
                return;

            if (!FieldsImpacted.ContainsField(field))
                FieldsImpacted.AddField(field);
            if (field.FieldType == HqlFieldType.FUNCTION)
            {
                LoadField(field.Func.Field);
            }
            else if (field.FieldType == HqlFieldType.SCALAR)
            {
                HqlScalar scalar = field.Scalar;
                if (scalar.HasMultipleFields)
                {
                    for (int i = 0; i < scalar.Fields.Length; ++i)
                    {
                        LoadField(scalar.Fields[i]);
                    }
                }
                else
                {
                    LoadField(scalar.Field);
                }
            }
        }

        private void OpenNext()
        {
            try
            {
                if (_reader != null)
                {
                    _filenames_counter++;
                    _reader.Close();
                    _reader.Dispose();
                    _reader = null;
                }
                
                if (_filenames_counter >= _filenames.Length)
                {
                    _reader = null;
                    return;
                }
#if DEBUG && false
                Console.Error.WriteLine(String.Format("Debug: OpenNext: Opening filename |{0}|", _filenames[_filenames_counter]));
#endif
                _reader = new StreamReader(_filenames[_filenames_counter]);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Unable to open HqlDataSource::WILDCARD_FILE::|{0}|", _filenames[_filenames_counter]), ex);
            }
        }

        private void Init(HqlDataSourceType type)
        {
            _type = type;
            _open = false;
            _rows = 0;
            _textreaderEOF = false;
            _tableReference = null;
            _joinType = HqlKeyword.UNKNOWN;
            _skipRecords = 0;
            
            _completedJoin = false;
            //_comp
            _onJoin = null;
            _delim = null;
            //FieldsImpacted = null;
        }

        ///////////////////////
        // Fields

        public bool EndOfStream
        {
            get
            {
                if (!_open)
                    Open();
                if (_type == HqlDataSourceType.FILE)
                {
                    if (_reader == null)
                        return true;
                    return _reader.EndOfStream;
                }
                else if (_type == HqlDataSourceType.WILDCARD_FILE)
                {
                    if (_reader == null)
                        return true;
                    if (_reader.EndOfStream)
                    {
                        OpenNext();
                        return EndOfStream;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (_type == HqlDataSourceType.DUAL)
                {
                    if (_rows > 1)
                        return true;
                    return false;
                }
                else if (_type == HqlDataSourceType.STDIN)
                {
                    //if (_textreader.Peek() == -1)
                    //  return true;
                    if (_textreaderEOF)
                        return true;
                    return false;
                }
                else if (_type == HqlDataSourceType.HQLQUERY)
                {
                    if (_textreader == null)
                        return true;
                    return (_textreader.Peek() == -1);
                }
                else if (_type == HqlDataSourceType.STREAM)
                {
                    if (_reader == null)
                        return true;
                    return _reader.EndOfStream;
                }
                else
                {
                    throw new Exception("Unknown type of DataSource");
                }
            }
        }

        ///////////////////////
        // Getters/Setters

        public bool HasTableReference
        {
            get { return (_tableReference != null); }
        }

        public string TableReference
        {
            get { return _tableReference; }
            set { _tableReference = value; }
        }

        public HqlWhere OnJoin
        {
            get { return _onJoin; }
            //set { _onJoin = value; }
        }

        internal List<HqlValues> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }

        internal HqlFieldGroup FieldsImpacted
        {
            get { return _fieldsImpacted; }
            set { _fieldsImpacted = value; }
        }

        public HqlValues ResultValues
        {
            get { return _resultValues; }
        }

        public HqlKeyword JoinType
        {
            get { return _joinType; }
            set { _joinType = value; }
        }

        public HqlValues BlankValues
        {
            get { return _blankValues; }
        }

        public Int64 RowNum
        {
            get { return _rows; }
        }

        public HqlValuesComparer Comparer
        {
            get { return comparer; }
            set { comparer = value; }
        }

        public string SourceName
        {
            get { return _name; }
        }

        public int SkipRecords
        {
            get { return _skipRecords; }
            set { _skipRecords = value; }
        }

        public string Name
        {
            get
            {
                switch (_type)
                {
                    case HqlDataSourceType.FILE:
                        return _name;
                    case HqlDataSourceType.WILDCARD_FILE:
                        return _filenames[_filenames_counter];
                    case HqlDataSourceType.DUAL:
                        return "dual";
                    case HqlDataSourceType.STDIN:
                        return "stdin";
                    case HqlDataSourceType.HQLQUERY:
                        return "subquery";
                    case HqlDataSourceType.STREAM:
                        return "stream";
                    default:
                        throw new InvalidDataException("Unknown type of DataSourceType");
                }
            }
        }

        /// <summary>
        /// Only save the first settings given to you
        /// </summary>
        public HqlWith Settings
        {
            //get { return _settings; }
            set
            {
                if (_settings == null)
                {
                    _settings = value;
                }
            }
        }

        public string Delimiter
        {
            get
            {
                return (_delim == null) ? _settings.InDelimiter : _delim;
            }
            set { _delim = value; }
        }

        ///////////////////////
        // Variables

        string _tableReference;
        HqlDataSourceType _type;
        string _name;
        bool _open;
        StreamReader _reader;
        TextReader _textreader;
        bool _textreaderEOF;
        Int64 _rows;
        string[] _filenames;
        int _filenames_counter;
        HqlValues _resultValues; // current result line
        bool _completedJoin;
        HqlValues _blankValues;
        int _skipRecords;
        string _delim;
        HqlWith _settings;

        // for joining
        HqlKeyword _joinType;
        HqlWhere _onJoin;
        HqlFieldGroup _fieldsImpacted;
        //HqlTable _table;
        List<HqlValues> _lines;
        HqlValuesComparer comparer;
    }
}
