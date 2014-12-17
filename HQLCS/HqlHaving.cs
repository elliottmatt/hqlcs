using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlHaving : HqlWhere
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlHaving(HqlClause select)
            : base(NAME_HAVING)
        {
            _select = select;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public

        public new void Parse(HqlTokenProcessor processor)
        {
            Parse(processor, HqlKeyword.HAVING, HqlKeyword.HAVING, _select);
        }

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        ///////////////////////
        // Variables

        HqlClause _select;
    }
}
