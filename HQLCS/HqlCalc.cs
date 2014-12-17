using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    abstract class HqlCalc
    {
        public HqlCalc(HqlCalcOptions options)
        {
            _options = options;
        }

        abstract public void Add(object o);
        abstract public void Add(HqlCalc calc);
        abstract public string GetPrintValue();
        abstract public object GetValue();

        protected HqlCalcOptions _options;
    }

    abstract class HqlCalcInt : HqlCalc
    {
        public HqlCalcInt(HqlCalcOptions options) : base(options) { }

        public override void Add(object o)
        {
            if (o is string)
            {
                Add(Int64.Parse(o.ToString()));
            }
            else if (o is int)
            {
                Add((int)o);
            }
            else if (o is Int64)
            {
                Add((Int64)o);
            }
            else
            {
                throw new ArgumentException("Expected an integer to HqlCalcInt");
            }
        }

        public override string GetPrintValue()
        {
            // TODO based on options, print out
            return GetResult().ToString();
        }

        public override object GetValue()
        {
            Int64? t = GetResult();
            if (t.HasValue)
                return t.Value;
            return null;
        }

        abstract public void Add(int o);
        abstract public void Add(Int64 o);
        abstract protected Int64? GetResult();
    }

    class HqlCount : HqlCalcInt
    {
        public HqlCount(HqlCalcOptions options) : base(options)
        {
            count = 0;
        }

        public override void Add(object o)
        {
            if (o == null || o.ToString().Length == 0)
                return;
            Add(1);
        }

        public override void Add(int o)
        {
            Add((Int64)o);
        }
        public override void Add(Int64 o)
        {
            if (!count.HasValue)
                count = 0;
            count += 1;
        }

        public override void Add(HqlCalc calc)
        {
            if (calc is HqlCount)
                count += ((HqlCount)calc).count;
            else
                throw new ArgumentException("Unable to add type HqlCalc to HqlCount");
        }

        protected override Int64? GetResult()
        {
            return count;
        }

        protected Int64? count;
    }

    class HqlMax : HqlCalcDec
    {
        public HqlMax(HqlCalcOptions options) : base(options)
        {
            max = null;
        }

        public override void Add(decimal o)
        {
            if (!max.HasValue)
                max = o;
            else if (o > max.Value)
                max = o;
        }

        public override void Add(HqlCalc calc)
        {
            if (calc is HqlMax)
            {
                Add(((HqlMax)calc).max);
            }
            else
                throw new ArgumentException("Unable to add type HqlCalc to HqlMax");
        }

        protected override decimal? GetResult()
        {
            return max;
        }

        protected decimal? max;
    }

    class HqlMin : HqlCalcDec
    {
        public HqlMin(HqlCalcOptions options) : base(options)
        {
            min = null;
        }

        public override void Add(decimal o)
        {
            if (!min.HasValue)
                min = o;
            else if (o < min.Value)
                min = o;
        }

        public override void Add(HqlCalc calc)
        {
            if (calc is HqlMin)
            {
                Add(((HqlMin)calc).min);
            }
            else
                throw new ArgumentException("Unable to add type HqlCalc to HqlMin");
        }

        protected override decimal? GetResult()
        {
            if (min.HasValue)
                return min;
            return null;
        }

        protected decimal? min;
    }

    class HqlSum : HqlCalcDec
    {
        public HqlSum(HqlCalcOptions options) : base(options) { sum = null; }

        public void Add(int o)
        {
            Add((decimal)o);
        }
        public void Add(Int64 o)
        {
            if (!sum.HasValue)
                sum = 0;
            sum += (decimal)o;
        }
        public override void Add(decimal o)
        {
            if (!sum.HasValue)
                sum = 0;
            sum += o;
        }

        public override void Add(HqlCalc calc)
        {
            if (calc is HqlSum)
            {
                Add(((HqlSum)calc).sum);
            }
            else
            {
                throw new ArgumentException("Unable to add type HqlCalc to HqlCount");
            }
        }

        protected override decimal? GetResult()
        {
            return sum;
        }
        protected decimal? sum;
    }

    abstract class HqlCalcDec : HqlCalc
    {
        public HqlCalcDec(HqlCalcOptions options) : base(options) { }

        public override void Add(object o)
        {
            try
            {
                if (o is string)
                {
                    Add(Decimal.Parse(o.ToString(), System.Globalization.NumberStyles.Currency));
                }
                else if (o is Int64)
                {
                    Add((Int64)o);
                }
                else if (o is int || o is Int32)
                {
                    Decimal d = (decimal)(Int32)o;
                    Add(d);
                }
                else if (o is decimal)
                {
                    Add((decimal)o);
                }
                else
                {
                    throw new ArgumentException("Expected a decimal to HqlCalcDec");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Unable to handle object type |{0}| of |{1}|", o.GetType().ToString(), o.ToString()), ex);
            }
        }

        public override string GetPrintValue()
        {
            decimal? d = GetResult();
            if (!d.HasValue)
                return GetPrintValue(0.0M);
            return GetPrintValue(d.Value);
        }

        private string GetPrintValue(decimal d)
        {
            if (_options == null)
            {
                return HqlCategory.PrintDefaultDecimal(d);
            }
            else
                return d.ToString(_options.DecimalPrintString);
        }

        public override object GetValue()
        {
            decimal? d = GetResult();
            if (!d.HasValue)
                return null;
            return d.Value;
        }

        abstract public void Add(decimal o);
        abstract protected decimal? GetResult();
    }

    class HqlAvg : HqlCalcDec
    {
        public HqlAvg(HqlCalcOptions options) : base(options) { sum = null; }

        public override void Add(decimal o)
        {
            if (!sum.HasValue)
                sum = 0.0M;
            sum += o;
            count++;
        }

        public override void Add(HqlCalc calc)
        {
            if (calc is HqlAvg)
            {
                HqlAvg c = (HqlAvg)calc;
                if (c.sum.HasValue)
                {
                    if (!sum.HasValue)
                        sum = 0.0M;
                    sum += c.sum;
                    count += c.count;
                }
            }
            else
            {
                throw new ArgumentException("Unable to add type HqlCalc to HqlAvg");
            }
        }

        protected override decimal? GetResult()
        {
            if (!sum.HasValue)
                return sum;
            return sum.Value / (decimal)count;
        }

        protected decimal? sum;
        protected int count;
    }

    //http://smallcode.weblogs.us/oldblog/2006/11/27/calculate-standard-deviation-in-one-pass/
    //double std_dev2(double a[], int n) {
    //    if(n == 0)
    //        return 0.0;
    //    double sum = 0;
    //    double sq_sum = 0;
    //    for(int i = 0; i < n; ++i) {
    //       sum += a[i];
    //       sq_sum += a[i] * a[i];
    //    }
    //    double mean = sum / n;
    //    double variance = sq_sum / n - mean * mean;
    //    return sqrt(variance);
    //}
    //
    // http://www.cs.berkeley.edu/~mhoemmen/cs194/Tutorials/variance.pdf
    // better algorithm (see 2.2.2) (implemented below)
    //
    class HqlStdev : HqlCalcDec
    {
        public HqlStdev(HqlCalcOptions options) : base(options)
        {
            Q = null;
            M = null;
            //count = 0; // unnecessary
        }

        public override void Add(decimal o)
        {
            if (count == 0)
            {
                Q = 0.0M;
                M = o;
            }
            else
            {
                int k = count + 1;
                Q = Q + ((k - 1) * (o - M) * (o - M)) / k;
                M = M + (o - M)/k;
            }
            count++;            
        }

        public override void Add(HqlCalc calc)
        {
            throw new ArgumentException("Unable to add to HqlStdev");
        }

        protected override decimal? GetResult()
        {
            if (count < 2)
                return 0.0M;
            return (decimal)Math.Sqrt(((double)Q.Value / (double)(count-1)));

        }

        int count;
        protected decimal? M;
        protected decimal? Q;
    }
}