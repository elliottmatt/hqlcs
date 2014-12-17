using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlKeywords
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlKeywords(HqlKeyword keyword, string[] words)
        {
            _keyword = keyword;
            _words = words;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        public HqlKeyword Keyword
        {
            get { return _keyword; }
        }

        public string[] Words
        {
            get { return _words; }
        }
        
        ///////////////////////
        // Variables

        HqlKeyword _keyword;
        string[] _words;
    }

    class HqlSplitwords
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlSplitwords(string firstpart, string wholephrase)
        {
            _firstpart = firstpart;
            _wholephrase = wholephrase;
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        ///////////////////////
        // Private

        ///////////////////////
        // Fields

        ///////////////////////
        // Getters/Setters

        public string FirstPart
        {
            get { return _firstpart; }
        }

        public string WholePhrase
        {
            get { return _wholephrase; }
        }

        ///////////////////////
        // Variables

        protected string _firstpart;
        
        protected string _wholephrase;
    }
}
