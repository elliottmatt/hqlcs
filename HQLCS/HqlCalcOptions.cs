using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlCalcOptions
    {
        public HqlCalcOptions()
        {
            DecimalPrintPlaces = 2;
        }

        public int DecimalPrintPlaces
        {
            set
            {
                _decimalPrintPlaces = value;
                if (_decimalPrintPlaces < 0)
                    throw new InvalidOperationException("Cannot print out fewer than zero decimal places");
                else if (_decimalPrintPlaces == 0)
                    _calculatedPrintString = "0";
                else
                {
                    _calculatedPrintString = "0." + new string('0', _decimalPrintPlaces);
                }
            }
        }

        public string DecimalPrintFormat
        {
            //get { return _decimalPrintFormat; }
            set
            {
                _decimalPrintFormat = value;
                //http://msdn.microsoft.com/en-us/library/dwhawy9k.aspx
                switch (_decimalPrintFormat)
                {
                        //dollarize
                    case "D": _calculatedPrintString = "C"; break;
                    case "C": _calculatedPrintString = "N"; break;
                    case "P": _calculatedPrintString = "P"; break;
                    case "X": _calculatedPrintString = "X"; break;
                    default:
                        throw new ArgumentException("Unknown character format for decimal format");
                }
            }
        }

        public string DecimalPrintString
        {
            get { return _calculatedPrintString; }
        }

        int _decimalPrintPlaces;
        string _calculatedPrintString;
        string _decimalPrintFormat;
    }
}
