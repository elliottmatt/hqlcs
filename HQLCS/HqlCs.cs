using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hql
{
    /*
     * Notes!
     * 
     * - WHEN reading the FROM, will read the first (N-1) FILES into memory. Therefore place them in order smallest to largest.
     * - What happens when we ask for a field not in a line? That line error out or the whole file error out?
     * 
     * 
     * 
     * Speed ideas
     * DONE - when count(*), don't save * in memory, just save String.Empty
     * 
     * 
     * 
     * 
        == begin email
        From: Elliott, Matt 
        Sent: Friday, January 30, 2009 12:05 AM
     
        - DONE nested queries - how would I do the from () part due to if I passed into another hqlcs then it wouldn't end in a new line. Therefore I need a way to tell it what I expect the end of query to be.
        - DONE I need scalars that do field manipulations such as trim() substr()
        - switch, case statements? Those are going to be a pain. I should implement oracle and sqlserver for experence.
        - need a true hqlcs-unit-test class that is going to run massive regression testing on it
        - need to be able to pass in a parameter, such as @source and pass in an open stream for reading in (very useful for testing!)
        - need to extrapolate a few interfaces for future expansion of the different object types. (Not high priority)
        - need to edit classes to have declarations in same order: static functions, contructors, overridden functions, public functions, private functions, array function, getter/setter, variable declaration
            = DONE: HqlCompareToken, HqlDataSource, HqlField,
            = DONE: HqlFieldGroup, HqlScalar, HqlFunction,
            = DONE: HqlFrom, HqlSelect, HqlQuery, HqlOrderBy, HqlKey,
            = DONE: HqlTable, HqlToken, HqlTokenProcessor, HqlWhere
        - what about changing so I could pass in an array of objects and I could write a sql statement with the city, state, etc being data members of the class? (would be slow using reflection. sort of a F# type behavior)
        - what about nonstandard options such as stop on 5 format-errors or stop on 5 rows?
        - What about select top 10?
        - order by is next followed by having followed by inner joins
        - any chance of coding unions? I use them once in a blue moon so not really that useful
        - need for decimal and ints a method for declaring how to print it out. Need the printoptions object.
        - what about date manipulation?
        - need a logging phrase such as "With logging [to "file.txt"]" to print out any error records
        - clean up exception handling.
        - going to say that it is not thread safe so don't try!!
        - whats memory usage on 100mb file group by?
        - DONE fix for count(*) since that is heavily used! if star is present, is it inside a count only? If so change to be if count('')
        - math on the fly would be a pain!!! Maybe talk to george abt library?
        - CHANGED ALGORITHM (2009/02/04) on hashtable I am doing my own crazy hashing thing. What are the chances I hash to same value? I should check to make sure that key is the same when I write it in. 
        - for jjcategorize, [output to file "*field2*.jjc.out"] where field2 changes. Or output to file "bob.txt" for a simple write out
        == end email
     
        - DONE IN () keyword
        - DONE IN (nexted) keyword
        - rownum < X
        - very complex testing between all types to make sure the WHERE statement can do them all.
            = types: string, int, float, field, scalar
            = operations: != = > >= < <= like !like
        - add ability to pass in ORDER BY 1,2,3 to sort by the SELECT fields of 1,2,3 w/o having to retype
        
        - Other stuff I'm missing: 2009/2/12
            = || field concatenation (select field1 || field2)
            = AS table referencing (select a.field1 from stdin as a)
            = AS field renaming (select field1 as btn from stdin)
     * 
     * 2009/03/18 - implemented having, but then noticed that...
     * - HAVING can do scalar functions in oracle, such as
     * select dispute_id, count(*) from pdt_dispute_detail where dispute_id = 1490 group by dispute_id having max(dispute_id) > 0
     * select dispute_id, count(*) from pdt_dispute_detail where dispute_id = 1490 group by dispute_id having max(dispute_id) > 1500

     * 
     * FUNCTIONS
        * AVG() - Returns the average value - DONE
        * COUNT() - Returns the number of rows - DONE
        * FIRST() - Returns the first value
        * LAST() - Returns the last value
        * MAX() - Returns the largest value - DONE
        * MIN() - Returns the smallest value - DONE
        * SUM() - Returns the sum - DONE (int)

     * SCALARS
        * UCASE() - Converts a field to upper case - DONE
        * LCASE() - Converts a field to lower case - DONE
        * MID() - Extract characters from a text field - DONE
        * LEN() - Returns the length of a text field - DONE
        * ROUND() - Rounds a numeric field to the number of decimals specified
        * NOW() - Returns the current system date and time + sysdate - DONE
        * FORMAT() - Formats how a field is to be displayed
        * trim() - DONE

        abs(numeric_expression)
        ascii(character_expression)
        avg([ All| Distinct] Expression)
        benchmark(count,expr)
        CASE value WHEN [compare-value] THEN result [WHEN [compare-value] THEN result ...] [ELSE result] END
        CASE WHEN [condition] THEN result [WHEN [condition] THEN result ...] [ELSE result] END
        ceiling(numeric_expression)
        char(integer_expression)
        charindex(expression1, expression2 [, start_location])
        concat(str1,str2,...)
        concat_ws(separator, str1, str2,...)
        count(*)
        count({[All | Distinct] expression]| *})
        database( ) - ME EDIT (filename())
        float(int)
        floor(numeric_expression)
        getdate( ) - coded as now() DONE
        greatest(expression [,...n])
        hex(N) 
        if(expr1,expr2,expr3)
        ifnull(expr1,expr2)
        initcap(string)
        insert(str,pos,len,newstr)
        integer(float)
        isdate(expression)
        isnull(check_expression, replacement_value)
        isnumeric(expression)
        least(expression [,...n])
        least(X,Y,...)
        left(character_expression, integer_expression)
        len(string_expression)
        locate(substr,str,pos)
        lower(character_expression)
        lpad(str,len,padstr)
        ltrim(character_expression)
        max([All | Distinct] expression)
        md5(string)
        min([All | Distinct] expression)
        mod(N,M)
        nullif(expr1,expr2)
        pi( ) - DONE
        pow(X,Y)
        repeat(str,count)
        replace(str, from_str,to_str)
        replace(string, search_string [,replacement_string])
        reverse(str)
        right(str,ten)
        round(X,D)
        rpad(str,len,padstr)
        rtrim(str)
        sign(number)
        sign(X) 
        sqrt(X) 
        std(expr) 
        stddev(expr)
        strcmp(expr1,expr2)
        text(char)
        trim([[BOTH | LEADING | TRAILING] [remstr] FROM] str)
        trunc (base [, number])
        ucase(str) 
        upper(str)
        version( )

	More examples of valid SQL:
	- sql server - select 'bob' +  'joe' + cast ( count(*) as varchar) from users	
     * 
     * 
     * * 
     * 
     * 
     * 
        WITH
            PFD -- done
            PRINT_FINAL_DELIMITER -- done
	        OD=\"\t\" -- done
	        OUT_DELIMITER=\"|\" -- done
	        D=\"|\" -- done
	        DELIMITER=\"|\" -- done
	        PRESERVE_QUOTES -- accepts but doesn't process
	        PQ -- accepts but doesn't process
            TEMPDIR="c:\temp\" -- accepts but doesn't process
            HEADER -- done
            OUTPUT="file.txt" -- normal output -- accepts -- done
            OUTPUT="{SELECT-VARIABLE}" -- jjcategorize functionality -- done
        
        SKIP 2 --how do I do this? it would have to be on the FROM piece...
        
        select TOP 10
     
        LOGGING TO "file.txt" -- debug output
     
        STOP ON 5 ERRORS
     * 
     * 
     */




    public class HqlCs 
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlCs()
        {
            Init();
        }

        public HqlCs(string sql)
        {
            Init();
            SetQuery(sql, null, String.Empty);
        }

        public HqlCs(string sql, object[] references)
        {
            Init();
            SetQuery(sql, references, String.Empty);
        }

        public HqlCs(string sql, string ExpectedEndOfQuery)
        {
            Init();
            SetQuery(sql, null, ExpectedEndOfQuery);
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public void SetQuery(string sql, object[] references, string ExpectedEndOfQuery)
        {
            _hql = new HqlQuery(sql, references, ExpectedEndOfQuery);
        }

        public void Compile()
        {
            CheckInit();
            _hql.Compile();
        }

        public string ReadLine()
        {
            CheckInit();
            return _hql.ReadLine();
        }

        public void Close()
        {
            CheckInit();
            _hql.Cleanup();
        }

        ///////////////////////
        // Private

        private void Init()
        {
            _hql = null;
        }

        private void CheckInit()
        {
            if (_hql == null)
                throw new InvalidOperationException("HqlCs is not initialized");
        }

        ///////////////////////
        // Fields

        public bool EndOfStream
        {
            get { CheckInit(); return _hql.EndOfStream; }
        }

        public string RemainingSql
        {
            get { CheckInit(); return _hql.RemainingSql; }
        }


        static public string HelpFile
        {
            get
            {
                #region HELP_PRINT_OUT
                const string help = @"
CAPITAL LETTERS -> variable, must be evaluated to terminals
lowercase letters -> terminals
( ) -> explicitly required parens in result
<optional> -> optional value in the statement
[option1|option2] -> must choose exactly one option
+ -> concatenate to one word
:: -> Everything after is a comment


QUERY -> SELECT FROM <WHERE> <GROUP> <ORDER> <HAVING> <WITH>
   SELECT -> select FIELDS
      FIELDS -> [*|EXPRESSION]<,FIELDS>
         EXPRESSION -> [SCALAR|FUNCTION|FIELD|EQUATION|VALUE]
            SCALAR -> APPENDIX_SCALAR
            FUNCTION -> [max|min|avg|count|sum](EXPRESSION) :: Restriction: Cannot logically nest functions although syntaxically it is permitted
            FIELD -> [field+ONENUMBER|FIELDNAME+FIELDDECL|FIELDNAME] :: Example: field1 field2 or city(10,2) city(10, 2) or city (if previously defined)
               ONENUMBER -> one_based_int :: Example: Must be >= 1 since 1-based field numbering
               FIELDNAME -> non_keyword_name 
               FIELDDECL -> (ONENUMBER , ONENUMBER)
           VALUE -> [LITERAL|INT|FLOAT|FIELD|EQUATION]
               LITERAL -> 'whatever_text_you_want'
               INT -> positve_or_negative_integer
               FLOAT -> positive_or_negative_integer
               EQUATION -> ^valid_c#_math^ :: Example: 2+2, 5*2, Math.Sin(Math.PI/2), (2+(5*1831)/4)
   FROM -> from [""filename""|stdin|(QUERY)|PASSED_OBJECT|dual] :: dual returns one row
      PASSED_OBJECT -> {ZERONUMBER} :: You can pass in a stream object in references[] and it will read it
      ZERONUMBER -> zero_based_int :: Example: Must be >= 0 since array is 0-based   
   WHERE -> where WHERECLAUSE
      WHERECLAUSE -> [CLAUSE|(WHERECLAUSE AND_OR WHERECLAUSE)]
         CLAUSE -> [VALUE LOGICAL_OPERATOR VALUE|VALUE INCLAUSE_OPERATOR (INCLAUSE)|VALUE INCLAUSE_OPERATOR (QUERY)]
            INCLAUSE -> [VALUE|,INCLAUSE]
            LOGICAL_OPERATOR -> [!=|=|<|>|<=|>=|like|not like]
            INCLAUSE_OPERATOR -> [in|in like|not in]
         AND_OR -> [and|or]
   GROUP -> group by GROUPFIELD
      GROUPFIELD -> [GROUPFIELD|EXPRESSION]
   ORDER -> order by ORDERFIELD
      ORDERFIELD -> [ORDERFIELD|EXPRESSION]
   HAVING -> having WHERECLAUSE
   WITH -> with WITHCLAUSE [WITHCLAUSE]
      WITHCLAUSE -> [SINGLE_WITH_CLAUSE|VALUE_WITH_CLAUSE]
         SINGLE_WITH_CLAUSE -> [pfd|print_final_delimiter|pq|preserve_quotes|header]
         VALUE_WITH_CLAUSE -> VALUE_WITH_CLAUSE_WORD = VALUE
            VALUE_WITH_CLAUSE_WORD = [output|d|delimiter|od|out_delimiter|tempdir]

APPENDIX_SCALAR:
    - len(VALUE)
    - substring(VALUE, NUMBER), substring(VALUE, NUMBER, NUMBER)
    - now()
    - upper(VALUE), ucase(VALUE)
    - lower(VALUE), lcase(VALUE)
    - trim(VALUE), trimleft(VALUE), trimright(VALUE)
    - replace(VALUE, SEARCH-VALUE, REPLACE-VALUE)
    - pi()
    - concat(VALUE, VALUE [, VALUE ...])
    - concat_ws(DELIMITER-VALUE, VALUE, VALUE [, VALUE ...]) :: places DELIMITER between all VALUEs
    - count_occur(VALUE, VALUE) :: returns number of times second VALUE occurs in first VALUE
    - getprintables(VALUE) :: returns printable characters using !Char.IsControl()
    - to_date(VALUE, DATEFORMAT [, OUTFORMAT]) :: returns a internal DateTime object for additional manipulation, '?' as format is auto-guessing.

NOTES:
    - ""WITH OUTPUT="" has the variables evaluated when surrounded by braces, such as ""out_{field1}.txt"".
      The full capabilities of SELECT-FIELDS may be used such as count(*), len(), sum(field1), etc
    - DATEFORMAT can be a string or another field. Format should be C# format, such a
      Day:     d = 1,2  dd = 01, 02 ddd = Mon, Tue dddd = Monday, Tuesday
      Month:   M = 1, 2 MM = 01, 02 MMM = Jan, Feb MMMM = January, February
      Year:    y = 99, 1, 2 yy = 99, 01, 02 yyyy = 1999, 2000, 2001
      Hour:    h = 1, 2, 12 hh = 01, 02, 12 H = 1, 2, 13, 23 HH = 01, 02, 13, 23
      Minute:  m = 1, 2, 59 mm = 01, 02, 59
      Second:  s = 1, 2, 59 ss = 01, 02, 59
      FracSec: f = 1, 2, 9  ff = 01, 17, 99 fff, ffff, ... fffffff = 1234567
      AM/PM:   t = A, P tt = AM, PM
";
                #endregion

                return help;
            }
        }


        ///////////////////////
        // Getters/Setters

        ///////////////////////
        // Variables

        private HqlQuery _hql;
    }

    internal class HqlStream : TextReader
    {
        ///////////////////////
        // Static Functions

        ///////////////////////
        // Constructors

        public HqlStream(String sql, String ExpectedEndOfQuery): base()
        {
            _hql = new HqlQuery(sql, ExpectedEndOfQuery);
            _hql.Compile();
        }

        ///////////////////////
        // Overridden functions

        public override string ReadLine()
        {
            return _hql.ReadLine();
        }
        public override int Peek()
        {
            if (_hql.EndOfStream)
                return -1;
            return 1;
        }
        public override void Close()
        {
            _hql.Cleanup();
        }

        ///////////////////////
        // Public 
        
        ///////////////////////
        // Private

        ///////////////////////
        // Getters/Setters

        public bool EndOfStream
        {
            get { return _hql.EndOfStream; }
        }
        
        public int CountReturnVisibleFields
        {
            get { return _hql.CountReturnVisibleFields; }
        }

        public string RemainingSql
        {
            get { return _hql.RemainingSql; }
        }

        ///////////////////////
        // Variables

        private HqlQuery _hql;
    }
}
