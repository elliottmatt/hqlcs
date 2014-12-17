using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlValuesComparer : IComparer<HqlValues>, IEnumerator<HqlValues>, IDisposable
    {
        enum HqlCompareMode
        {
            SORT_MODE,
            SEARCH_MODE
        };

        public HqlValuesComparer(HqlDataSource prior, HqlWhere onJoin, HqlWhere where) //, HqlWith settings)
        {
            _prior = prior;
            _onJoin = onJoin;
            _where = where;
            //_settings = settings;
            _orderOfSortFields = new ArrayList();
            _mode = HqlCompareMode.SORT_MODE;

            _onJoin.CreateSortFields(this);
            _where.CreateSortFields(this);

            _currentPrior = -1;
            _joinMethod = HqlJoinMethod.SORT_COMPARE;
        }

        public int Compare(HqlValues x, HqlValues y)
        {
            switch (_mode)
            {
                case HqlCompareMode.SEARCH_MODE:
                    return Search(x, y);
                case HqlCompareMode.SORT_MODE:
                    return Sort(x, y);
                default:
                    throw new Exception("Unknown compare mode");
            }
        }

        /// <summary>
        /// This will go through the list of joining records (x is left-side file) (y is right-side file)
        /// and run both pieces of data through the join.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int Sort(HqlValues x, HqlValues y)
        {
            int? ret;
            for (int i = 0; i < _orderOfSortFields.Count; ++i)
            {
                object item = _orderOfSortFields[i];
                if (!(item is HqlCompareToken))
                    throw new Exception("Unknown object in SortFields");

                HqlCompareToken c = (HqlCompareToken)item;
                
                ret = CompareHqlValues(x, y, c.Token1, c.Compare);
                if (ret.HasValue && ret != 0)
                    return ret.Value;

                ret = CompareHqlValues(x, y, c.Token2, c.Compare);
                if (ret.HasValue && ret != 0)
                    return ret.Value;
            }
            return 0;
        }

        private int Search(HqlValues a, HqlValues b)
        {
            object oa = null;
            object ob = null;
            int result = 0;

            for (int i = 0; i < _orderOfSortFields.Count; ++i)
            {
                object item = _orderOfSortFields[i];
                if (!(item is HqlCompareToken))
                    throw new Exception("Unknown object in SortFields");

                HqlCompareToken ctoken = (HqlCompareToken)item;
                if (!GetValues(b, a, ctoken, ref ob, ref oa) && !GetValues(a, b, ctoken, ref oa, ref ob))
                {
                    continue;
                }

                int ret = HqlCompareToken.CompareTo(oa, ob);
                if (ret != 0)
                {
                    result = ret;
                    break;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Must be a SINGLE CompareToken
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="item"></param>
        /// <param name="ox"></param>
        /// <param name="oy"></param>
        /// <returns></returns>
        static private bool GetValues(HqlValues x, HqlValues y, HqlCompareToken item, ref object ox, ref object oy)
        {
            object a;
            object b;

            if (!x.HasValue(item.Token1, out a))
                return false;
            if (!y.HasValue(item.Token2, out b))
                return false;

            ox = a;
            oy = b;

            return true;
        }

        static private int? CompareHqlValues(HqlValues x, HqlValues y, HqlToken item, HqlToken compareType)
        {
            object ox;
            object oy;

            if (!x.HasValue(item, out ox))
                return null;
            if (!y.HasValue(item, out oy))
                throw new Exception("Error! How come it doesn't have it in Y?");

            int ret = HqlCompareToken.CompareTo(ox, oy);
            //switch (compareType)
            //{
            //    // TODO
            //}
            return ret;
        }

        public void Add(HqlCompareToken c)
        {
            if (c.CompareType != HqlCompareTokenType.SINGLE)
                return;

            switch (c.Compare.Data)
            {
                case "=":
                case "like":
                    break;
                default:
                    return;
            }

            if (c.ContainsRownum(false))
                return;
            
            _orderOfSortFields.Add(c);
        }

        public HqlValues CombinedValues
        {
            get { return _combinedValues; }
        }

        public int CountOfJoinFields
        {
            get { return _orderOfSortFields.Count; }
        }

        public HqlJoinMethod JoinMethod
        {
            get { return _joinMethod; }
            set { _joinMethod = value; }
        }

        void IDisposable.Dispose()
        {
            // TODO not sure what i can destroy here
        }

        public void SetValues(HqlValues compareValues)
        {
            _compareValues = compareValues;
        }

        public HqlValues Current
        {
            get { return _prior.Lines[_currentPrior]; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { throw new Exception("Use the other Current"); }
        }

        public bool MoveNext()
        {
            switch (JoinMethod)
            {
                case HqlJoinMethod.LINEAR_COMPARE:
                    return MoveNext_LinearCompare();
                case HqlJoinMethod.SORT_COMPARE:
                    return MoveNext_SortCompare();
                case HqlJoinMethod.RETURN_ALL:
                    return MoveNext_ReturnAllUnused();
                default:
                    throw new Exception("Unknown type of compare");
            }
        }

        private bool MoveNext_LinearCompare()
        {
            if (_currentPrior == -1)
            {
                if (_prior == null || _prior.Lines == null || _prior.Lines.Count == 0)
                    return false;
                _currentPrior = 0;
            }
            else
            {
                _currentPrior++;
            }

            for (; _currentPrior < _prior.Lines.Count; ++_currentPrior)
            {
                _combinedValues = new HqlValues(_impactedFields, _prior.Lines[_currentPrior], _compareValues);

                if (_onJoin.EvaluateJoin(_combinedValues, false)) // && _where.EvaluateJoin(_combinedValues))
                    return true;
            }

            return false;
        }

        private bool MoveNext_ReturnAllUnused()
        {
            if (_currentPrior == -1)
            {
                if (_prior == null || _prior.Lines == null || _prior.Lines.Count == 0)
                    return false;
                _currentPrior = 0;
            }
            else
            {
                _currentPrior++;
            }

            for (; _currentPrior < _prior.Lines.Count; ++_currentPrior)
            {
                HqlValues priorvalues = _prior.Lines[_currentPrior];
                if (priorvalues.IsUsed)
                    continue;

                _combinedValues = new HqlValues(_impactedFields, _prior.Lines[_currentPrior], _compareValues);
                return true;
            }

            return false;
        }

        private bool MoveNext_SortCompare()
        {
            if (_currentPrior == -1)
            {
                if (_prior == null || _prior.Lines == null || _prior.Lines.Count == 0)
                    return false;

                // Find the first one
                _currentPrior = _prior.Lines.BinarySearch(_compareValues, this);
                if (_currentPrior < 0)
                    return false;

                for (; _currentPrior >= 0 && this.Compare(_compareValues, _prior.Lines[_currentPrior]) == 0; _currentPrior--)
                {
                }
                _currentPrior++;
            }
            else
            {
                _currentPrior++;
            }

            if (_currentPrior < 0)
                return false;

            for (; _currentPrior < _prior.Lines.Count && this.Compare(_compareValues, _prior.Lines[_currentPrior]) == 0; _currentPrior++)
            {
                _combinedValues = new HqlValues(_impactedFields, _prior.Lines[_currentPrior], _compareValues);

                if (_onJoin.EvaluateJoin(_combinedValues, false)) // && _where.EvaluateJoin(_combinedValues))
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _currentPrior = -1;
            _mode = HqlCompareMode.SEARCH_MODE;
        }

        public void SortPrior()
        {
            bool alreadySorted = true;
            
            // look to see if already sorted due to that being worst-case scenario!
            for (int i = 1; i < _prior.Lines.Count; ++i)
            {
                int ret = this.Compare(_prior.Lines[i - 1], _prior.Lines[i]);
                if (ret > 0)
                {
                    alreadySorted = false;
                    break;
                }
            }

            if (!alreadySorted)
                _prior.Lines.Sort(this);
        }

        public HqlFieldGroup CombinedFieldsImpacted
        {
            get { return _impactedFields; }
            set { _impactedFields = value;  }
        }
        
        HqlFieldGroup _impactedFields;
        HqlDataSource _prior;
        HqlWhere _onJoin;
        HqlWhere _where;
        //HqlWith _settings;
        ArrayList _orderOfSortFields;
        HqlCompareMode _mode;
        HqlJoinMethod _joinMethod;

        HqlValues _combinedValues;
        HqlValues _compareValues;
        int _currentPrior;
    }
}
