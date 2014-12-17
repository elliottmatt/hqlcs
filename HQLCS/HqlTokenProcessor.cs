using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    enum HqlWordType
    {
        TEXT = 0,
        FLOAT,
        INT,

        KEYWORD,

        END_OF_LINE,
        FIELD,
        FUNCTION,

        LITERAL_STRING,

        UNKNOWN,

        SCALAR,

        STREAM,

        ROWNUM,
        NULL,
    }

    enum HqlKeyword
    {
        UNKNOWN = 0,
        NUMBER,
        SELECT,
        FROM,
        WHERE,
        GROUPBY,
        ORDERBY,
        HAVING,
        STAR,
        AND,
        OR,
        FUNCTION, // max min count average sum
        SCALAR, // length, substr, etc
        STDIN,
        DUAL,
        COMPARE, // > < >= <= == = not like like
        OPENPAREN,
        CLOSEDPAREN,
        COMMA,
        IN,
        NOT_IN,
        DESCENDING,
        ASCENDING,
        AS,
        WITH,
        RIGHT_OUTER,
        LEFT_OUTER,
        FULL_OUTER,
        INNER,
        JOIN,
        ON,
        SKIP,
        NULL,
    }

    class HqlTokenProcessor
    {
        ///////////////////////
        // Static Functions

        static protected HqlKeywords[] keywords = new HqlKeywords[]
        {
            new HqlKeywords(HqlKeyword.SELECT, new string[] { "select" }),
            new HqlKeywords(HqlKeyword.FROM, new string[] { "from" }),
            new HqlKeywords(HqlKeyword.WHERE, new string[] { "where" }),
            new HqlKeywords(HqlKeyword.GROUPBY, new string[] { "group by", "groupby" }),
            new HqlKeywords(HqlKeyword.ORDERBY, new string[] { "order by", "orderby" }),
            new HqlKeywords(HqlKeyword.HAVING, new string[] { "having" }),
            new HqlKeywords(HqlKeyword.AND, new string[] { "&&", "and" }),
            new HqlKeywords(HqlKeyword.OR, new string[] { "||", "or" }),
            new HqlKeywords(HqlKeyword.STAR, new string[] { "*" }),
            new HqlKeywords(HqlKeyword.COMMA, new string[] { "," }),
            new HqlKeywords(HqlKeyword.COMPARE, new string[] { ">", "<", ">=", "<=", "=", "<>", "!=", "not like", "like", "is", "is not"}),
            new HqlKeywords(HqlKeyword.FUNCTION, new string[] { "max", "min", "avg", "count", "sum", "average", "stdev", }),
            new HqlKeywords(HqlKeyword.OPENPAREN, new string[] { "(" }),
            new HqlKeywords(HqlKeyword.CLOSEDPAREN, new string[] { ")" }),
            new HqlKeywords(HqlKeyword.STDIN, new string[] { "stdin" }),
            new HqlKeywords(HqlKeyword.DUAL, new string[] { "dual" }),
            new HqlKeywords(HqlKeyword.ASCENDING, new string[] { "asc", "ascending" }),
            new HqlKeywords(HqlKeyword.DESCENDING, new string[] { "desc", "descending" }),
            new HqlKeywords(HqlKeyword.IN, new string[] { "in", "in like", "inlike" }),
            new HqlKeywords(HqlKeyword.NOT_IN, new string[] { "not in", "not in like", "in not like", "not inlike" }),
            new HqlKeywords(HqlKeyword.AS, new string[] { "as" }),
            new HqlKeywords(HqlKeyword.WITH, new string[] { "with" }),
            new HqlKeywords(HqlKeyword.LEFT_OUTER, new string[] { "left outer" }),
            new HqlKeywords(HqlKeyword.FULL_OUTER, new string[] { "full outer" }),
            new HqlKeywords(HqlKeyword.RIGHT_OUTER, new string[] { "right outer" }),
            new HqlKeywords(HqlKeyword.INNER, new string[] { "inner" }),
            new HqlKeywords(HqlKeyword.JOIN, new string[] { "join" }),
            new HqlKeywords(HqlKeyword.ON, new string[] { "on" }),
            new HqlKeywords(HqlKeyword.SKIP, new string[] { "skip" }),
            new HqlKeywords(HqlKeyword.NULL, new string[] { "null" }),
            new HqlKeywords(HqlKeyword.SCALAR, new string[]
                {
                    "len", "length",
                    "substr", "substring",
                    "now",
                    "pi",
                    "upper", "ucase", "lower", "lcase",
                    "trim", "trimleft", "trimright",
                    "concat", "concat_ws",
                    "replace",
                    "count_occur",
                    "getprintables",
                    "decimal",
                    "decode",
                    "to_date",
                    "to_char",
                }
            ),
        };

        static protected HqlSplitwords[] splitwords = new HqlSplitwords[]
        {
            new HqlSplitwords("group", "group by"),
            new HqlSplitwords("order", "order by"),
            new HqlSplitwords("not", "not like"),
            new HqlSplitwords("in", "in like"),
            new HqlSplitwords("not", "not in"),
            new HqlSplitwords("left", "left outer"),
            new HqlSplitwords("right", "right outer"),
            new HqlSplitwords("full", "full outer"),
            new HqlSplitwords("in", "in notlike"),
            new HqlSplitwords("not", "not inlike"),
            new HqlSplitwords("is", "is not"),
        };

        static public string EvaluateMath(string s)
        {
            // http://www.leeholmes.com/blog/LibraryForInlineCInMSH.aspx
            string s2 = String.Empty;
            try
            {
                Microsoft.CSharp.CSharpCodeProvider p = new Microsoft.CSharp.CSharpCodeProvider();
                System.CodeDom.Compiler.CompilerParameters cp = new System.CodeDom.Compiler.CompilerParameters();
                cp.IncludeDebugInformation = true;
                cp.GenerateInMemory = true;

                string codeToCompile = @"
                    using System;
                    public class InlineRunner { public object Invoke() { return (@CODE@); } }
                ";
                string code = codeToCompile.Replace("@CODE@", s);

                System.CodeDom.Compiler.CompilerResults r = p.CompileAssemblyFromSource(cp, code);

                if (r.Errors.Count > 0)
                {
                    return String.Empty;
                }

                object o = r.CompiledAssembly.CreateInstance("InlineRunner");

                Type t = o.GetType();
                object result = t.InvokeMember("Invoke", System.Reflection.BindingFlags.InvokeMethod, null, o, null);
                if (result is string)
                {
                    s2 = (string)result;
                }
                else if (result is Int64)
                {
                    s2 = result.ToString();
                }
                else if (result is int)
                {
                    s2 = result.ToString();
                }
                //else if (result is Decimal)
                //{
                //    s2 = ((Decimal)result).ToString("0.00");
                //}
                //else if (result is double)
                //{
                //    s2 = ((double)result).ToString("0.00");
                //}
                else
                {
                    s2 = result.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception evaluate math function", ex);
            }

            return s2;
        }

        static public void GetResultOfNestedQueryForInStatement(HqlTokenProcessor processor, ArrayList list)
        {
            try
            {
                HqlStream cs = new HqlStream(processor.GetRemainingSql(true), ")");
                if (cs.CountReturnVisibleFields != 1)
                    throw new Exception("Internal Hql streams must return only one value");

                for (; !cs.EndOfStream; )
                {
                    string s = cs.ReadLine();
                    if (s == null || s.Length == 0)
                        continue;

                    HqlToken token = new HqlToken(HqlWordType.UNKNOWN, s);
                    HqlTokenProcessor.Categorize(ref token, false, false, true, false);
                    if (token.WordType == HqlWordType.UNKNOWN)
                        token = new HqlToken(HqlWordType.LITERAL_STRING, token.Data);
                    list.Add(token);
                }
                cs.Close();
                processor.SetRemainingSql(cs.RemainingSql);

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to execute nested IN statement SQL", ex);
            }
        }

        static public string CleanupDelimiter(string delim)
        {
            if (delim.Equals("\\t"))
                return "\t";
            return delim;
        }

        ///////////////////////
        // Constructors

        public HqlTokenProcessor(string sql, HqlWith settings, object[] references)
        {
            Init(sql, settings);
            _references = references;
        }

        public HqlTokenProcessor(string sql, HqlTokenProcessor processor)
        {
            Init(sql, processor._settings);
            _processor = processor;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public bool MatchNext(string match)
        {
            if (_sql.Length >= match.Length)
                return _sql.Substring(0, match.Length).Equals(match, StringComparison.CurrentCultureIgnoreCase);
            return false;
        }
        
        public HqlToken GetToken()
        {
            if (_token == null)
                MoveNextToken();

            return _token;
        }

        public bool CheckForTokenReference(ref HqlToken token)
        {
            bool ret;

            ret = HqlTokenProcessor.CheckForTokenReference(_processor, ref token);
            if (ret) return ret;

            return HqlTokenProcessor.CheckForTokenReference(this, ref token);
        }

        public void MoveNextToken()
        {
            // empty line?
            if (_sql.Length == 0)
            {
                _token = new HqlToken(HqlWordType.END_OF_LINE);
                _sql = String.Empty;
                return;
            }

            HqlToken token = new HqlToken(HqlWordType.UNKNOWN);
            PeekTokenText(token);
            //_sql = _sql.Substring(text.Length + (token.HadOutlingCharacters() ? 2 : 0)).Trim();
            _sql = _sql.Substring(token.LengthOfDataWithOutlining).Trim();

            // is this a splitable word?
            if (!token.HadOutlingCharacters && _sql.Length > 0)
            {
                HqlToken second = null;
                foreach (HqlSplitwords w in splitwords)
                {
                    if (token.Data.Equals(w.FirstPart, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (second == null)
                        {
                            second = new HqlToken(HqlWordType.UNKNOWN);
                            PeekTokenText(second);
                            if (second.HadOutlingCharacters)
                                break;
                        }
                        string wholepiece = String.Format("{0} {1}", token.Data, second.Data);
                        if (wholepiece.Equals(w.WholePhrase, StringComparison.CurrentCultureIgnoreCase))
                        {
                            _sql = _sql.Substring(second.LengthOfDataWithOutlining).Trim();
                            token.Data = wholepiece;
                            break;
                        }
                    }
                }
            }

            _token = token;
            Categorize(ref _token);

            ChangeTokenToField();

            if (MoveNextTokenCheckFixedWidth()) 
            {
                //_token = _token;
            }
            else if (MoveNextTokenCheckFunction())
            {
                //_token = _token;
            }
            else if (MoveNextTokenCheckScalar())
            {
                //_token = _token;
            }
            else if (MoveNextTokenCheckForParameterized())
            {
                Categorize(ref _token);
            }
            else if (MoveNextTokenCheckASReference())
            {
                //Categorize(ref _token);
            }
        }

        public bool MatchesEndOfProcessing()
        {
            HqlToken t = GetToken();
            if (t.WordType == HqlWordType.END_OF_LINE)
                return true;
            if (t.WordType != HqlWordType.LITERAL_STRING && t.Data != null && t.Data.Equals(_settings.ExpectedEndOfQuery))
                return true;
            return false;
        }

        public static void Categorize(ref HqlToken token)
        {
            Categorize(ref token, true, true, true, true);
        }

        public static void Categorize(ref HqlToken token, bool checkEquation, bool checkKeyword, bool checkFloatInt, bool checkFieldRownumFilename)
        {
            if (token == null)
                return;

            if (checkEquation && token.HadEquation)
            {
                try
                {
                    string result = EvaluateMath(token.Data);
                    if (result.Length == 0)
                        token.Data = "^" + token.Data + "^";
                    else
                        token.Data = result;
                    token.WordType = HqlWordType.LITERAL_STRING;
                    return;
                }
                catch
                {
                    token.Data = "^" + token.Data + "^";
                }
            }

            if (token.HadTicky)
            {
                token.WordType = HqlWordType.LITERAL_STRING;
                return;
            }

            if (token.HadQuotes)
            {
                token.WordType = HqlWordType.TEXT;
                return;
            }

            if (checkKeyword && token.Data != null)
            {
                foreach (HqlKeywords k in keywords)
                {
                    foreach (string s in k.Words)
                    {
                        if (s.Equals(token.Data, StringComparison.CurrentCultureIgnoreCase))
                        {
                            token.WordType = HqlWordType.KEYWORD;
                            token.Keyword = k.Keyword;
                            return;
                        }
                    }
                }
            }

            if (checkFloatInt)
            {
                bool FoundNumber = false;
                bool IsInt = true;
                bool IsFloat = true;
                for (int i = 0; token.Data != null && (IsInt || IsFloat) && i < token.Data.Length; ++i)
                {
                    char c = token.Data[i];

                    if (Char.IsDigit(c))
                    {
                        FoundNumber = true;
                        continue;
                    }
                    else if (c == '-')
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        else
                        {
                            IsInt = false;
                            IsFloat = false;
                            break;
                        }
                    }
                    else if (c == '.')
                    {
                        IsInt = false;
                    }
                    else
                    {
                        IsFloat = false;
                        IsInt = false;
                        break;
                    }
                }
                if (FoundNumber && IsInt)
                {
                    token.WordType = HqlWordType.INT;
                    token.Parsed = Int64.Parse(token.Data);
                }
                else if (FoundNumber && IsFloat)
                {
                    token.WordType = HqlWordType.FLOAT;
                    token.Parsed = Decimal.Parse(token.Data);
                }
            }

            if (checkFieldRownumFilename)
            {
                if (token.WordType == HqlWordType.UNKNOWN && token.Data != null && token.Data.Length > 5 && token.Data.Substring(0, 5).Equals("field", StringComparison.CurrentCultureIgnoreCase))
                {
                    string fieldnumstr = token.Data.Substring(5);
                    int fieldnum;
                    if (!Int32.TryParse(fieldnumstr, out fieldnum))
                        throw new Exception("Expected a fieldnum on FIELD");
                    if (fieldnum < 1)
                        throw new Exception("Expected a greater than 0 FIELD");
                    // it is one based so move back to zero based now
                    fieldnum--;

                    token = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.FIELDNUM, fieldnum));
                }
                else if (token.WordType == HqlWordType.UNKNOWN && token.Data != null && token.Data.Length > 5 && token.Data.Substring(0, 6).Equals("rownum", StringComparison.CurrentCultureIgnoreCase))
                {
                    token = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.ROWNUM));
                }
                else if (token.WordType == HqlWordType.UNKNOWN && token.Data != null && token.Data.Length > 7 && token.Data.Substring(0, 8).Equals("filename", StringComparison.CurrentCultureIgnoreCase))
                {
                    token = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.FILENAME));
                }
            }
        }

        public string GetRemainingSql(bool IncludeCurrentToken)
        {
            string s = _sql;
            if (IncludeCurrentToken && _token.Data != null)
                s = _token.Data + " " + s;
            return s.Trim();
        }

        public void SetRemainingSql(string s)
        {
            _sql = s;
        }

        public HqlToken GetOptionData(string option)
        {
            MoveNextToken();
            HqlToken token = GetToken();

            // optional = sign
            if (token.WordType == HqlWordType.KEYWORD && token.Data.Equals("="))
            {
                // that's fine, eat it!
                MoveNextToken();
                token = GetToken();
            }

            if (MatchesEndOfProcessing())
                throw new Exception(String.Format("Unexpected END-OF-LINE after {0}", option));

            return token;
        }

        ///////////////////////
        // Private

        private static bool CheckForTokenReference(HqlTokenProcessor processor, ref HqlToken token)
        {
            if (processor == null || token == null)
                return false;

            if (processor._fixedwidthTokens == null)
                return false;
            for (int i = 0; i < processor._fixedwidthTokens.Count; ++i)
            {
                HqlToken t = (HqlToken)processor._fixedwidthTokens[i];
                if (token.Data.Equals(t.Field.Name))
                {
                    token = t;
                    return true;
                }
            }
            return false;
        }

        private void Init(string sql, HqlWith settings)
        {
            _token = null;
            _sql = sql;
            //_originalSql = sql;
            _settings = settings;
        }

        private void PeekTokenText(HqlToken token)
        {
            token.Data = String.Empty;

            // am i looking at quotes?
            if (_sql.Length == 0)
            {
                return;
            }
            else if (_sql.StartsWith("\"", StringComparison.CurrentCulture))
            {
                int next_quote = _sql.IndexOf('"', 1);
                if (next_quote == -1)
                    throw new Exception("Unbalanced quotes in the SQL");
                token.Data = _sql.Substring(1, next_quote - 1);

                token.SetQuote(true);
            }
            // am i looking at ticky?
            else if (_sql.StartsWith("\'", StringComparison.CurrentCulture))
            {
                int start = 1;
                int next_ticky;
                while (true)
                {
                    next_ticky = _sql.IndexOf('\'', start);
                    if (next_ticky == -1)
                        throw new Exception("Unbalanced ticky in the SQL");
                    if (next_ticky == _sql.Length - 1)
                        break;
                    if (_sql[next_ticky + 1] == '\'')
                    {
                        _sql = _sql.Remove(next_ticky, 1);
                        start = next_ticky + 1;
                    }
                    else
                        break;
                }

                token.Data = _sql.Substring(1, next_ticky - 1);

                token.SetTicky(true);
            }
            else if (_sql.StartsWith("^", StringComparison.CurrentCulture))
            {
                int next_caret = _sql.IndexOf('^', 1);
                if (next_caret == -1)
                    throw new Exception("Unbalanced caret in the SQL");
                token.Data = _sql.Substring(1, next_caret - 1);

                token.SetEquation(true);
            }
            else
            {
                token.UnsetFlags();

                StringBuilder sb = new StringBuilder();

                if (Char.IsLetter(_sql[0]) || _sql[0] == '_')
                {
                    for (int i = 0; i < _sql.Length; ++i)
                    {
                        if (Char.IsLetterOrDigit(_sql[i]) || _sql[i] == '_')
                            sb.Append(_sql[i]);
                        else
                            break;
                    }
                }
                else if (Char.IsDigit(_sql[0]) || _sql[0] == '.' || _sql[0] == '-')
                {
                    for (int i = 0; i < _sql.Length; ++i)
                    {
                        if (Char.IsDigit(_sql[i]) || _sql[i] == '.' || _sql[i] == '-')
                            sb.Append(_sql[i]);
                        else
                            break;
                    }
                }
                else if (
                        _sql.StartsWith(">=", StringComparison.CurrentCulture)
                        || _sql.StartsWith(">=", StringComparison.CurrentCulture)
                        || _sql.StartsWith("!=", StringComparison.CurrentCulture)
                        || _sql.StartsWith("<>", StringComparison.CurrentCulture)
                    )
                {
                    sb.Append(_sql.Substring(0, 2));
                }
                else if (Char.IsSymbol(_sql[0]) || Char.IsPunctuation(_sql[0]))
                {
                    sb.Append(_sql[0]);
                }
                else
                {
                    throw new Exception(String.Format("Unknown character in string! What do I do with this??? |{0}|", _sql[0]));
                }

                token.Data = sb.ToString();
            }

#if DEBUG && false
            Console.WriteLine(String.Format("Debug: PeekTokenText |{0}|", token.Data));
#endif
        }

        private void ChangeTokenToField()
        {
            if (_token.WordType == HqlWordType.KEYWORD)
            {
                if (_token.Keyword == HqlKeyword.STAR)
                {
                    _token = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.STAR));
                }
                else if (_token.Keyword == HqlKeyword.NULL)
                {
                    _token = new HqlToken(HqlWordType.NULL);
                }
            }
        }

        private bool MoveNextTokenCheckASReference()
        {
            if (_token.WordType == HqlWordType.UNKNOWN && MatchNext("."))
            {
                HqlToken tableReference = _token;
                MoveNextToken();
                HqlToken dot = _token;
                if (dot.WordType != HqlWordType.UNKNOWN || !dot.Data.Equals("."))
                {
                    throw new Exception("Unknown dot in field reference");
                }

                MoveNextToken();

                if (
                    (_token.WordType == HqlWordType.FIELD) ||
                    (_token.WordType == HqlWordType.KEYWORD && _token.Keyword == HqlKeyword.STAR)
                    )
                {
                    // pass through
                }
                else
                {
                    throw new Exception("Unknown field referenced");
                }

                _token.Field.TableReference = tableReference.Data;
                
                return true;
            }

            return false;
        }

        private bool MoveNextTokenCheckScalar()
        {
            if (_token.WordType == HqlWordType.KEYWORD && _token.Keyword == HqlKeyword.SCALAR && MatchNext("("))
            {
                HqlScalar scalar = new HqlScalar();
                scalar.Parse(this);
                _token = new HqlToken(HqlWordType.SCALAR, new HqlField(HqlFieldType.SCALAR, scalar));
                return true;
            }

            return false;
        }

        private bool MoveNextTokenCheckFunction()
        {
            if (_token.WordType == HqlWordType.KEYWORD && _token.Keyword == HqlKeyword.FUNCTION && MatchNext("("))
            {
                HqlFunction func = null;
                HqlToken funcName = _token;

                MoveNextToken();
                HqlToken paren = _token;
                if (paren.WordType != HqlWordType.KEYWORD || paren.Keyword != HqlKeyword.OPENPAREN)
                    throw new Exception("Expected an open-paren in function declaration.");

                MoveNextToken();
                HqlToken next = _token;

                if (next.WordType == HqlWordType.FIELD)
                {
                    if (next.Field.HasFunction)
                        throw new Exception("Cannot have nested functions");
                    func = new HqlFunction(HqlFunction.ResolveFunctionType(funcName.Data), next.Field);                        
                    next = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.FUNCTION, func));
                }
                else if (next.WordType == HqlWordType.KEYWORD && next.Keyword == HqlKeyword.STAR)
                {
                    HqlField star = new HqlField(HqlFieldType.STAR);
                    func = new HqlFunction(HqlFunction.ResolveFunctionType(funcName.Data), star);
                    next = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.FUNCTION, func));
                }
                else if (next.WordType == HqlWordType.SCALAR)
                {
                    func = new HqlFunction(HqlFunction.ResolveFunctionType(funcName.Data), next.Field);
                    next = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.FUNCTION, func));
                }
                else if (next.WordType == HqlWordType.INT || next.WordType == HqlWordType.FLOAT)
                {
                    func = new HqlFunction(HqlFunction.ResolveFunctionType(funcName.Data), HqlField.CreateObject(next));
                    next = new HqlToken(HqlWordType.FIELD, new HqlField(HqlFieldType.FUNCTION, func));
                }
                else
                {
                    throw new Exception("Expected a valid field instead of this unknown object");
                }

                MoveNextToken();
                HqlToken lookForComma = _token;
                while (lookForComma.WordType == HqlWordType.KEYWORD && lookForComma.Keyword == HqlKeyword.COMMA)
                {
                    MoveNextToken();
                    HqlToken option = _token;
                    func.SetOption(option);
                    
                    MoveNextToken();
                    lookForComma = _token;
                }

                HqlToken closedparen = _token;
                if (closedparen.WordType != HqlWordType.KEYWORD || closedparen.Keyword != HqlKeyword.CLOSEDPAREN)
                    throw new Exception("Expected an close-paren in function declaration.");

                _token = next;
                return true;
            }

            return false;
        }

        private bool MoveNextTokenCheckFixedWidth()
        {
            // if this is just text followed by a ( it must be a field or function
            //
            if (_token.WordType == HqlWordType.UNKNOWN && MatchNext("("))
            {
                HqlToken fieldName = _token;
                
                MoveNextToken();
                HqlToken paren = _token;
                if (paren.WordType != HqlWordType.KEYWORD || paren.Keyword != HqlKeyword.OPENPAREN)
                    throw new Exception("Expected an open-paren in fixed-width declaration.");

                MoveNextToken();
                HqlToken first = _token;

                if (first.WordType == HqlWordType.INT)
                {
                    // means it is a field. Next two tokens should be a comma and int
                    MoveNextToken();
                    HqlToken comma = _token;
                    if (!comma.Data.Equals(","))
                        throw new Exception("Expected a comma in fixed-width declaration.");
                    MoveNextToken();
                    HqlToken second = _token;
                    if (second.WordType != HqlWordType.INT)
                        throw new Exception("Expected an int in fixed-width declaration.");
                    MoveNextToken();
                    HqlToken closeparen = _token;
                    if (closeparen.WordType != HqlWordType.KEYWORD || closeparen.Keyword != HqlKeyword.CLOSEDPAREN)
                        throw new Exception("Expected a close-paren in fixed-width declaration.");

                    if ((Int64)first.Parsed < 1)
                        throw new Exception("Expected an integer greater than 0 for start position.");
                    if ((Int64)second.Parsed < 1)
                        throw new Exception("Expected an integer greater than 0 for length.");

                    first.Parsed = (Int64)first.Parsed - 1; // it is one-based so move back to zero-based

                    HqlField f = new HqlField(HqlFieldType.FIXEDWIDTH, (int)(Int64)first.Parsed, (int)(Int64)second.Parsed, fieldName.Data);
                    _token = new HqlToken(HqlWordType.FIELD, f);

                    AddTokenToInternalList(_token);

                    return true;
                }
                else
                {
                    throw new Exception("Expected a well-formed fixed-width statement.");
                }
            }
            return false;
        }

        private bool MoveNextTokenCheckForParameterized()
        {
            // if this is a {, make it followed by a number and a }
            //
            if (_references != null && _token.WordType == HqlWordType.UNKNOWN && _token.Data.Equals("{"))
            {
                // save the current info to save this state
                string savedSql = _sql;
                HqlToken savedToken = _token;

                MoveNextToken();
                if (_token.WordType == HqlWordType.INT && MatchNext("}"))
                {
                    HqlToken parameterNumber = _token;
                    MoveNextToken(); // eat the }

                    object o = GetReference((int)parameterNumber.Parsed);
                    if (o is StreamReader)
                    {
                        _token = new HqlToken(HqlWordType.STREAM);
                        _token.Parsed = o;
                    }
                    else
                    {
                        throw new Exception("What sort of stuff can you reference-pass-in???");
                    }

                    return true;
                }
                else
                {
                    _sql = savedSql;
                    _token = savedToken;
                }
            }
            return false;
        }

        private void AddTokenToInternalList(HqlToken token)
        {
            if (_fixedwidthTokens == null)
                _fixedwidthTokens = new ArrayList();
            for (int i = 0; i < _fixedwidthTokens.Count; ++i)
            {
                HqlToken t = (HqlToken)_fixedwidthTokens[i];
                if (token.Field.Name.Equals(t.Field.Name))
                {
                    if (token.Field.Start != t.Field.Start || token.Field.Length != t.Field.Length)
                    {
                        throw new Exception("Same name given to more than one fixed-width field!");
                    }
                }
            }

            _fixedwidthTokens.Add(token);
        }

        private object GetReference(int n)
        {
            if (_references == null || _references.Length < n)
                throw new Exception("Attempted to reference a Parameter that doesn't exist");
            return _references[n];            
        }

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        ///////////////////////
        // Variables

        object[] _references;
        HqlToken _token;
        string _sql;
        //string _originalSql;
        ArrayList _fixedwidthTokens;
        HqlWith _settings;
        HqlTokenProcessor _processor;
    }
}
