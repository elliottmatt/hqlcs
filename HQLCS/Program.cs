using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    // hierarchy of operations

    // SELECT
    // - ORDER BY
    // - - HAVING
    // - - - GROUP BY
    // - - - - WHERE
    // - - - - - FROM

    class Program
    {
        static int Main(string[] args)
        {
            HqlCs hql;
            int verbose =
#if DEBUG
            2;
#else
            0;
#endif
            //int linear = 0;

            try
            {
                //HqlKey key = new HqlKey(new object[] { "SUPER", "Feb-2009", "SUPER AWESOME RABBIT", "3056666666" });
                //int hash = key.GetHashCode();
                //hash = hash + 1 - 1;

                //return 0;
                int argc = 0;
                for (;argc < args.Length - 1; argc++)
                {
                    if (args[argc].StartsWith("-"))
                    {
                        if (args[argc].Equals("-v"))
                            verbose++;
                        else
                        {
                            Console.Error.WriteLine(String.Format("Unknown option {0}", args[argc]));
                            return 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (argc == args.Length - 1)
                {
                    hql = new HqlCs(args[argc]);
                    hql.Compile();
                    for (; ; )
                    {
                        string s = hql.ReadLine();
                        if (hql.EndOfStream)
                            break;
                        Console.WriteLine("{0}", s);
                    }
                    return 0;
                }
                else
                {
                    //HqlCsTester.PrintAllTestsToStdout();
#if !DEBUG 
                    Console.WriteLine(HqlCs.HelpFile);
                    return 1;
#endif
                }
#if DEBUG
                // TODO, more '' and scalar() compares
                if (false)
                {
                    if (!HqlCsTester.CheckLike()) { return 1; }
                    if (!HqlCsTester.CheckBoolean()) { return 1; }
                    if (!HqlCsTester.CheckToDate()) { return 1; }
                    //if (!HqlCsTester.CreateOutput()) { return 1; }
                    if (!HqlCsTester.CheckOutput(false)) { return 1; }                    
                    Console.WriteLine("Done checking prior output.");
                    //return 1;
                }
#endif
                //hql = new HqlCs("select * from stdin");
                hql = new HqlCs("select 'forgot to set a line to test' from dual");
                //hql = new HqlCs("select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\"");
                //hql = new HqlCs("select field10 , * from stdin group by field10 order by field10");
                //hql = new HqlCs("select field10 , count(*) from stdin group by field10 order by field10");
                //hql = new HqlCs("select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field2 = \"a\" and field2 >= \"a\"");
                //hql = new HqlCs("select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field3 like \"205%666\"");
                //hql = new HqlCs("select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field3 like \"2%%%%%%%%%%%%%%%%%%6\"");
                //hql = new HqlCs("select field1,field2, 'field10 + with spaces 1 1901230 053403542' ,* from \"..\\..\\..\\Testing\\DS\\rabbits*sm*txt\" where field4 like \"%record%\"");
                //hql = new HqlCs("select '(2+4)/3 + 10.0 / 3=', ^(2+4)/3 + 10.0 / 3^, * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\"");
                //hql = new HqlCs("select '(2+4)/3 + 10.0 / 3=', ^(2+4)/3 + 10.0 / 3^, * from stdin");
                //hql = new HqlCs("select bob(1, 2) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\"");
                //hql = new HqlCs("select max(field2) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1 order by field1 asc");
                //hql = new HqlCs("select sum(field2), count(field2), max(field2) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1");
                //hql = new HqlCs("select field1, field1, count(field2), sum(field2), field1, len(sum(field2)) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1");
                //hql = new HqlCs("select city(62, 29), state(92, 2) from \"D:\\data\\emfwdatagrp.txt\" where state = \"AL\"");
                //hql = new HqlCs("select state(92, 2), count(*) from \"D:\\data\\emfwdatagrp.txt\" where state like \"A%\" group by state");
                //hql = new HqlCs("select field1, max(field2), min(field2), count(field2), count(*), sum(field2) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1");
                //hql = new HqlCs("select field1, len(field1) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\"");
                //hql = new HqlCs("select len(field2), count(*) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\" group by len(field2)");
                //hql = new HqlCs("select 'nested', *, 'nested' from (select count(*) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\")");
                //hql = new HqlCs("select count(*) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\"");
                //hql = new HqlCs("select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field1 in ('A')");
                //hql = new HqlCs("select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field1 in ('A', field1, 12, 10.12, len(field1))");
                //hql = new HqlCs("select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field1 in (select field1 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\")");
                //hql = new HqlCs("select field1, avg(field2) from \"..\\..\\..\\Testing\\DS\\test_avg.txt\" group by field1");
                //hql = new HqlCs("select field1, avg(field2) from \"..\\..\\..\\Testing\\DS\\test_avg.txt\" group by field1");
                //hql = new HqlCs("select *, 'now', now(), 'processed', len(field1), len(field2), substring(field1, 3), substring(field1, 3, 3), substring(field1, 10, 20) from \"..\\..\\..\\Testing\\DS\\test_substring.txt\"");
                //hql = new HqlCs("select *, 'processed', len(field1), len(field2), 'double', len(len(field2)) from \"..\\..\\..\\Testing\\DS\\test_substring.txt\"");
                //hql = new HqlCs("select field1, upper(field1), lower(field1) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\"");
                //hql = new HqlCs("select '', field1, upper(field1), lower(field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\" group by '', field1, upper(field1), lower(field1)");
                //hql = new HqlCs("select '', field1, *, upper(field1), lower(field1) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\" where field2 > ^100+100^");
                //hql = new HqlCs("select * from {0}", new object[] { new StreamReader(@"..\\..\\..\\Testing\\DS\test_len_groupby.txt") });
                //hql = new HqlCs("select * from {0}", new object[] { sr });
                //hql = new HqlCs("select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" group by field1 order by field1 desc");
                //hql = new HqlCs("select field1, field2, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql_order.txt\" group by field1, field2 order by field1 desc, field2 desc");
                //hql = new HqlCs("select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql_order.txt\" order by field1 desc, field2 desc");
                //hql = new HqlCs("select *, 'start', field1, field2, 'both', TRIM(field1), 'left', trimleft(field1), 'right', trimright(field1), 'trimall', trim(*) from \"..\\..\\..\\Testing\\DS\\test_trim.txt\" order by field1 desc, field2 desc");
                //hql = new HqlCs("select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" group by field1 order by field1 desc");
                //hql = new HqlCs("select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1 order by field1");
                //hql = new HqlCs("select now(), upper(field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by now(), field1");
                //hql = new HqlCs("select concat('bob', 'joe') from dual");
                //hql = new HqlCs("select field1, trim(field1), concat(field1, field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by concat(field1, field1), field1");
                //hql = new HqlCs("select field1, field2, concat(field1, field2), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1, field2");
                //hql = new HqlCs("select concat_ws(concat('<', '-', '>') , field1, field2, field1), concat(field1, field2), field1, field2, count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1, field2 order by concat(field1, field2)");
                //hql = new HqlCs("select field1, bob(1,1), bob.field1 as \"My Field1\" from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" as bob where field1 > 1"); // compiles!
                //hql = new HqlCs("select field1 as \"New field\", bob(1,1), bob.field1 as \"My Field1\" from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" as bob where field1 > 1 with header od = \"$\" pfd");
                //hql = new HqlCs("select field1 as \"New field\", bob(1,1), bob.field1 as \"My Field1\" from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" as bob where field1 > 1 with header od = \"$\" pfd output='test.txt'");
                //hql = new HqlCs("select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\" with output='test_{0}_{1}_{0}.txt'");
                //hql = new HqlCs("select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1 order by field1 with output='test_{0}_{1}_{0}.txt'");
                //hql = new HqlCs("select *, test(1, 10) from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\" with output2='test_{field1}__{joe(1, 20)}_{test}_{length(field1)}_{length(length(field1))}.txt'");
                //hql = new HqlCs("select * from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\" with output='test_{field1}.txt'");
                //hql = new HqlCs("select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1 order by field1 with output='test_{field1}.txt'");
                //hql = new HqlCs("select upper(field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by upper(field1) order by upper(field1) with output='test_{count(upper(field1))}.txt'");
                //hql = new HqlCs("select upper(field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by upper(field1) order by upper(field1) with output='bob.txt'");
                //hql = new HqlCs("select pi(), upper(field1), * from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" order by field1 desc with output=\"bob_{concat(field1, 'hey there', length(field2))}.txt\"");
                //hql = new HqlCs("select *, '~', replace(*, '1', 'a'), '~', replace(field1, field1, field2) from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\"");

                //hql = new HqlCs("select * from \"d:\\TestRunParseSo\\a.a\" where len(field8) > 10");
                //hql = new HqlCs("select count_occur('abcdeabcde', '') from dual");

                //hql = new HqlCs("select field7, min(len(field9)), avg(len(field9)), max(len(field9)), stdev(len(field9)), count(*) from \"D:\\TestRunParseSo\\out_multilineusoc_RC40Q*.txt\" where field3 = \"FID\" and field2 not like \"CA%\" group by field7 order by field7");
                //hql = new HqlCs("select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_having.txt\" group by field1 having count(*) < 3");
                
                hql = new HqlCs("select a.field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\" group by a.field1");
                hql = new HqlCs("select a.field1, a.field2, b.field2, 'cfield2', c.field2, 'a-all', a.* from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\"");

                hql = new HqlCs("select field1, trim(field1), concat(field1, field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by concat(field1, field1), field1");

                hql = new HqlCs("select * from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\"");

                hql = new HqlCs("select a.field1, a.field2, b.field1, field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1");


                hql = new HqlCs("select 'empty', sum(field1), '10', sum(field1, 10), 'C', sum(field1, 'C'), 'D', sum(field1, 'D'), 'P', sum(field1, 'P') from (select '12345678.12876' from dual)");

                hql = new HqlCs("select count(*), sum(123.00000) from \"..\\..\\..\\Testing\\DS\\test_join1.txt\"");

                hql = new HqlCs("select decimal(count(*), 4), decimal(sum(123.00100), 6) from \"..\\..\\..\\Testing\\DS\\test_join1.txt\"");

                hql = new HqlCs("select decimal(field2, 0), decimal(field2, 3), decimal(field2, 7), decimal(field3, 3), decimal(field3, 6), a.field1, a.field2, a.field3 from \"..\\..\\..\\Testing\\DS\\test_int_decimal.txt\"");
                hql = new HqlCs("select field1 from (select 'bob    joe susan' from dual) with delimiter='\t'");
                //hql = new HqlCs("select field1 from (select 'bob    joe susan' from dual) with delimiter = \"\t\"");
                hql = new HqlCs("select * from (select '1|2' from dual) where field1 not in (1, 2)");


                hql = new HqlCs("select * from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\" with output='test_{field1}.txt'");

                hql = new HqlCs("select replace('b''ham', '''', '''''') from dual");

                hql = new HqlCs("select field1 from 'c:\\temp\\a.a.sample' group by field1");

                hql = new HqlCs("select a.*, b.* from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 where a.field1 = \"A\" or b.field1 = \"B\"");

                //hql = new HqlCs("select a.*, b.* from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 where a.field1 = \"A\" and a.field1 = \"B\" and (a.field1 = \"C\" and a.field1 = \"D\") and (a.field1 = \"E\" or a.field1 = \"F\") and a.field1 = \"G\" and (a.field1 = \"H\" and a.field1 = \"I\")");
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a left outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1");
                //hql = new HqlCs("select rownum, * from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" order by rownum desc");

                //hql = new HqlCs("select field1 from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1 having count(*) in (3, 4)");

                //hql = new HqlCs("select decimal(field2, 0), decimal(field2, 3), decimal(field2, 7), decimal(field3, 3), decimal(field3, 6), a.field1, a.field2, a.field3 from \"..\\..\\..\\Testing\\DS\\test_int_decimal.txt\"");

                //hql = new HqlCs("select field1, decode(field1, 'A', 11, 'B', 22, 'C', 10), sum(decode(field1, 'A', 1, 0)) from '..\\..\\..\\Testing\\DS\\test_join1.txt' group by field1");

#if false
                //hql = new HqlCs("select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 < 2147483647 or field1 > 21474836470");
                //hql = new HqlCs("select decimal(sum(field1), 0), decimal(sum(field1), 1), decimal(sum(field1), 7), sum(field1) from (select '$1,234,112.52', rownum from dual)");
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from '..\\..\\..\\Testing\\DS\\test_skip1.txt' as a skip 1 full outer join '..\\..\\..\\Testing\\DS\\test_skip1.txt' as b skip = 2 on a.field1 = b.field1 order by a.field1");

                //hql = new HqlCs("select field1, field2, replace(field2, ' 12:00:00 AM', ''), field2 from '..\\..\\..\\Testing\\DS\\test_substring_compare.txt' where field1 = replace(field2, ' 12:00:00 AM', '')");
                
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a full outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1 where a.field1 IS NOT NULL");
                
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 = b.field1"); // good2 matches oracle 
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 = \"\""); // good2 BECAUSE \"\" IS NULL IN THIS, doesn't match oracle because '' != NULL select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 = b.field2 where a.field1 = ''
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 is null"); // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 = b.field2 where a.field1 is null
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 = b.field1");  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 = b.field1
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 != b.field1 order by b.field1");  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 != b.field1
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 != b.field1 or a.field1 = b.field1");  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 != b.field1 or a.field1 = b.field1
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 != b.field1 and a.field1 = b.field1");  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 != b.field1 and a.field1 = b.field1
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2");  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field2 = b.field2
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 != b.field2");  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field2 != b.field2
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 is null"); // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 is null
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 is not null"); // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field2 = b.field2 where a.field1 is not null
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on 'A' = 'A'"); // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on 'A' = 'A'
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on 'A' = 'B'"); // matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on 'A' = 'B'

                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum = 1"); //good2 1 row select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum = 1 // 1 joined
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum > 1"); // good1 2 rows //select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum > 1 // 2 records
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum = 0"); // bad2 - oracle treats rownum = 0 funny!! select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum = 0 // 1 only A
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum = 100"); //good2 2 rows select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum = 100 // 2 records
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum < 100"); //good2 1 row select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum < 100 // 1 joined
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum < 1"); // bad2 - oracle treats rownum = 0 funny!! select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum < 1 // 1 joined
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum > 0"); //good2 select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum > 0 // 2 records
                //hql = new HqlCs("select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.rownum != 1"); //bad2 - oracle treats rownum = 0 funny!! select rownum, a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on rownum != 1 // 1 only A

                //hql = new HqlCs("select field1 from '..\\..\\..\\Testing\\DS\\Qacct_*.txt' group by field1 order by field1 with delimiter = ','");

                //hql = new HqlCs("select z.rownum, z.field1, a.rownum, b.rownum, c.rownum, d.rownum, e.rownum, f.rownum, g.rownum, h.rownum, a.field1, a.field2, b.field1, b.field2, c.field1, c.field2, d.field1, d.field2, e.field1, e.field2, f.field1, f.field2, g.field1, g.field2, h.field1, h.field2 from 'c:\\Qacct_list.txt' as z full outer join 'c:\\Qacct_AmtDisputed_2008-11-06.txt' as a on a.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2008-12-09.txt' as b on b.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2009-01-09.txt' as c on c.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2009-02-09.txt' as d on d.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2009-03-09.txt' as e on e.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2009-04-08.txt' as f on f.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2009-05-08.txt' as g on g.field1 = z.field1 full outer join 'c:\\Qacct_AmtDisputed_2009-06-08.txt' as h on h.field1 = z.field1 with delimiter=','");
                //hql = new HqlCs("select z.rownum, z.field1, a.rownum, b.rownum, c.rownum, d.rownum, e.rownum, f.rownum, g.rownum, h.rownum, a.field1, a.field2, b.field1, b.field2, c.field1, c.field2, d.field1, d.field2, e.field1, e.field2, f.field1, f.field2, g.field1, g.field2, h.field1, h.field2 from (select field1 from '..\\..\\..\\Testing\\DS\\Qacct_*.txt' group by field1 order by field1 with delimiter=',') as z full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2008-11-06.txt' as a on a.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2008-12-09.txt' as b on b.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-01-09.txt' as c on c.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-02-09.txt' as d on d.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-03-09.txt' as e on e.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-04-08.txt' as f on f.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-05-08.txt' as g on g.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-06-08.txt' as h on h.field1 = z.field1 order by z.rownum with delimiter=',' ");

                //hql = new HqlCs("select field1,field1 from (select field1 from (select 'A+B' from dual) delim='+' with d='#' od='#')");

                //hql = new HqlCs("select field1 from '..\\..\\..\\Testing\\DS\\test_xls.txt' group by field1 having count(*) > 1 order by count(*) desc");
#endif

                // TODO
                //hql = new HqlCs("select * from '..\\..\\..\\Testing\\DS\\test_like_notlike.txt' where field4 not inlike ('205%', '334%')");



                hql = new HqlCs("select field1 from '..\\..\\..\\Testing\\DS\\test_xls.txt' group by field1 having sum(field1) > 0 order by sum(field1)");


                hql = new HqlCs("select a.* from '..\\..\\..\\Testing\\DS\\test_groupby.txt' as a skip 1");
                hql = new HqlCs("select a.* from '..\\..\\..\\Testing\\DS\\test_groupby.txt' skip 1 as a");


                hql.Compile();
                for (; ; ) 
                {
                    string s = hql.ReadLine();
                    if (hql.EndOfStream)
                        break;
                    Console.WriteLine("{0}", s);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                for (Exception ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Console.Error.WriteLine("Inner Exception: " + ie.Message);
                if (verbose >= 2)
                    Console.Error.WriteLine("Full Error: " + ex.ToString());
                return 1;
            }

            return 0;
        }
    }
}
