using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlToken
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlToken(HqlWordType type)
        {
            _type = type;
        }
        
        public HqlToken(HqlWordType type, string data)
        {
            _type = type;
            _data = data;
        }

        public HqlToken(HqlWordType type, HqlField field)
        {
            _type = type;
            _field = field;
        }

        ///////////////////////
        // Overridden functions

        public override string ToString()
        {
            switch (_type)
            {
                case HqlWordType.UNKNOWN:
                case HqlWordType.LITERAL_STRING:
                case HqlWordType.TEXT:
                    return Data;
                case HqlWordType.END_OF_LINE:
                    return "\\n";
                case HqlWordType.FIELD:
                case HqlWordType.FUNCTION:
                case HqlWordType.SCALAR:
                    return Field.ToString();
                case HqlWordType.FLOAT:
                case HqlWordType.INT:
                case HqlWordType.ROWNUM:
                    return Parsed.ToString();
                case HqlWordType.KEYWORD:
                    return Data;
                case HqlWordType.NULL:
                    return "NULL";
                default:
                    throw new Exception("Unknown type of token");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is HqlToken)
            {
                HqlToken t = (HqlToken)obj;
                return Equals(t);
            }
            
            return false;
        }

        public bool Equals(HqlToken token)
        {
            if (_type != token._type)
                return false;

            if (_field == null && token._field == null)
            { } 
            else if (!_field.Equals(token._field))
                return false;

            if (!_keyword.Equals(token._keyword))
                return false;

            if (_data == null && token._data == null)
            { }
            else if (!_data.Equals(token._data))
                return false;

            if (_parsed == null && token._parsed == null)
            { } 
            else if (!_parsed.Equals(token._parsed))
                return false;

            if (_hadEquation != token._hadEquation)
                return false;

            if (_hadQuotes != token._hadQuotes)
                return false;

            if (_hadTicky != token._hadTicky)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        ///////////////////////
        // Public 

        public void UnsetFlags()
        {
            _hadTicky = false;
            _hadQuotes = false;
            _hadEquation = false;
        }

        public void SetTicky(bool unsetRest)
        {
            if (unsetRest)
                UnsetFlags();
            _hadTicky = true;
        }

        public void SetQuote(bool unsetRest)
        {
            if (unsetRest)
                UnsetFlags();
            _hadQuotes = true;
        }

        public void SetEquation(bool unsetRest)
        {
            if (unsetRest)
                UnsetFlags();
            _hadEquation = true;
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        public bool HadEquation
        {
            get { return _hadEquation; }
        }

        public bool HadQuotes
        {
            get { return _hadQuotes; }
        }

        public bool HadTicky
        {
            get { return _hadTicky; }
        }

        public int LengthOfDataWithOutlining
        {
            get { return Data.Length + AdditionalOutlineCharacters; }
        }

        public bool HadOutlingCharacters
        {
            get { return (HadTicky || HadQuotes || HadEquation); }
        }

        public int AdditionalOutlineCharacters
        {
            get
            {
                if (HadOutlingCharacters)
                    return 2;
                return 0;
            }
        }

        public bool NeedsEvaluation
        {
            get
            {
                switch (WordType)
                {
                    case HqlWordType.LITERAL_STRING:
                    case HqlWordType.TEXT:
                    case HqlWordType.INT:
                    case HqlWordType.FLOAT:
                    case HqlWordType.NULL:
                        return false;
                    case HqlWordType.KEYWORD:
                    case HqlWordType.ROWNUM:
                    case HqlWordType.FIELD:
                    case HqlWordType.SCALAR:
                    case HqlWordType.FUNCTION:
                        return true;
                    default:
                        throw new Exception("Unknown token type");
                }
            }
        }

        public object EvaluatedData
        {
            get
            {
                switch (WordType)
                {
                    case HqlWordType.LITERAL_STRING:
                    case HqlWordType.TEXT:
                        return Data;
                    case HqlWordType.INT:
                    case HqlWordType.FLOAT:
                        return Parsed;
                    case HqlWordType.NULL:
                        return String.Empty;
                    case HqlWordType.FIELD:
                    case HqlWordType.SCALAR:
                    case HqlWordType.FUNCTION:
                    case HqlWordType.KEYWORD:
                        throw new Exception("Cannot evaluate token internally");
                    default:
                        throw new Exception("Unknown token type");
                }
            }
        }

        public void Cleanup()
        {
            if (_field != null)
            {
                _field.Cleanup();
                _field = null;
            }
            _parsed = null;
        }

        ///////////////////////
        // Getters/Setters

        public HqlField Field
        {
            get { return _field; }
            //set { _field = value; }
        }

        public HqlWordType WordType
        {
            get { return _type; }
            set { _type = value; }
        }
        
        public HqlKeyword Keyword
        {
            get { return _keyword; }
            set { _keyword = value; }
        }
        
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public object Parsed
        {
            get { return _parsed; }
            set { _parsed = value; }
        }

        public int ParsedAsInt
        {
            get { return (int)(Int32)ParsedAsInt64; }
        }

        public Int64 ParsedAsInt64
        {
            get { return (Int64)_parsed; }
        }

        ///////////////////////
        // Variables

        HqlField _field;
        HqlWordType _type;
        HqlKeyword _keyword;
        string _data;
        object _parsed;

        bool _hadQuotes;
        bool _hadTicky;
        bool _hadEquation;
    }
}
