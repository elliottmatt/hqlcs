using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlHavingRow : HqlRecord
    {
        public HqlHavingRow(HqlFieldGroup orderby, HqlFieldGroup groupby, HqlFieldGroup select)
        {
            _orderby = orderby;
            _groupby = groupby;
            _select = select;
        }

        public HqlKey Key { set { _key = value; } }

        public HqlResultRow Row { set { _row = value; } }

        public override object GetValue(HqlField field)
        {
            // layout is 
            // key = [order, group]
            // row = [order, select, with]

            for (int i = 0; i < _select.Count; ++i)
            {
                if (_select[i].Equals(field))
                    return _row.GetValue(_orderby.Count + i);
            }
            for (int i = 0; i < _groupby.Count; ++i)
            {
                if (_groupby[i].Equals(field))
                    return _key.GetValue(_orderby.Count + i);
            }

            //for (int i = 0; i < _orderby.Count; ++i)
            //{
            //    if (_orderby[i].Equals(field))
            //        return _key.GetValue(_orderby.Count + i);
            //}

            throw new Exception("Unknown field in Having");
        }

        private HqlResultRow _row;
        private HqlKey _key;

        private HqlFieldGroup _select;
        private HqlFieldGroup _groupby;
        private HqlFieldGroup _orderby;
    }
}
