hqlcs
=====

Flat-file (csv, etc) SQL-like processor in C#.
Written in spare time to replace a handful of single-purpose tools.

Is this production ready? No. But it was a fun exercise back in 2009.

Things done wrong:
- Don't use a lex/yacc parser
- Testing is horrific and was not built-in to the build (mix of batch files and comparisons)
- No interfaces or secondary projects for smaller code bases -- e.g. one codebase

Things done right:
- It was fun
- I understand and appreciate SQL a lot more
- Refactored a couple things to make the code more manageable (most things are HQLFields)
- I learned a ton

Syntax
=====
```code
CAPITAL LETTERS -> variable, must be evaluated to terminals
lowercase letters -> terminals
( ) -> explicitly required parens in result
<optional> -> optional value in the statement
[option1|option2] -> must choose exactly one option
\+ -> concatenate to one word
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
   FROM -> from ["filename"|stdin|(QUERY)|PASSED_OBJECT|dual] :: dual returns one row
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
    - "WITH OUTPUT=" has the variables evaluated when surrounded by braces, such as "out_{field1}.txt".
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
```
