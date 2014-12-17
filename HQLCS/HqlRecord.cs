using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    abstract class HqlRecord
    {
        public virtual object GetValue(HqlToken t)
        {
            if (t.NeedsEvaluation)
                return GetValue(t.Field);
            else
                return t.EvaluatedData;
        }

        abstract public object GetValue(HqlField field);
    }
}
