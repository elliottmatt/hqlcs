using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    public enum HqlFunctionType
    {
        SUM,
        MAX,
        MIN,
        COUNT,
        AVG,
        STDEV,
    }
    
    class HqlFunction
    {
        ///////////////////////
        // Static Functions

        static public HqlFunctionType ResolveFunctionType(string s)
        {
            switch (s.ToLower())
            {
                case "sum":
                case "summation":
                    return HqlFunctionType.SUM;
                case "max":
                case "maximum":
                    return HqlFunctionType.MAX;
                case "min":
                case "minimum":
                    return HqlFunctionType.MIN;
                case "avg":
                case "average":
                    return HqlFunctionType.AVG;
                case "count":
                    return HqlFunctionType.COUNT;
                case "stdev":
                    return HqlFunctionType.STDEV;
                default:
                    throw new Exception("Unknown FUNCTION type");
            }
        }

        ///////////////////////
        // Constructors
        
        public HqlFunction(HqlFunctionType type, HqlField field)
        {
            _type = type;
            _field = field;
            //_options = null; // unnecessary
        }

        ///////////////////////
        // Overridden functions

        public override string ToString()
        {
            return GetFullName();
        }

        ///////////////////////
        // Public 

        public string GetFunctionName()
        {
            switch (_type)
            {
                case HqlFunctionType.SUM:
                    return "sum";
                case HqlFunctionType.MAX:
                    return "max";
                case HqlFunctionType.MIN:
                    return "min";
                case HqlFunctionType.COUNT:
                    return "count";
                case HqlFunctionType.AVG:
                    return "avg";
                case HqlFunctionType.STDEV:
                    return "stdev";
                default:
                    throw new Exception("Unknown FUNCTION name");
            }            
        }
        
        public string GetFullName()
        {
            return String.Format("{0}({1})", GetFunctionName(), Field.ToString());
        }
        
        public string GetFieldName()
        {
            if (Field.HasFunction)
                return Field.GetFieldName();
            return String.Format("{0}", Field.ToString());
        }

        public HqlCalc CreateCalcObject()
        {
            switch (FuncType)
            {
                case HqlFunctionType.SUM:
                    return new HqlSum(_options);
                case HqlFunctionType.MAX:
                    return new HqlMax(_options);
                case HqlFunctionType.MIN:
                    return new HqlMin(_options);
                case HqlFunctionType.COUNT:
                    return new HqlCount(_options);
                case HqlFunctionType.AVG:
                    return new HqlAvg(_options);
                case HqlFunctionType.STDEV:
                    return new HqlStdev(_options);
                default:
                    throw new Exception("Unknown FUNCTION name");
            }            
        }

        //public object GetFunctionValue(HqlRecord record)
        //{
        //    return Field.GetValue(record);
        //}

        public object GetValue(HqlRecord record)
        {
            return Field.GetValue(record);
        }

        public bool Equals(HqlFunction func)
        {
            if (FuncType != func.FuncType)
                return false;
            if (!Field.Equals(func.Field))
                return false;
            if (!(_options == null && func._options == null) && !_options.Equals(func._options))
                return false;
            return true;
        }

        public void SetOption(HqlToken token)
        {
            if (_options == null)
                _options = new HqlCalcOptions();

            if (token.WordType == HqlWordType.INT)
                _options.DecimalPrintPlaces = token.ParsedAsInt;
            else if (token.WordType == HqlWordType.LITERAL_STRING)
                _options.DecimalPrintFormat = (string)token.Data;
            else
                throw new Exception("Unknown option to Function");
        }

        public void Cleanup()
        {
            _field.Cleanup();
        }

        public bool ContainsField(HqlField field)
        {
            return _field.ContainsField(field);
        }

        public bool ContainsRownum(bool OnlyWithoutTableReference)
        {
            return _field.ContainsRownum(OnlyWithoutTableReference);
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        public bool HasScalar
        {
            get
            {
                return Field.HasScalar;
            }
        }

        ///////////////////////
        // Getters/Setters

        public HqlField Field
        {
            get { return _field; }
            set { _field = value; }
        }

        public HqlFunctionType FuncType
        {
            get { return _type; }
        }

        ///////////////////////
        // Variables


        HqlFunctionType _type;
        HqlField _field;
        HqlCalcOptions _options;
    }
}
