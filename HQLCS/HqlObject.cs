using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    enum HqlObjectType
    {
        OBJECT,
        STRING,
        INT32,
        INT64,
        DECIMAL,
        DATETIME,
    };

    abstract class HqlObject
    {

    }

    class HqlDumbObject : HqlObject
    {
        public HqlDumbObject(object o)
            : base()
        {
            _o = o;
        }

        public override string ToString()
        {
            return _o.ToString();
        }

        public object Value { get { return _o; } set { _o = value; } }

        object _o;
    }
}
