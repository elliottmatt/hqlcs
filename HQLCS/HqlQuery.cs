using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlQuery
    {
        ///////////////////////
        // Static Functions

        private static object[] CreateRecordKey(HqlRecord record, HqlOrderBy order, HqlClause group, HqlClause output, int linenum)
        {
            int orderlength = order.Count;
            int grouplength = (group.Count == 0) ? 1: group.Count;
            int outputlength = ((output == null) ? 0 : output.Count);
            
            int length = orderlength + grouplength + outputlength;
            object[] keys = new object[length];

            object[] orderarray = order.GetKeyValues(record);
            object[] grouparray;
            if (group.Count > 0)
                grouparray = group.GetValues(record);
            else
                grouparray = new object[1] { linenum };
            object[] outputarray = ((output == null) ? null : output.GetValues(record));

            int counter = 0;

            orderarray.CopyTo(keys, counter);
            counter += orderarray.Length;

            grouparray.CopyTo(keys, counter);
            counter += grouparray.Length;

            if (outputarray != null)
            {
                outputarray.CopyTo(keys, counter);
                //counter += outputarray.Length;
            }

            return keys;
        }

        private static HqlResultRow CreateObjectArrayValue(HqlClause order, HqlClause select, HqlClause output)
        {
            HqlResultRow row = new HqlResultRow(order.FieldGroup, select.FieldGroup, ( (output == null) ? null : output.FieldGroup ));
            return row;
        }

        private static void CloseAllFiles(Dictionary<HqlKey, HqlOutFile> dic)
        {
            if (dic == null)
                return;

            foreach (System.Collections.Generic.KeyValuePair<HqlKey, HqlOutFile> d in dic)
            {
                if (d.Value.IsOpen)
                {
                    d.Value.Close();
                }
            }
        }
        
        ///////////////////////
        // Constructors

        public HqlQuery(string sql, string ExpectedEndOfQuery)
        {
            Init(sql, ExpectedEndOfQuery);
        }

        public HqlQuery(string sql, object[] references, string ExpectedEndOfQuery)
        {
            _references = references;
            Init(sql, ExpectedEndOfQuery);
        }

        public void Cleanup()
        {
            if (_select != null) { _select.Cleanup(); }
            if (_from != null) { _from.Cleanup(); }
            if (_where != null) { _where.Cleanup(); }
            if (_groupby != null) { _groupby.Cleanup(); }
            if (_orderby != null) { _orderby.Cleanup(); }
            if (_having != null) { _having.Cleanup(); }
            if (_settings != null) { _settings.Cleanup(); }
            
            _references = null;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public string ReadLine()
        {
            if (EndOfStream)
                return String.Empty;

            if (_settings.PrintsToFile)
            {
                RunQueryToOutput();
                EndOfStream = true;
                return ReadLine(); // incase i want to do something on EndOfStream I only have to do it once above
            }
            
            if (!GetLine())
            {
                EndOfStream = true;
                return ReadLine(); // incase i want to do something on EndOfStream I only have to do it once above
            }

            return _line;
        }
        
        public void Compile_Inner(HqlTokenProcessor processor)
        {
            // find phrase select
            _select.Parse(processor);

            // find phrase from
            _from.Parse(processor);
            if (processor.MatchesEndOfProcessing())
                return;

            // find phrase where
            _where.Parse(processor);
            if (processor.MatchesEndOfProcessing())
                return;

#if DEBUG && false
                Console.WriteLine("Debug: Where START::");
                Console.Write(_where.ToString());
                Console.WriteLine("Debug: Where END::");
#endif
            // find phrase group by
            _groupby.Parse(processor);
            if (processor.MatchesEndOfProcessing())
                return;

            // find phrase having
            _having.Parse(processor);
            if (processor.MatchesEndOfProcessing())
                return;

            // find phrase order by
            _orderby.Parse(processor);
            if (processor.MatchesEndOfProcessing())
                return;

            // find phrase order by
            _settings.Parse(processor);
        }

        public void Compile()
        {
            try
            {
                _select = new HqlSelect(_settings);
                _from = new HqlFrom();
                _where = new HqlWhere(HqlWhere.NAME_WHERE);
                _groupby = new HqlGroupBy();
                _having = new HqlHaving(_select); 
                _orderby = new HqlOrderBy();                

                HqlTokenProcessor processor = new HqlTokenProcessor(_sql, _settings, _references);

                Compile_Inner(processor);
                
                PostCompile(processor);
            }
            catch
            {
                throw;
            }
        }
       
        ///////////////////////
        // Private

        private void Init(string sql, string expectedEndOfQuery)
        {
            _sql = sql;
            _endOfStream = false;
            _select = null;
            _linenum = 0;
            _successCompiled = false;
            _ptr = null;
            _successCalculate = false;
            _loadedFirstFiles = false;
            _initializedForStreaming = false;

            _settings = new HqlWith();
            _expectedEndOfQuery = expectedEndOfQuery;
            _settings.ExpectedEndOfQuery = expectedEndOfQuery;
        }

        private void PostCompile(HqlTokenProcessor processor)
        {
            _select.VerifyFieldsPresent();
            _from.PostCompile(_settings);

            _sqlRemainingAfterCompile = processor.GetRemainingSql(true);

            // if string is not empty and sql remaining doesnt start with it...
            // or if string is empty and sql remaining is NOT empty...
            if (_expectedEndOfQuery.Length > 0 && !_sqlRemainingAfterCompile.StartsWith(_expectedEndOfQuery, StringComparison.CurrentCulture))
            {
                //error
                throw new Exception(String.Format("Expected the SQL to end with |{0}|, unknown remaining line", _expectedEndOfQuery));
            }
            else if (_expectedEndOfQuery.Length == 0 && _sqlRemainingAfterCompile.Length > 0)
            {
                //error
                throw new Exception(String.Format("Expected the remaining SQL to be empty, unknown remaining line of |{0}|", _sqlRemainingAfterCompile));
            }
            else
            {
                // no error
            }

            SanityCheck();
            Optimize1();
            _successCompiled = true;
        }

        private void Optimize1()
        {
            // check for any count(*) and save as count('*') with a literal because it doesn't really matter what the value is
            for (int i = 0; i < _select.Count; ++i)
            {
                HqlField f = _select.FieldGroup[i];
                for (; f.HasFunction; f = f.Func.Field)
                {
                    if (f.HasFunction && f.Func.FuncType == HqlFunctionType.COUNT && f.Func.Field.FieldType == HqlFieldType.STAR)
                    {
                        f.Func.Field = new HqlField(HqlFieldType.LITERAL_STRING, "*");
                    }
                }
            }

            for (int i = 0; i < _orderby.Count; ++i)
            {
                HqlField f = _orderby.FieldGroup[i];
                for (; f.HasFunction; f = f.Func.Field)
                {
                    if (f.HasFunction && f.Func.FuncType == HqlFunctionType.COUNT && f.Func.Field.FieldType == HqlFieldType.STAR)
                    {
                        f.Func.Field = new HqlField(HqlFieldType.LITERAL_STRING, "*");
                    }
                }
            }

            for (int i = 0; i < _groupby.Count; ++i)
            {
                HqlField f = _groupby.FieldGroup[i];
                for (; f.HasFunction; f = f.Func.Field)
                {
                    if (f.HasFunction && f.Func.FuncType == HqlFunctionType.COUNT && f.Func.Field.FieldType == HqlFieldType.STAR)
                    {
                        f.Func.Field = new HqlField(HqlFieldType.LITERAL_STRING, "*");
                    }
                }
            }

            for (int i = 0; _settings.Output != null && i < _settings.Output.Count; ++i)
            {
                HqlField f = _settings.Output.FieldGroup[i];
                for (; f.HasFunction; f = f.Func.Field)
                {
                    if (f.HasFunction && f.Func.FuncType == HqlFunctionType.COUNT && f.Func.Field.FieldType == HqlFieldType.STAR)
                    {
                        f.Func.Field = new HqlField(HqlFieldType.LITERAL_STRING, "*");
                    }
                }
            }
        }

        static private void CheckFieldIntoGroupBy(HqlGroupBy groupby, HqlField f, string type, HqlField entireField)
        {
            if (f.HasFunction)
                return;
            if (f.IsLiteralType)
                return;
            if (!f.PrintResult)
                return;

            if (groupby.ContainsField(f))
                return;

            if (!f.HasScalar)
            {
                throw new Exception(String.Format("Expected the {0} value of {1} in the GROUP-BY", type, f.GetFullName()));
            }
            else if (f.HasScalar)
            {
                if (f.Scalar.HasMultipleFields)
                {
                    HqlField[] fields = f.Scalar.Fields;
                    if (fields == null)
                        throw new Exception(String.Format("Expected the {0} value of {1} in the GROUP-BY", type, f.GetFullName()));

                    for (int j = 0; j < fields.Length; j++)
                    {
                        HqlField ff = fields[j];

                        CheckFieldIntoGroupBy(groupby, ff, type, entireField);
                    }
                }
                else
                {
                    f = f.FinalField;

                    if (f.IsLiteralType)
                        return;

                    if (!groupby.ContainsField(f))
                        throw new Exception(String.Format("Expected the {0} value of {1} in the GROUP-BY", type, f.GetFullName()));
                }
            }
            else
            {
                throw new Exception("Unknown case of FIELD");
            }
        }

        private void SanityCheckSelectGroupbyFields()
        {
            // if groupby is used, then for every non-function field in select, it must be in the group by
            if (_groupby.Count > 0 || _select.HasFunctions)
            {
                for (int i = 0; i < _select.Count; ++i)
                {
                    HqlField f = _select.FieldGroup[i];
                    
                    CheckFieldIntoGroupBy(_groupby, f, "SELECT", f);
                }
            }
        }

        private void SanityCheckOrderbyGroupbyFields()
        {
            // if groupby is used, then for every non-function field in order, it must be in the group by
            if (_groupby.Count > 0 || _orderby.HasFunctions)
            {
                for (int i = 0; i < _orderby.Count; ++i)
                {
                    HqlField f = _orderby.FieldGroup[i];

                    CheckFieldIntoGroupBy(_groupby, f, "ORDER-BY", f);
                }
            }
        }

        private void SanityCheckOutputGroupbyFields()
        {
            // if groupby is used, then for every non-function field in select, it must be in the group by
            if (_groupby.Count > 0 || (_settings.Output != null && _settings.Output.HasFunctions))
            {
                for (int i = 0; _settings.Output != null && i < _settings.Output.Count; ++i)
                {
                    HqlField f = _settings.Output.FieldGroup[i];

                    CheckFieldIntoGroupBy(_groupby, f, "OUTPUT", f);
                }
            }
        }

        private void SanityCheckHaving()
        {
            if (_having.Count == 0)
                return;

            if (_groupby.Count == 0)
                throw new Exception("Expected GROUP-BY if HAVING is present");
            
            for (int i = 0; i < _having.Count; ++i)
            {
                object o = _having.EvaluationCriteria[i];
                if (o is HqlToken)
                    continue;
                else if (o is HqlCompareToken)
                {
                    HqlCompareToken ct = (HqlCompareToken)o;
                    if (!ct.ExistsIn(_select.FieldGroup))
                        throw new Exception("Unable to find value from HAVING in the SELECT");
                }
                else
                {
                    throw new Exception("Unable to find value from HAVING in the SELECT");
                }
            }
        }

        private void SanityCheckSingleFunction()
        {
            if (_groupby.Count == 0 && _select.Count > 0)
            {
                int countFunctions = 0;
                for (int i = 0; i < _select.Count; ++i)
                {
                    if (!_select.FieldGroup[i].PrintResult)
                        continue;

                    if (_select.FieldGroup[i].HasFunction)
                        countFunctions++;
                }
                if (countFunctions == _select.CountPrintedFields)
                {
                    if (OneDataSource && HasFunctions && HasOrderBy && !HasGroupBy)
                    {
                        // TODO, one day possibly do this, but it's kind of a dumb one!
                        //SELECT sum(field1) FROM 'a.a' ORDER BY field1
                        //SELECT len(sum(field1)) FROM 'a.a' ORDER BY field1
                        //SELECT sum(len(field1)) FROM 'a.a' ORDER BY field1
                        //SELECT sum(field1) FROM 'a.a' WHERE field1 > 0 ORDER BY field1
                        //SELECT len(sum(field1)) FROM 'a.a' WHERE field1 > 0 ORDER BY field1
                        //SELECT sum(len(field1)) FROM 'a.a' WHERE field1 > 0 ORDER BY field1
                        //SELECT sum(field1), len(sum(field1)) FROM 'a.a' ORDER BY field1
                        //SELECT sum(field1), sum(len(field1)) FROM 'a.a' ORDER BY field1
                        //SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a' ORDER BY field1
                        //throw new Exception("Refusing to process because result doesn't make sense");
                        _orderby.ClearFields();
                    }

                    // means i have one or more functions, no other field selects, and no group by
                    _groupby.FieldGroup.AddField(new HqlField(HqlFieldType.LITERAL_STRING, "_"));
                }
            }
        }

        static private void SanityCheckTableReferences(HqlClause clause, HqlFrom from)
        {
            if (clause == null || clause.FieldGroup == null)
                return;

            for (int i = 0; i < clause.FieldGroup.Count; ++i)
            {
                if (clause.FieldGroup[i].HasTableReference)
                {
                    if (!from.ContainsTableReference(clause.FieldGroup[i].TableReference))
                        throw new Exception(String.Format("Unable to find table reference for |{0}| in the FROM clause", clause.FieldGroup[i].TableReference));
                }
            }
        }

        private void SanityCheckTableReferences()
        {
            SanityCheckTableReferences(_select, _from);
            SanityCheckTableReferences(_groupby, _from);
            SanityCheckTableReferences(_orderby, _from);
            if (_settings.Output != null)
                SanityCheckTableReferences(_settings.Output, _from);
        }

        private void SanityCheckRownum()
        {
            for (int i = 0; i < _from.Count; ++i)
            {
                HqlDataSource src = _from[i];
                if (src.OnJoin != null && src.OnJoin.ContainsRownum(true))
                    throw new Exception("Cannot have a \"rownum\" without a table in \"ON Clause\" due to complexity");
            }                    
        }

        private void SanityCheck()
        {
            SanityCheckSelectGroupbyFields(); 
            SanityCheckOrderbyGroupbyFields();
            SanityCheckOutputGroupbyFields();
            SanityCheckHaving();

            SanityCheckSingleFunction();

            SanityCheckTableReferences();

            SanityCheckRownum();
        }

        private void CreateKeyAndValue(HqlRecord record)
        {
            HqlKey KEY = null;

            object[] keys = CreateRecordKey(record, _orderby, _groupby, _settings.Output, _linenum);
            KEY = new HqlKey(keys);

            HqlResultRow row = _groupby.FindResultRow(KEY);
            if (row == null)
                row = CreateObjectArrayValue(_orderby, _select, _settings.Output);
#if DEBUG
            else
            {
                int stop = 0;
            }
#endif

            // set the values on the row
            int counter = 0;

            _orderby.AddToResultRow(counter, record, row);
            counter += _orderby.Count;

            _select.AddToResultRow(counter, record, row);
            counter += _select.Count;

            if (_settings.PrintCategorizeFilename)
            {
                _settings.Output.AddToResultRow(counter, record, row);
                //counter += _output.Count;
            }

            if (!row.ValuesSet)
            {
                row.ValuesSet = true;
                _groupby.SetResultRow(KEY, row);
            }
        }

        private void CalculateData()
        {
            for (; ; _linenum++)
            {
                if (_from.EndOfStream)
                {
                    break;
                }

                //string fromline = _from.ReadLine();
                //if (fromline.Equals(String.Empty) && _from.EndOfStream)
                //    continue;

                // get the current line of data
                //HqlLine line = new HqlLine(fromline, _settings, _from.RowNum);
                HqlLine line = _from.ReadLine();
                if (line.IsBlank && _from.EndOfStream)
                    continue;
                try
                {
                    // does this match the where statement?
                    if (_where.Evaluate(line))
                    {
                        // Process:
                        // Determine key
                        // - Figure out if this key exists
                        // - If it does, pull back the VALUE, which has the static info and the calcs
                        // - The calcs must be kept in a Field Format though
                        // - if written sum(substr(value)) then run through scalar first
                        // - if written len(sum(value)) then run through scalar on GetValue coming out
                        //

                        CreateKeyAndValue(line);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Caught except processing row num|{0}|{1}|", _from.RowNum, line.Line), ex);
                }
            }
        }

        private void ProcessHavingAndOrder()
        {
            if (_having.Count > 0)
            {
                HqlHavingRow havingRow = new HqlHavingRow(_orderby.FieldGroup, _groupby.FieldGroup, _select.FieldGroup);
                for (IDictionaryEnumerator ptr = _groupby.Table.GetEnumeratorForTable(); ptr.MoveNext(); )
                {
                    HqlKey key2 = (HqlKey)ptr.Key;
                    HqlResultRow value2 = (HqlResultRow)ptr.Value;

                    havingRow.Key = key2;
                    havingRow.Row = value2;

                    if (!_having.Evaluate(havingRow))
                    {
                        value2.ValuesSet = false;
                    }
                }
            }

            if (_orderby.Count > 0)
            {
                // I need to replace the keys with the actual values so that I sort them correctly
                for (IDictionaryEnumerator ptr = _groupby.Table.GetEnumeratorForTable(); ptr.MoveNext(); )
                {
                    HqlKey key2 = (HqlKey)ptr.Key;
                    HqlResultRow value2 = (HqlResultRow)ptr.Value;
                    if (!value2.ValuesSet)
                        continue;

                    for (int i = 0; i < _orderby.Count; ++i)
                    {
                        HqlField f = _orderby.FieldGroup[i];
                        if (f.HasFunction)
                        {
                            object o = ((HqlCalc)value2[i]).GetValue();
                            if (f.Scalar != null)
                            {
                                key2[i] = f.Scalar.Evaluate(o);
                            }
                            else
                            {
                                key2[i] = o;
                            }
                        }
                    }
                }
            }
        }

        private bool GetLineFunctionOneSourceOrderbySortFunction()
        {
            for (; ; )
            {
                if (!_successCalculate)
                {
                    _groupby.InitializeForStoringData(_orderby);
                    CalculateData();
                    _successCalculate = true;

                    ProcessHavingAndOrder();

                    _groupby.Table.Sort();
                    _ptr = _groupby.Table.GetSortedEnumeratorForTable();
                }

                if (!_ptr.MoveNext())
                    return false;

                HqlKey key = (HqlKey)_ptr.Current;
                HqlResultRow value = (HqlResultRow)_groupby.Table[key];

                // did this pass the Having command?
                if (!value.ValuesSet)
                    continue;

                _line = _select.EvaluateGroupBy(_orderby.Count, value);

                if (_settings.PrintCategorizeFilename)
                {
                    _outfilename = _settings.Output.EvaluateGroupBy(_orderby.Count + _select.Count, value);
                }

                return true;
            }
        }

        private void CalculateDataMultifiles()
        {
            HqlDataSource prior = _from[_from.Count - 2]; // guaranteed to be present b/c there is more than one file           
            HqlDataSource src = _from[_from.Count - 1];

            InitializeSourceForStreaming(src);

            for (; ; _linenum++)
            {
                if (src.ReadThroughUsingJoin(prior, prior.OnJoin, _where, true))
                {
                    CreateKeyAndValue(src.ResultValues);
                }
                else
                {
                    prior.DropLoadedData();
                    break;
                }
            }
        }

        private bool GetLineMultiSourceFunction()
        {
            for (; ; )
            {
                if (!_loadedFirstFiles)
                {
                    GetLineMultifileStreamableLoadFirstFiles();
                    _loadedFirstFiles = true;
                }

                if (!_successCalculate)
                {
                    _groupby.InitializeForStoringData(_orderby);
                    CalculateDataMultifiles();
                    _successCalculate = true;

                    ProcessHavingAndOrder();

                    _groupby.Table.Sort();
                    _ptr = _groupby.Table.GetSortedEnumeratorForTable();
                }

                if (!_ptr.MoveNext())
                    return false;

                HqlKey key = (HqlKey)_ptr.Current;
                HqlResultRow value = (HqlResultRow)_groupby.Table[key];

                // did this pass the Having command?
                if (!value.ValuesSet)
                    continue;

                _line = _select.EvaluateGroupBy(_orderby.Count, value);

                if (_settings.PrintCategorizeFilename)
                {
                    _outfilename = _settings.Output.EvaluateGroupBy(_orderby.Count + _select.Count, value);
                }

                return true;
            }
        }
                
        private void GetLineMultifileStreamableLoadFirstFiles()
        {
            HqlDataSource prior = null;

            for (int i = 0; i < _from.Count - 1; ++i)
            {
                HqlDataSource src = _from[i];
                InitializeSourceForStreaming(src);

                if (i == 0)
                {
                    src.ReadThroughUsingJoin(null, src.OnJoin, _where, false);
                }
                else
                {
                    src.ReadThroughUsingJoin(prior, prior.OnJoin, _where, false);
                    prior.DropLoadedData();
                }

                prior = src;
            }
        }

        private void InitializeSourceForStreaming(HqlDataSource src)
        {
            if (src.OnJoin == null)
                src.InitJoining();
            src.LoadFieldReferences(_select.FieldGroup);
            src.LoadFieldReferences(_where.EvaluationCriteria);
            src.LoadFieldReferences(_groupby.FieldGroup);
            src.LoadFieldReferences(_orderby.FieldGroup);
            if (_settings.PrintCategorizeFilename)
                src.LoadFieldReferences(_settings.Output.FieldGroup);
            
            for (int j = 0; j < _from.Count; ++j)
            {
                if (_from[j].OnJoin != null)
                    src.LoadFieldReferences(_from[j].OnJoin.EvaluationCriteria);
            }
            
            src.BuildBlankValues();
        }

        private bool GetLineMultifileStreamable()
        {
            if (!_loadedFirstFiles)
            {
                GetLineMultifileStreamableLoadFirstFiles();
                _loadedFirstFiles = true;
            }

            HqlDataSource prior = _from[_from.Count - 2]; // guaranteed to be present b/c there is more than one file           
            HqlDataSource src = _from[_from.Count - 1];

            if (!_initializedForStreaming)
            {
                InitializeSourceForStreaming(src);
                _initializedForStreaming = true;
            }

            if (src.ReadThroughUsingJoin(prior, prior.OnJoin, _where, true))
            {
                _line = _select.Evaluate(src.ResultValues);
                return true;
            }

            return false;
        }

        private bool GetLineStreamable()
        {
            for (; ; )
            {
                if (_from.EndOfStream)
                {
                    _line = String.Empty;
                    return false;
                }

                HqlLine line = _from.ReadLine();
                if (line.IsBlank && _from.EndOfStream)
                    continue;
                
                if (_where.Evaluate(line))
                {
                    // now i found a record that matches
                    _line = _select.Evaluate(line);
                    if (_settings.PrintCategorizeFilename)
                    {
                        _outfilename = _settings.Output.Evaluate(line);
                    }
                    return true;
                }
            }
        }

        // saves to _line
        // returns false on endofstream
        private bool GetLine()
        {
            bool result = false;
            if (_settings.PrintHeader)
            {
                _line = _select.GetHeaderRow();

                // TODO, is this the best way to set this flag?
                // If I never use it again it is, but what if I want to print header every 100 records?
                _settings.PrintHeader = false;
                result = true;
            }
            else if (OneDataSource && IsStreamable && !HasFunctions)
            {
                result = GetLineStreamable(); // added categorize
            }
            else if (OneDataSource)
            {
                result = GetLineFunctionOneSourceOrderbySortFunction();
            }
            else if (!OneDataSource && IsStreamable)
            {
                result = GetLineMultifileStreamable();
            }
            else if (!OneDataSource && HasFunctions)
            {
                result = GetLineMultiSourceFunction();
            }
            else if (!OneDataSource && HasOrderBy)
            {
                result = GetLineMultiSourceFunction();
            }
            else
            {
                throw new Exception("Complex FROM not coded");
            }

            return result;
        }

        private void RunQueryToOutput()
        {
            if (_settings.PrintCategorizeFilename)
                RunQueryToOutputCategorize();
            else
                RunQueryToOutputFile();
        }

        private void RunQueryToOutputCategorize()
        {
            Dictionary<HqlKey, HqlOutFile> dic = null;
            try
            {
                // have a hash table of the open filenames, when I get 25 open, close them all
                dic = new Dictionary<HqlKey, HqlOutFile>();
                int openedFiles = 0;
                for (; ; )
                {
                    if (openedFiles > 125)
                    {
                        CloseAllFiles(dic);
                        openedFiles = 0;
                    }
                    if (!GetLine())
                        break;

                    // due to windows being case-insensitive, I am forcing to uppercase when inside my dictionary.
                    //string upperCaseFilename = _outfilename.ToUpper();
                    HqlKey key = new HqlKey(new string[] { _outfilename.ToUpper() });
                    if (dic.ContainsKey(key))
                    {
                        if (!dic[key].IsOpen)
                        {
                            openedFiles++;
                        }
                    }
                    else
                    {
                        dic[key] = new HqlOutFile(_outfilename);
                        openedFiles++;
                    }
                    dic[key].WriteLine(_line);
                }
            }
            finally
            {
                CloseAllFiles(dic);
            }
        }

        private void RunQueryToOutputFile()
        {
            HqlOutFile outfile = new HqlOutFile(_settings.OutputFilename);
            for (; ; )
            {
                if (!GetLine())
                    break;
                outfile.WriteLine(_line);
            }
            outfile.Close();
        }

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        public bool IsStreamable
        {
            get { return (_having.Count == 0 && _groupby.Count == 0 && _orderby.Count == 0); }
        }
       
        public bool HasFunctions
        {
            get { return _select.HasFunctions; }
        }
       
        public bool OneDataSource
        {
            get { return (_from.Count == 1); }
        }
       
        public bool HasOrderBy
        {
            get { return (_orderby.Count > 0); }
        }

        public bool HasGroupBy
        {
            get { return (_groupby.Count > 0); }
        }

        public bool Compiled
        {
            get { return _successCompiled; }
        }

        public bool EndOfStream
        {
            get { return _endOfStream; }
            private set { _endOfStream = value; }
        }

        public int CountReturnVisibleFields
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _select.Count;++i)
                {
                    if (_select.FieldGroup[i].PrintResult)
                        count++;
                }
                return count;
            }
        }

        public string RemainingSql
        {
            get { return _sqlRemainingAfterCompile; }
        }


        ///////////////////////
        // Variables

        private string _sql;
        private string _sqlRemainingAfterCompile;
        private int _linenum;
        private string _line;
        private bool _endOfStream;
        private bool _successCompiled;
        private bool _successCalculate;
        private bool _loadedFirstFiles;
        private bool _initializedForStreaming;
        private object[] _references;
        private string _outfilename;

        private HqlSelect _select;
        private HqlFrom _from;
        private HqlWhere _where;
        private HqlGroupBy _groupby;
        private HqlHaving _having;
        private HqlOrderBy _orderby;
        //private HqlOutput _output;

        private HqlWith _settings;
        private IEnumerator _ptr;
        private string _expectedEndOfQuery;

    }
}
