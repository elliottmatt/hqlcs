using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    public enum HqlScalarType
    {
        UNKNOWN,

        LENGTH,
        SUBSTRING,
        NOW,

        UPPER,
        LOWER,

        TRIM,
        TRIMLEFT,
        TRIMRIGHT,

        CONCATENTATE,
        CONCATENTATE_WS,

        PI,

        REPLACE,

        COUNT_OCCUR,

        GETPRINTABLES,

        DECIMAL,

        DECODE,
        
        TO_DATE,
    }

    class HqlScalar
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlScalar()
        {
            //_internal = null; // unnecessary
        }

        ///////////////////////
        // Overridden functions

        public override bool Equals(object obj)
        {
            if (obj is HqlScalar)
            {
                return this.Equals((HqlScalar)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException("Did not implement hashcode");
        }

        ///////////////////////
        // Public 

        public bool Equals(HqlScalar scalar)
        {
            if (scalar.Scalar == null || this.Scalar == null)
                return false;

            return Scalar.Equals(scalar.Scalar);
        }

        public void Parse(HqlTokenProcessor processor)
        {
            HqlToken token;
            bool FieldExpected = true;

            // first thing should be a function type
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.SCALAR)
                throw new Exception("Expected SCALAR type");

            Scalar = HqlScalarInternal.CreateObject(token.Data, processor);

            processor.MoveNextToken();
            token = processor.GetToken();
            if (token.WordType != HqlWordType.KEYWORD || token.Keyword != HqlKeyword.OPENPAREN)
                throw new Exception("Expected paren in SCALAR");            

            // now pull back things until you get to FROM
            for (; ; )
            {
                processor.MoveNextToken();
                token = processor.GetToken();
                if (token.WordType == HqlWordType.END_OF_LINE)
                    throw new Exception("Reached EOL in SCALAR clause");

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.CLOSEDPAREN)
                    break;
                
                if (processor.MatchesEndOfProcessing())
                    break;                    

                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.COMMA)
                {
                    FieldExpected = true;
                    continue;
                }

                if (!FieldExpected)
                    throw new Exception("Found additional SCALAR parameters not expected");
                FieldExpected = false;

                Scalar.AddToken(token);
            }

            Validate();
        }

        public object Evaluate(object o) { return Scalar.Evaluate(o); }

        public object Evaluate(HqlRecord record) { return Scalar.Evaluate(record); }

        public string GetFullName() { return Scalar.GetFullName(); }
        
        public string GetFieldName() { return Scalar.GetFieldName(); }

        public bool ContainsTableReference(string tableReference)
        {
            return Scalar.ContainsTableReference(tableReference);
        }

        public bool ContainsField(HqlField field)
        {
            return Scalar.ContainsField(field);
        }

        public bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return Scalar.ContainsRownum(OnlyWithoutTableReference);
        }

        public void Cleanup()
        {
            _scalar.Cleanup();
        }

        ///////////////////////
        // Private

        private void Validate() { Scalar.Validate(); }

        //private void AddToken(HqlToken token) { Scalar.AddToken(token); }
        
        ///////////////////////
        // Fields
        
        public bool HasFunction { get { return Scalar.HasFunction; } }

        public bool HasMultipleFields { get { return Scalar.HasMultipleFields; } }

        ///////////////////////
        // Getters/Setters

        public HqlField Field { get { return Scalar.Field; } }

        public HqlField[] Fields { get { return Scalar.Fields; } }

        private HqlScalarInternal Scalar
        { 
            get { return _scalar; } 
            set { _scalar = value; } 
        }

        ///////////////////////
        // Variables

        HqlScalarInternal _scalar;
    }

    internal abstract class HqlScalarInternal
    {
        ///////////////////////
        // Static Functions

        public static HqlScalarInternal CreateObject(string name, HqlTokenProcessor processor)
        {
            switch (name.ToLower())
            {
                case "len":
                case "length":
                    return new HqlScalarLen(processor);
                case "substring":
                case "substr":
                    return new HqlScalarSubstring(processor);
                case "now":
                    return new HqlScalarNow(processor);
                case "upper":
                case "ucase":
                    return new HqlScalarUpper(processor);
                case "lower":
                case "lcase":
                    return new HqlScalarLower(processor);
                case "trim":
                    return new HqlScalarTrim(processor);
                case "trimleft":
                    return new HqlScalarTrimLeft(processor);
                case "trimright":
                    return new HqlScalarTrimRight(processor);
                case "concat":
                    return new HqlScalarConcatenate(processor);
                case "concat_ws":
                    return new HqlScalarConcatenateWs(processor);
                case "replace":
                    return new HqlScalarReplace(processor);
                case "pi":
                    return new HqlScalarPI(processor);
                case "count_occur":
                    return new HqlScalarCountOccur(processor);
                case "getprintables":
                    return new HqlScalarGetPrintables(processor);
                case "decimal":
                    return new HqlScalarDecimal(processor);
                case "decode":
                    return new HqlScalarDecode(processor);
                case "to_date":
                case "to_char":
                    return new HqlScalarToDate(processor);
                default:
                    throw new Exception("Unknown SCALAR name");
            }
        }

        ///////////////////////
        // Constructors

        public HqlScalarInternal(HqlTokenProcessor processor)
        {
            Processor = processor;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public abstract bool Equals(HqlScalarInternal scalar);

        public virtual object Evaluate(object o)
        {
            if (o is string)
                return Evaluate((string)o);
            else if (o is HqlRecord)
                return Evaluate((HqlRecord)o);
            else if (o is Decimal)
                return Evaluate((decimal)o);
            else if (o is Int64)
                return Evaluate((Int64)o);
            else
                return Evaluate(o.ToString());
        }

        public virtual object Evaluate(HqlRecord record) { throw new Exception("Unexpected data"); }

        public virtual object Evaluate(string s) { throw new Exception("Unexpected data"); }

        public virtual object Evaluate(Int64 s)
        {
#if DEBUG
            Console.WriteLine("Debug Warning: {0} accepts int64 and doesn't process correctly!", this.GetType().ToString());
#endif
            return Evaluate(s.ToString());
        }

        public virtual object Evaluate(Decimal s)
        {
#if DEBUG
            Console.WriteLine("Debug Warning: {0} accepts decimal and doesn't process correctly!", this.GetType().ToString());
#endif
            return Evaluate(HqlCategory.PrintDefaultDecimal(s));
        }

        public abstract string GetScalarName();

        public abstract string GetFullName();

        public abstract string GetFieldName();

        public abstract void AddToken(HqlToken token);

        public abstract void Validate();

        public abstract bool ContainsTableReference(string tableReference);

        public virtual void Cleanup() { }

        public abstract bool ContainsField(HqlField field);

        public abstract bool ContainsRownum(bool OnlyWithoutTableReference);

        ///////////////////////
        // Private
        
        
        ///////////////////////
        // Fields

        public abstract bool HasFunction { get; }
        
        public abstract bool HasMultipleFields { get; }

        ///////////////////////
        // Getters/Setters

        public abstract HqlField Field { get; }

        public virtual HqlField[] Fields 
        {
            get { return null; }
        }

        public HqlScalarType ScalarType
        {
            get { return _type; }
        }

        protected HqlTokenProcessor Processor
        {
            get { return _processor; }
            set { _processor = value; }
        }
        ///////////////////////
        // Variables

        protected HqlTokenProcessor _processor;
        protected HqlScalarType _type;
    }
    
    // Various different types

    class HqlScalarCountOccur : HqlScalarInternal
    {
        public HqlScalarCountOccur(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.COUNT_OCCUR;
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar is HqlScalarCountOccur))
                return false;

            HqlScalarCountOccur s = (HqlScalarCountOccur)scalar;

            return (Field.Equals(s.Field) && Search.Equals(s.Search));
        }

        public override bool HasFunction
        {
            get
            {
                if (Field == null) return false;
                return Field.HasFunction;
            }
        }

        public override HqlField Field
        {
            get { return _field; }
        }

        public HqlField Search
        {
            get { return _search; }
        }

        public override void AddToken(HqlToken token)
        {
            if (_field == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _field = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _field = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _field = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _field = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else if (_search == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _search = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _search = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _search = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _search = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else
            {
                throw new Exception("Too many arguments given in SCALAR parameters");
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            string field = Field.GetValue(record).ToString();
            string search = Search.GetValue(record).ToString();

            int count = 0;
            int start = 0;
            for (; ; )
            {
                int index = field.IndexOf(search, start, StringComparison.CurrentCulture);
                if (index >= start)
                {
                    count++;
                    start = index + 1;
                    continue;
                }
                break;
            }

            return count;
        }

        public override void Validate()
        {
            if (Field == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
        }

        public override string GetScalarName()
        {
            return "count_occur";
        }

        public override string GetFullName()
        {
            return String.Format("{0}({1}, {2})", GetScalarName(), Field.ToString(), Search.ToString());
        }

        public override string GetFieldName()
        {
            return String.Format("{0}", Field.ToString());
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return _field.ContainsTableReference(tableReference) || _search.ContainsTableReference(tableReference);
        }

        public override bool ContainsField(HqlField field)
        {
            return _field.ContainsField(field) || _search.ContainsField(field);
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference) || _search.ContainsRownum(OnlyWithoutTableReference);
        }

        HqlField _field;
        HqlField _search;
    }

    class HqlScalarDecimal : HqlScalarInternal
    {
        public HqlScalarDecimal(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.DECIMAL;
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar is HqlScalarDecimal))
                return false;

            HqlScalarDecimal s = (HqlScalarDecimal)scalar;

            return (Field.Equals(s.Field) && Length.Equals(s.Length));
        }

        public override bool HasFunction
        {
            get
            {
                if (Field == null) return false;
                return Field.HasFunction;
            }
        }

        public override HqlField Field
        {
            get { return _field; }
        }

        public HqlField Length
        {
            get { return _length; }
        }

        public override void AddToken(HqlToken token)
        {
            if (_field == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _field = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _field = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _field = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _field = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else if (_length == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _length = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _length = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _length = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _length = token.Field;
                }
                else if (token.WordType == HqlWordType.INT)
                {
                    _length = new HqlField(HqlFieldType.LITERAL_INT, (Int64)token.Parsed);
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else
            {
                throw new Exception("Too many arguments given in SCALAR parameters");
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            object field = Field.GetValue(record);
            object length = Length.GetValue(record);
            if (!(length is Int32))
            {
                if (HqlCategory.IsInt(length.ToString()))
                {
                    length = Int32.Parse(length.ToString());
                }
                if (!(length is Int32) || (int)length < 0)
                    throw new Exception("Second parameter in Decimal must be a positive or zero integer");
            }

            if (!(field is Decimal))
            {
                if (HqlCategory.IsFloat(field.ToString()))
                {
                    field = Decimal.Parse(field.ToString());
                }
                if (!(field is Decimal))
                    throw new Exception("First parameter in Decimal must be a decimal");

            }

            return Evaluate((decimal)field, (int)length);
        }

        static public object Evaluate(decimal field, int length)
        {
            string calculatedPrintString;
            if (length == 0)
                calculatedPrintString = "0";
            else
                calculatedPrintString = "0." + new string('0', length);

            return field.ToString(calculatedPrintString);
        }

        public override object Evaluate(decimal field)
        {
            if (Length.FieldType != HqlFieldType.LITERAL_INT)
                throw new Exception("Cannot evaluate due to precision not a literal integer");

            return Evaluate(field, (int)(Int32)Length.IntValue);
        }

        //public override object Evaluate(Int64 field)
        //{
        //    if (Length.FieldType != HqlFieldType.LITERAL_INT)
        //        throw new Exception("Cannot evaluate due to precision not a literal integer");

        //    return Evaluate(field, (int)(Int32)Length.IntValue);
        //}

        public override void Validate()
        {
            if (Field == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
        }

        public override string GetScalarName()
        {
            return "decimal";
        }

        public override string GetFullName()
        {
            return String.Format("{0}({1}, {2})", GetScalarName(), Field.ToString(), Length.ToString());
        }

        public override string GetFieldName()
        {
            return String.Format("{0}", Field.ToString());
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return _field.ContainsTableReference(tableReference) || _length.ContainsTableReference(tableReference);
        }

        public override bool ContainsField(HqlField field)
        {
            return _field.ContainsField(field) || _length.ContainsField(field);
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference) || _length.ContainsRownum(OnlyWithoutTableReference);
        }

        HqlField _field;
        HqlField _length;
    }

    class HqlScalarReplace : HqlScalarInternal
    {
        public HqlScalarReplace(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.REPLACE;
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar is HqlScalarReplace))
                return false;

            HqlScalarReplace s = (HqlScalarReplace)scalar;

            return (Field.Equals(s.Field) && Replace.Equals(s.Replace));
        }

        public override bool HasFunction
        {
            get
            {
                if (Field == null) return false;
                return Field.HasFunction;
            }
        }

        public override HqlField Field
        {
            get { return _field; }
        }

        public override void AddToken(HqlToken token)
        {
            HqlField f = null;
            if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
            {
                f = new HqlField(HqlFieldType.STAR);
            }
            else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
            {
                f = token.Field;
            }
            else if (token.WordType == HqlWordType.LITERAL_STRING)
            {
                f = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
            }
            else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
            {
                f = token.Field;
            }
            else
            {
                throw new Exception("Expected a valid field in SCALAR parameters");
            }

            if (_field == null)
            {
                _field = f;
            }
            else if (Search == null)
            {
                Search = f;
            }
            else if (Replace == null)
            {
                Replace = f;
            }
            else
            {
                throw new Exception("Already set maximum parameters to REPLACE");
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            string field = Field.GetValue(record).ToString();
            string search = Search.GetValue(record).ToString();
            string replace = Replace.GetValue(record).ToString();

            return field.Replace(search, replace);
        }

        public override void Validate()
        {
            if (Field == null) { throw new Exception("Expected a valid field in REPLACE parameters"); }
            if (Search == null) { throw new Exception("Expected a valid search in REPLACE parameters"); }
            if (Replace == null) { throw new Exception("Expected a valid replace in REPLACE parameters"); }
        }

        public override string GetScalarName() { return "replace"; }

        public override string GetFullName()
        {
            return String.Format("{0}({1}, {2})", GetScalarName(), Field.ToString(), Search.ToString(), Replace.ToString());
        }

        public override string GetFieldName()
        {
            return String.Format("{0}", Field.ToString());
        }

        public HqlField Search
        {
            get { return _search; }
            private set { _search = value; }
        }

        public HqlField Replace
        {
            get { return _replace; }
            private set { _replace = value; }
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return _field.ContainsTableReference(tableReference) ||
                _search.ContainsTableReference(tableReference) ||
                _replace.ContainsTableReference(tableReference);
        }

        public override bool ContainsField(HqlField field)
        {
            return _field.ContainsField(field) ||
                _search.ContainsField(field) ||
                _replace.ContainsField(field);
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference) ||
                _search.ContainsRownum(OnlyWithoutTableReference) ||
                _replace.ContainsRownum(OnlyWithoutTableReference);
        }

        HqlField _field;
        HqlField _search;
        HqlField _replace;
    }

    class HqlScalarSubstring : HqlScalarInternal
    {
        public HqlScalarSubstring(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.SUBSTRING;
            Start = -1;
            Length = -1;
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar is HqlScalarSubstring))
                return false;

            HqlScalarSubstring s = (HqlScalarSubstring)scalar;

            return (Field.Equals(s.Field) && Start == s.Start && Length == s.Length);
        }

        public override bool HasFunction
        {
            get
            {
                if (Field == null) return false;
                return Field.HasFunction;
            }
        }

        public override HqlField Field
        {
            get { return _field; }
        }

        public override void AddToken(HqlToken token)
        {
            if (_field == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _field = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _field = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _field = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _field = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else
            {
                if (Start == -1)
                {
                    if (token.WordType != HqlWordType.INT)
                        throw new Exception("Expected START value to be an INTEGER");
                    Start = token.ParsedAsInt;
                    if (Start < 1)
                        throw new Exception("Start value must be >= 1");
                    Start--;
                }
                else if (Length == -1)
                {
                    if (token.WordType != HqlWordType.INT)
                        throw new Exception("Expected LENGTH value to be an INTEGER");
                    Length = token.ParsedAsInt;
                    if (Length < 1)
                        throw new Exception("Length value must be >= 1");
                }
                else
                {
                    throw new Exception("Too many parameters given to SCALAR");
                }
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            string s = Field.GetValue(record).ToString();
            return Evaluate(s);
        }

        public override object Evaluate(string s)
        {
            if (s.Length < Start)
                return String.Empty;
            else if (Length > 0)
            {
                if (s.Length < Start + Length)
                    return s.Substring(Start);
                else
                    return s.Substring(Start, Length);
            }
            else
            {
                return s.Substring(Start);
            }
        }

        public override void Validate()
        {
            if (Field == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
            if (Start < 0)
                throw new Exception("Expected a START value in SCALAR parameters");
        }

        public override string GetScalarName() { return "substring"; }

        public override string GetFullName()
        {
            if (Length <= 0)
                return String.Format("{0}({1}, {2})", GetScalarName(), Field.ToString(), Start + 1);
            return String.Format("{0}({1}, {2}, {3})", GetScalarName(), Field.ToString(), Start + 1, Length);
        }

        public override string GetFieldName()
        {
            return String.Format("{0}", Field.ToString());
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public int Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return _field.ContainsTableReference(tableReference);
        }

        public override bool ContainsField(HqlField field)
        {
            return _field.Equals(field);
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference);
        }

        HqlField _field;
        int _start;
        int _length;
    }

    class HqlScalarToDate : HqlScalarInternal
    {
        public HqlScalarToDate(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.TO_DATE;
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar is HqlScalarToDate))
                return false;

            HqlScalarToDate s = (HqlScalarToDate)scalar;

            return (Field.Equals(s.Field) && Format.Equals(s.Format));
        }

        public override bool HasFunction
        {
            get
            {
                if (Field == null) return false;
                return Field.HasFunction;
            }
        }

        public override HqlField Field
        {
            get { return _field; }
        }

        private HqlField Format
        {
            get { return _format; }
        }

        private HqlField Outformat
        {
            get { return _outformat; }
        }

        public override void AddToken(HqlToken token)
        {
            if (_field == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _field = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _field = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _field = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _field = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else if (_format == null)
            {
                if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _format = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _format = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _format = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else if (_outformat == null)
            {
                if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _outformat = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _outformat = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _outformat = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else
            {
                throw new Exception("Too many arguments given in SCALAR parameters");
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            string datestr = Field.GetValue(record).ToString();
            string format = Format.GetValue(record).ToString();
            string outformat = (Outformat == null ? null : Outformat.GetValue(record).ToString());

            return Evaluate(datestr, format, outformat);
        }

        public object Evaluate(string datestr, string format, string outformat)
        {
            DateTime date = DateTime.Now;
            bool validDate = false;
            if (format.Equals("?"))
            {
                if (!validDate) { validDate = DateTime.TryParse(datestr, null, dateStyle, out date); }
                if (!validDate) { validDate = DateTime.TryParseExact(datestr, "yyyyMMdd", null, dateStyle, out date); }
                if (!validDate) { validDate = DateTime.TryParseExact(datestr, "yyyy/MM/dd", null, dateStyle, out date); }
                if (!validDate) { validDate = DateTime.TryParseExact(datestr, "yyyy-MM-dd", null, dateStyle, out date); }
                if (!validDate) { validDate = DateTime.TryParseExact(datestr, "MM-dd-YYYY", null, dateStyle, out date); }
                if (!validDate) { validDate = DateTime.TryParseExact(datestr, "YY-mmm", null, dateStyle, out date); }
            }
            else
            {
                validDate = DateTime.TryParseExact(datestr, format, null, dateStyle, out date);
            }

            if (validDate)
                return Evaluate(date, outformat);
            
            Console.Error.WriteLine(String.Format("HqlScalarToDate|Warning|Unable to parse |{0}| as DateTime", datestr));
            return datestr;
        }

        public object Evaluate(DateTime date, string outformat)
        {
            if (outformat == null)
                return date; 
            
            string s = date.ToString(outformat);
            return s;
        }

        public override void Validate()
        {
            if (Field == null || Format == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
        }

        public override string GetScalarName()
        {
            return "to_date";
        }

        public override string GetFullName()
        {
            return String.Format("{0}({1}, {2})", GetScalarName(), Field.ToString(), Format.ToString());
        }

        public override string GetFieldName()
        {
            return String.Format("{0}", Field.ToString());
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return _field.ContainsTableReference(tableReference) || _field.ContainsTableReference(tableReference);
        }

        public override bool ContainsField(HqlField field)
        {
            return _field.ContainsField(field) || _field.ContainsField(field);
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference) || _field.ContainsRownum(OnlyWithoutTableReference);
        }

        HqlField _field;
        HqlField _format;
        HqlField _outformat;
        static System.Globalization.DateTimeStyles dateStyle = System.Globalization.DateTimeStyles.AssumeLocal | 
                        System.Globalization.DateTimeStyles.AllowInnerWhite |
                        System.Globalization.DateTimeStyles.AllowTrailingWhite |
                        System.Globalization.DateTimeStyles.AllowWhiteSpaces;
    }

    // Zero fields

    abstract class HqlScalarInternalZeroField : HqlScalarInternal
    {
        public HqlScalarInternalZeroField(HqlTokenProcessor processor)
            : base(processor)
        {
            _field = new HqlField(HqlFieldType.LITERAL_STRING, String.Empty);
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            return scalar.GetType() == this.GetType();
        }

        public override bool HasFunction { get { return false; } }

        public override HqlField Field { get { return null; } }

        public override void AddToken(HqlToken token)
        {
            throw new Exception("Too many arguments given in SCALAR parameters");
        }

        public override object Evaluate(HqlRecord record)
        {
            return Evaluate();
        }

        public abstract object Evaluate();

        public override void Validate()
        {
            return;
        }

        public override string GetFullName()
        {
            return String.Format("{0}()", GetScalarName());
        }

        public override string GetFieldName()
        {
            return String.Empty;
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return false;
        }

        public override void Cleanup()
        {
            return;
        }

        public override bool ContainsField(HqlField field)
        {
            return false;
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return false;
        }
        
        HqlField _field;
    }

    class HqlScalarNow : HqlScalarInternalZeroField
    {
        public HqlScalarNow(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.NOW;
        }

        public override object Evaluate()
        {
            return DateTime.Now.ToString();
        }

        public override string GetScalarName() { return "now"; }
    }

    class HqlScalarPI : HqlScalarInternalZeroField
    {
        public HqlScalarPI(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.PI;
        }

        public override object Evaluate()
        {
            return Math.PI.ToString("0.0000");
        }

        public override string GetScalarName() { return "pi"; }
    }

    // Single field

    abstract class HqlScalarInternalSingleField : HqlScalarInternal
    {
        public HqlScalarInternalSingleField(HqlTokenProcessor processor)
            : base(processor)
        {            
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar.GetType() == this.GetType()))
                return false;

            return Field.Equals(scalar.Field);
        }

        public override bool HasFunction
        {
            get
            {
                if (Field == null) return false;
                return Field.HasFunction;
            }
        }

        public override HqlField Field
        {
            get { return _field; }
        }

        public override void AddToken(HqlToken token)
        {
            if (_field == null)
            {
                if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
                {
                    _field = new HqlField(HqlFieldType.STAR);
                }
                else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
                {
                    _field = token.Field;
                }
                else if (token.WordType == HqlWordType.LITERAL_STRING)
                {
                    _field = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
                }
                else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
                {
                    _field = token.Field;
                }
                else
                {
                    throw new Exception("Expected a valid field in SCALAR parameters");
                }
            }
            else
            {
                throw new Exception("Too many arguments given in SCALAR parameters");
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            return Evaluate(Field.GetValue(record));
        }

        //public override object Evaluate(string s)
        //{
        //    return s.Length;
        //}

        public override void Validate()
        {
            if (Field == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
        }

        public override string GetFullName()
        {
            return String.Format("{0}({1})", GetScalarName(), Field.ToString());
        }

        public override string GetFieldName()
        {
            return String.Format("{0}", Field.ToString());
        }

        public override bool HasMultipleFields { get { return false; } }

        public override bool ContainsTableReference(string tableReference)
        {
            return _field.ContainsTableReference(tableReference);
        }

        public override void Cleanup()
        {
            _field.Cleanup();
        }

        public override bool ContainsField(HqlField field)
        {
            return _field.Equals(field);
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference);
        }

        protected HqlField _field;
    }

    class HqlScalarLen : HqlScalarInternalSingleField
    {
        public HqlScalarLen(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.LENGTH;
        }

        public override string GetScalarName() { return "len"; }

        public override object Evaluate(string s) { return s.Length; }
        public override object Evaluate(Decimal s) { return Evaluate(HqlCategory.PrintDefaultDecimal(s)); }
        public override object Evaluate(Int64 s) { return Evaluate(s.ToString()); }
    }

    class HqlScalarUpper : HqlScalarInternalSingleField
    {
        public HqlScalarUpper(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.UPPER;
        }

        public override object Evaluate(string s) { return s.ToUpper(); }
        public override string GetScalarName() { return "upper"; }
    }

    class HqlScalarLower : HqlScalarInternalSingleField
    {
        public HqlScalarLower(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.LOWER;
        }

        public override object Evaluate(string s) { return s.ToLower(); }
        public override string GetScalarName() { return "lower"; }
    }

    class HqlScalarTrimLeft : HqlScalarInternalSingleField
    {
        public HqlScalarTrimLeft(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.TRIMLEFT;
        }

        public override object Evaluate(string s) { return s.TrimStart(); }
        public override string GetScalarName() { return "trimleft"; }
    }

    class HqlScalarTrimRight : HqlScalarInternalSingleField
    {
        public HqlScalarTrimRight(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.TRIMRIGHT;
        }

        public override object Evaluate(string s) { return s.TrimEnd(); }
        public override string GetScalarName() { return "trimright"; }
    }

    class HqlScalarTrim : HqlScalarInternalSingleField
    {
        public HqlScalarTrim(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.TRIM;
        }

        public override object Evaluate(string s) { return s.Trim(); }
        public override string GetScalarName() { return "trim"; }
    }

    class HqlScalarGetPrintables : HqlScalarInternalSingleField
    {
        public HqlScalarGetPrintables(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.GETPRINTABLES;
        }

        public override object Evaluate(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; ++i)
            {
                if (!Char.IsControl(s[i]))
                    sb.Append(s[i]);
            }
            return sb.ToString();
        }

        public override string GetScalarName() { return "getprintables"; }
    }

    // Multi fields

    abstract class HqlScalarMultiField : HqlScalarInternal
    {
        public HqlScalarMultiField(HqlTokenProcessor processor)
            : base(processor)
        {
            // TODO
        }

        public override bool Equals(HqlScalarInternal scalar)
        {
            if (!(scalar.GetType() == this.GetType()))
                return false;

            HqlScalarMultiField s = (HqlScalarMultiField)scalar;
            if (Fields.Length != s.Fields.Length)
                return false;
            for (int i = 0; i < Fields.Length; ++i)
            {
                if (!Fields[i].Equals(s.Fields[i]))
                    return false;
            }

            return true;
        }

        public override bool HasFunction
        {
            get
            {
                for (int i = 0; i < _fields.Length; ++i)
                {
                    if (_fields[i].HasFunction)
                        return true;
                }
                return false;
            }
        }

        public override HqlField Field
        {
            get { return null; }
        }

        public override HqlField[] Fields
        {
            get { return _fields; }
        }

        public override void AddToken(HqlToken token)
        {
            if (_fields == null)
            {
                _fields = new HqlField[1];
            }
            else
            {
                HqlField[] tempfields = new HqlField[_fields.Length + 1];
                _fields.CopyTo(tempfields, 0);
                _fields = tempfields;
            }

            if (token.WordType == HqlWordType.KEYWORD && token.Keyword == HqlKeyword.STAR)
            {
                _fields[_fields.Length - 1] = new HqlField(HqlFieldType.STAR);
            }
            else if (token.WordType == HqlWordType.FIELD || token.WordType == HqlWordType.SCALAR)
            {
                _fields[_fields.Length - 1] = token.Field;
            }
            else if (token.WordType == HqlWordType.LITERAL_STRING)
            {
                _fields[_fields.Length - 1] = new HqlField(HqlFieldType.LITERAL_STRING, token.Data);
            }
            else if (token.WordType == HqlWordType.INT)
            {
                _fields[_fields.Length - 1] = new HqlField(HqlFieldType.LITERAL_INT, (Int64)token.Parsed);
            }
            else if (token.WordType == HqlWordType.UNKNOWN && Processor.CheckForTokenReference(ref token))
            {
                _fields[_fields.Length - 1] = token.Field;
            }
            else
            {
                throw new Exception("Expected a valid field in SCALAR parameters");
            }
        }

        public override object Evaluate(HqlRecord record)
        {
            return Evaluate(Field.GetValue(record));
        }

        //public override string GetScalarName() { return "decode"; }

        public override void Validate()
        {
            if (Fields == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
        }

        public override string GetFullName()
        {
            StringBuilder sb = new StringBuilder(GetScalarName());
            sb.Append("(");
            for (int i = 0; i < Fields.Length; ++i)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(Fields[i].ToString());
            }
            sb.Append(")");

            return sb.ToString();
        }

        public override string GetFieldName()
        {
            return null;
        }

        //public string[] GetFieldNames()
        //{
        //    string[] results = new string[Fields.Length];
        //    for (int i = 0; i < Fields.Length; ++i)
        //    {
        //        results[i] = Fields[i].ToString();
        //    }
        //    return results;
        //}


        public override bool HasMultipleFields { get { return true; } }

        public override bool ContainsTableReference(string tableReference)
        {
            for (int i = 0; i < _fields.Length; ++i)
            {
                if (_fields[i].ContainsTableReference(tableReference))
                    return true;
            }
            return false;
        }

        public override void Cleanup()
        {
            for (int i = 0; i < _fields.Length; ++i)
            {
                _fields[i].Cleanup();
            }
            _fields = null;
        }

        public override bool ContainsField(HqlField field)
        {
            for (int i = 0; i < _fields.Length; ++i)
            {
                if (_fields[i].ContainsField(field))
                    return true;
            }
            return false;
        }

        public override bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            for (int i = 0; i < _fields.Length; ++i)
            {
                if (_fields[i].ContainsRownum(OnlyWithoutTableReference))
                    return true;
            }
            return false;
        }

        HqlField[] _fields;
    }

    class HqlScalarConcatenate : HqlScalarMultiField
    {
        public HqlScalarConcatenate(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.CONCATENTATE;
        }

        public override object Evaluate(HqlRecord record)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Fields.Length; ++i)
            {
                string s = Fields[i].GetValue(record).ToString();
                sb.Append(s);
            }
            return sb.ToString();
        }

        public override string GetScalarName() { return "concat"; }
    }

    class HqlScalarConcatenateWs : HqlScalarMultiField
    {
        public HqlScalarConcatenateWs(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.CONCATENTATE_WS;
        }

        public override object Evaluate(HqlRecord record)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < Fields.Length; ++i)
            {
                if (i > 1)
                    sb.Append(Fields[0].GetValue(record).ToString());
                string s = Fields[i].GetValue(record).ToString();
                sb.Append(s);
            }
            return sb.ToString();
        }

        public override string GetScalarName() { return "concat_ws"; }
    }

    class HqlScalarDecode : HqlScalarMultiField
    {
        public HqlScalarDecode(HqlTokenProcessor processor)
            : base(processor)
        {
            _type = HqlScalarType.DECODE;
        }

        public override object Evaluate(HqlRecord record)
        {
            // must have at least 3 entries
            object o = Fields[0].GetValue(record);
            for (int i = 1; ; i += 2)
            {
                // if last entry
                if (i == Fields.Length - 1)
                {
                    object ret = Fields[i].GetValue(record);
                    return ret;
                }
                else
                {
                    object test = Fields[i].GetValue(record);
                    if (HqlCompareToken.Equals(o, test))
                    {
                        object ret = Fields[i+1].GetValue(record);
                        return ret;
                    }
                }
            }
        }

        public override void Validate()
        {
            if (Fields == null)
                throw new Exception("Expected a valid field in SCALAR parameters");
            if (Fields.Length < 3)
                throw new Exception("Not enough fields in SCALAR");
        }

        public override string GetScalarName() { return "decode"; }



    }
}