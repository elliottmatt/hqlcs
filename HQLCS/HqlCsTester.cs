using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlTestInfo
    {
        public HqlTestInfo(int num, string dir, string query)
        {
            Query = query;
            ResultDirectory = dir;
            if (ResultDirectory.Length > 3)
            {
                ResultDirectory += (dir.EndsWith("\\") ? "" : "\\");
            }
            
            ResultNumber = num.ToString();
        }
        public string ResultDirectory;
        public string ResultNumber;
        public string Query;

        public string GetResultDirectory()
        {
            return ResultDirectory;
        }

        public string GetNewResultDirectory()
        {
            return ResultDirectory.Substring(0, ResultDirectory.Length - 1) + "_old_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "\\";
        }

        public string GetResultFilename()
        {
            return ResultDirectory + "result_" + ResultNumber;
        }
        //public string GetResultFilename(string dir)
        //{
        //    return dir + (dir.EndsWith("\\")?"":"\\") + "result_" + ResultNumber;
        //}
    }

    class HqlCsTester
    {
        private HqlCsTester() { }

        static string ResultDirectory = "..\\..\\..\\Testing\\Results\\";
        #region successful_tests
        static HqlTestInfo[] tests = new HqlTestInfo[] {
            new HqlTestInfo(0, ResultDirectory, "select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\""),
            new HqlTestInfo(1, ResultDirectory, "select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field2 = \"a\" and field2 >= \"a\""),
            new HqlTestInfo(35, ResultDirectory, "select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field2 = \"A\" and field2 >= \"A\""),
            new HqlTestInfo(2, ResultDirectory, "select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field3 like \"205%666\""),
            new HqlTestInfo(3, ResultDirectory, "select * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field3 like \"2%%%%%%%%%%%%%%%%%%6\""),
            new HqlTestInfo(4, ResultDirectory, "select field1,field2, 'field10 + with spaces 1 1901230 053403542' ,* from \"..\\..\\..\\Testing\\DS\\rabbits*sm*txt\" where field4 like \"%record%\""),
            new HqlTestInfo(36, ResultDirectory, "select field1,field2, 'field10 + with spaces 1 1901230 053403542' ,* from \"..\\..\\..\\Testing\\DS\\rabbits*sm*txt\" where field4 like \"%RECORD%\""),
            new HqlTestInfo(5, ResultDirectory, "select '(2+4)/3 + 10.0 / 3=', ^(2+4)/3 + 10.0 / 3^, * from \"..\\..\\..\\Testing\\DS\\test_hql.txt\""),
            new HqlTestInfo(6, ResultDirectory, "select '(2+4)/3 + 10.0 / 3=', ^(2+4)/3 + 10.0 / 3^, * from dual"),
            new HqlTestInfo(7, ResultDirectory, "select bob(1, 2) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\""),
            new HqlTestInfo(8, ResultDirectory, "select max(field2) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1"),
            new HqlTestInfo(9, ResultDirectory, "select sum(field2), count(field2), max(field2) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1"),
            new HqlTestInfo(10, ResultDirectory, "select field1, field1, count(field2), sum(field2), field1, len(sum(field2)) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1"),
            //new HqlTestInfo(11, ResultDirectory, "select city(62, 29), state(92, 2) from \"D:\\data\\emfwdatagrp.txt\" where city = \"AL\""),
            new HqlTestInfo(12, ResultDirectory, "select field1, max(field2), min(field2), count(field2), count(*), sum(field2) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\" group by field1"),
            //new HqlTestInfo(13, ResultDirectory, "select state(92, 2), count(*) from \"D:\\data\\emfwdatagrp.txt\" where state like \"A%\" group by state"),
            new HqlTestInfo(14, ResultDirectory, "select len(field2), count(*) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\" group by len(field2)"),
            new HqlTestInfo(15, ResultDirectory, "select field1, substr(field1, 1, 3) from \"..\\..\\..\\Testing\\DS\\test_substring.txt\""),
            new HqlTestInfo(16, ResultDirectory, "select field1, len(substr(field1, 1, 3)) from \"..\\..\\..\\Testing\\DS\\test_substring.txt\""),
            new HqlTestInfo(17, ResultDirectory, "select len('bob') from dual"),
            new HqlTestInfo(18, ResultDirectory, "select 'nested', *, 'nested' from (select count(*) from \"..\\..\\..\\Testing\\DS\\test_groupby.txt\")"),
            new HqlTestInfo(19, ResultDirectory, "select field1, avg(field2) from \"..\\..\\..\\Testing\\DS\\test_avg.txt\" group by field1"),
            new HqlTestInfo(20, ResultDirectory, "select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field1 in ('A')"),
            new HqlTestInfo(21, ResultDirectory, "select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field1 in ('A', field1, 12, 10.12, len(field1))"),
            new HqlTestInfo(22, ResultDirectory, "select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" where field1 in (select field1 from \"..\\..\\..\\Testing\\DS\\test_hql.txt\")"),
            new HqlTestInfo(23, ResultDirectory, "select field1, avg(field2) from \"..\\..\\..\\Testing\\DS\\test_avg.txt\" group by field1"),
            new HqlTestInfo(24, ResultDirectory, "select field1, *, upper(field1), lower(field1) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\" where field2 > ^100+100^"),
            new HqlTestInfo(25, ResultDirectory, "select *, 'processed', len(field1), len(field2), 'double', len(len(field2)) from \"..\\..\\..\\Testing\\DS\\test_substring.txt\""),
            new HqlTestInfo(26, ResultDirectory, "select field1, upper(field1), lower(field1) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\""),
            new HqlTestInfo(27, ResultDirectory, "select '', field1, upper(field1), lower(field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_len_groupby.txt\" group by '', field1, upper(field1), lower(field1)"),
            new HqlTestInfo(28, ResultDirectory, "select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" group by field1 order by field1 desc"),
            new HqlTestInfo(30, ResultDirectory, "select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" group by field1 order by field1 desc, field1 asc, field1 desc"),
            new HqlTestInfo(31, ResultDirectory, "select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" group by field1 order by field1 desc, field1 asc"),
            new HqlTestInfo(32, ResultDirectory, "select field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql.txt\" group by field1 order by field1 desc"),
            new HqlTestInfo(33, ResultDirectory, "select field1, field2, count(*) from \"..\\..\\..\\Testing\\DS\\test_hql_order.txt\" group by field1, field2 order by field1 desc, field2 desc"),
            new HqlTestInfo(34, ResultDirectory, "select field1, field2 from \"..\\..\\..\\Testing\\DS\\test_hql_order.txt\" order by field1 desc, field2 desc"),        
            new HqlTestInfo(1000, ResultDirectory, "SELECT FIELD1, FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\""),
            new HqlTestInfo(1001, ResultDirectory, "SELECT * FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD2 = \"a\" AND FIELD2 >= \"a\""),
            new HqlTestInfo(1035, ResultDirectory, "SELECT * FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD2 = \"A\" AND FIELD2 >= \"A\""),
            new HqlTestInfo(1002, ResultDirectory, "SELECT * FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD3 LIKE \"205%666\""),
            new HqlTestInfo(1003, ResultDirectory, "SELECT * FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD3 LIKE \"2%%%%%%%%%%%%%%%%%%6\""),
            new HqlTestInfo(1004, ResultDirectory, "SELECT FIELD1,FIELD2, 'field10 + with spaces 1 1901230 053403542' ,* FROM \"..\\..\\..\\TESTING\\DS\\RABBITS*SM*TXT\" WHERE FIELD4 LIKE \"%record%\""),
            new HqlTestInfo(1036, ResultDirectory, "SELECT FIELD1,FIELD2, 'field10 + with spaces 1 1901230 053403542' ,* FROM \"..\\..\\..\\TESTING\\DS\\RABBITS*SM*TXT\" WHERE FIELD4 LIKE \"%RECORD%\""),
            new HqlTestInfo(1005, ResultDirectory, "SELECT '(2+4)/3 + 10.0 / 3=', ^(2+4)/3 + 10.0 / 3^, * FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\""),
            new HqlTestInfo(1006, ResultDirectory, "SELECT '(2+4)/3 + 10.0 / 3=', ^(2+4)/3 + 10.0 / 3^, * FROM DUAL"),
            new HqlTestInfo(1007, ResultDirectory, "SELECT BOB(1, 2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\""),
            new HqlTestInfo(1008, ResultDirectory, "SELECT MAX(FIELD2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_GROUPBY.TXT\" GROUP BY FIELD1"),
            new HqlTestInfo(1009, ResultDirectory, "SELECT SUM(FIELD2), COUNT(FIELD2), MAX(FIELD2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_GROUPBY.TXT\" GROUP BY FIELD1"),
            new HqlTestInfo(1010, ResultDirectory, "SELECT FIELD1, FIELD1, COUNT(FIELD2), SUM(FIELD2), FIELD1, LEN(SUM(FIELD2)) FROM \"..\\..\\..\\TESTING\\DS\\TEST_GROUPBY.TXT\" GROUP BY FIELD1"),
            //new HqlTestInfo(1011, ResultDirectory, "SELECT CITY(62, 29), STATE(92, 2) FROM \"D:\\DATA\\EMFWDATAGRP.TXT\" WHERE CITY = \"AL\""),
            new HqlTestInfo(1012, ResultDirectory, "SELECT FIELD1, MAX(FIELD2), MIN(FIELD2), COUNT(FIELD2), COUNT(*), SUM(FIELD2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_GROUPBY.TXT\" GROUP BY FIELD1"),
            //new HqlTestInfo(1013, ResultDirectory, "SELECT STATE(92, 2), COUNT(*) FROM \"D:\\DATA\\EMFWDATAGRP.TXT\" WHERE STATE LIKE \"A%\" GROUP BY STATE"),
            new HqlTestInfo(1014, ResultDirectory, "SELECT LEN(FIELD2), COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEN_GROUPBY.TXT\" GROUP BY LEN(FIELD2)"),
            new HqlTestInfo(1015, ResultDirectory, "SELECT FIELD1, SUBSTR(FIELD1, 1, 3) FROM \"..\\..\\..\\TESTING\\DS\\TEST_SUBSTRING.TXT\""),
            new HqlTestInfo(1016, ResultDirectory, "SELECT FIELD1, LEN(SUBSTR(FIELD1, 1, 3)) FROM \"..\\..\\..\\TESTING\\DS\\TEST_SUBSTRING.TXT\""),
            new HqlTestInfo(1017, ResultDirectory, "SELECT LEN('bob') FROM DUAL"),
            new HqlTestInfo(1018, ResultDirectory, "SELECT 'nested', *, 'nested' FROM (SELECT COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_GROUPBY.TXT\")"),
            new HqlTestInfo(1019, ResultDirectory, "SELECT FIELD1, AVG(FIELD2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_AVG.TXT\" GROUP BY FIELD1"),
            new HqlTestInfo(1020, ResultDirectory, "SELECT FIELD1, FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD1 IN ('a')"),
            new HqlTestInfo(1021, ResultDirectory, "SELECT FIELD1, FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD1 IN ('a', FIELD1, 12, 10.12, LEN(FIELD1))"),
            new HqlTestInfo(1022, ResultDirectory, "SELECT FIELD1, FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" WHERE FIELD1 IN (SELECT FIELD1 FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\")"),
            new HqlTestInfo(1023, ResultDirectory, "SELECT FIELD1, AVG(FIELD2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_AVG.TXT\" GROUP BY FIELD1"),
            new HqlTestInfo(1024, ResultDirectory, "SELECT FIELD1, *, UPPER(FIELD1), LOWER(FIELD1) FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEN_GROUPBY.TXT\" WHERE FIELD2 > ^100+100^"),
            new HqlTestInfo(1025, ResultDirectory, "SELECT *, 'processed', LEN(FIELD1), LEN(FIELD2), 'double', LEN(LEN(FIELD2)) FROM \"..\\..\\..\\TESTING\\DS\\TEST_SUBSTRING.TXT\""),
            new HqlTestInfo(1026, ResultDirectory, "SELECT FIELD1, UPPER(FIELD1), LOWER(FIELD1) FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEN_GROUPBY.TXT\""),
            new HqlTestInfo(1027, ResultDirectory, "SELECT '', FIELD1, UPPER(FIELD1), LOWER(FIELD1), COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEN_GROUPBY.TXT\" GROUP BY '', FIELD1, UPPER(FIELD1), LOWER(FIELD1)"),
            new HqlTestInfo(1028, ResultDirectory, "SELECT FIELD1, COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" GROUP BY FIELD1 ORDER BY FIELD1 DESC"),
            new HqlTestInfo(1030, ResultDirectory, "SELECT FIELD1, COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" GROUP BY FIELD1 ORDER BY FIELD1 DESC, FIELD1 ASC, FIELD1 DESC"),
            new HqlTestInfo(1031, ResultDirectory, "SELECT FIELD1, COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" GROUP BY FIELD1 ORDER BY FIELD1 DESC, FIELD1 ASC"),
            new HqlTestInfo(1032, ResultDirectory, "SELECT FIELD1, COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql.TXT\" GROUP BY FIELD1 ORDER BY FIELD1 DESC"),
            new HqlTestInfo(1033, ResultDirectory, "SELECT FIELD1, FIELD2, COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql_ORDER.TXT\" GROUP BY FIELD1, FIELD2 ORDER BY FIELD1 DESC, FIELD2 DESC"),
            new HqlTestInfo(1034, ResultDirectory, "SELECT FIELD1, FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_Hql_ORDER.TXT\" ORDER BY FIELD1 DESC, FIELD2 DESC"),        

            new HqlTestInfo(37, ResultDirectory, "select field1, trim(field1), concat(field1, field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by concat(field1, field1), field1"),
            new HqlTestInfo(38, ResultDirectory, "select field1, field2, concat(field1, field2), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1, field2 order by concat(field1, field2)"),
            new HqlTestInfo(39, ResultDirectory, "select field1, field2, concat(field1, field2), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1, field2"),
            new HqlTestInfo(40, ResultDirectory, "select field1, trim(field1), concat(field1, field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by concat(field1, field1), field1"),

            new HqlTestInfo(41, ResultDirectory, "select concat_ws(concat('<', '-', '>') , field1, field2, field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by field1, field2 order by concat(field1, field2)"),
            new HqlTestInfo(42, ResultDirectory, "SELECT CONCAT_WS(CONCAT('<', '-', '>') , FIELD1, FIELD2, FIELD1), COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_CASESENSITIVE.TXT\" GROUP BY FIELD1, FIELD2 ORDER BY CONCAT(FIELD1, FIELD2)"),

            new HqlTestInfo(43, ResultDirectory, "select upper(field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by upper(field1) order by upper(field1)"),
            new HqlTestInfo(44, ResultDirectory, "SELECT UPPER(FIELD1), COUNT(*) FROM \"..\\..\\..\\TESTING\\DS\\TEST_CASESENSITIVE.TXT\" GROUP BY UPPER(FIELD1) ORDER BY UPPER(FIELD1)"),

            new HqlTestInfo(45, ResultDirectory, "select *, '~', replace(*, '1', 'a'), '~', replace(field1, field1, field2) from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\""),
            new HqlTestInfo(46, ResultDirectory, "SELECT *, '~', REPLACE(*, '1', 'A'), '~', REPLACE(FIELD1, FIELD1, FIELD2) FROM \"..\\..\\..\\TESTING\\DS\\TEST_JJCAT.TXT\""),

            new HqlTestInfo(47, ResultDirectory, "select count(*) from \"..\\..\\..\\Testing\\DS\\test_jjcat.txt\""),

            new HqlTestInfo(48, ResultDirectory, "select count(*) from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\""),

            new HqlTestInfo(49, ResultDirectory, "select a.field1, count(*) from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\" group by a.field1"),
            new HqlTestInfo(50, ResultDirectory, "select a.field1, a.field2, b.field2, 'cfield2', c.field2, 'a-all', a.* from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\""),

            new HqlTestInfo(51, ResultDirectory, "select field1, trim(field1), concat(field1, field1), count(*) from \"..\\..\\..\\Testing\\DS\\test_casesensitive.txt\" group by concat(field1, field1), field1"),

            new HqlTestInfo(52, ResultDirectory, "select a.field1, a.field2, b.field2, 'cfield2', c.field2, 'a-all', a.* from \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_join2.txt\" as b on a.field1 = b.field1 inner join \"..\\..\\..\\Testing\\DS\\test_join1.txt\" as c on a.field1 = c.field1 where a.field1 = \"A\" or b.field1 = \"B\" or c.field1 = \"C\""),
            new HqlTestInfo(53, ResultDirectory, "SELECT A.FIELD1, A.FIELD2, B.FIELD2, 'CFIELD2', C.FIELD2, 'A-ALL', A.* FROM \"..\\..\\..\\TESTING\\DS\\TEST_JOIN1.TXT\" AS A INNER JOIN \"..\\..\\..\\TESTING\\DS\\TEST_JOIN2.TXT\" AS B ON A.FIELD1 = B.FIELD1 INNER JOIN \"..\\..\\..\\TESTING\\DS\\TEST_JOIN1.TXT\" AS C ON A.FIELD1 = C.FIELD1 WHERE A.FIELD1 = \"A\" OR B.FIELD1 = \"B\" OR C.FIELD1 = \"C\""),

            new HqlTestInfo(54, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a inner join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1"),
            new HqlTestInfo(55, ResultDirectory, "SELECT A.FIELD1, A.FIELD2, B.FIELD1, B.FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN1.TXT\" AS A INNER JOIN \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN2.TXT\" AS B ON A.FIELD1 = B.FIELD1"),

            new HqlTestInfo(56, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a left outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1"),
            new HqlTestInfo(57, ResultDirectory, "SELECT A.FIELD1, A.FIELD2, B.FIELD1, B.FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN1.TXT\" AS A LEFT OUTER JOIN \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN2.TXT\" AS B ON A.FIELD1 = B.FIELD1"),

            new HqlTestInfo(58, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a right outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1"),
            new HqlTestInfo(59, ResultDirectory, "SELECT A.FIELD1, A.FIELD2, B.FIELD1, B.FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN1.TXT\" AS A RIGHT OUTER JOIN \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN2.TXT\" AS B ON A.FIELD1 = B.FIELD1"),

            new HqlTestInfo(60, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a full outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1"),
            new HqlTestInfo(61, ResultDirectory, "SELECT A.FIELD1, A.FIELD2, B.FIELD1, B.FIELD2 FROM \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN1.TXT\" AS A FULL OUTER JOIN \"..\\..\\..\\TESTING\\DS\\TEST_LEFTRIGHTJOIN2.TXT\" AS B ON A.FIELD1 = B.FIELD1"),

            new HqlTestInfo(62, ResultDirectory, "select decimal(a.field2, 0), decimal(a.field2, 3), decimal(a.field2, 7), decimal(a.field3, 3), decimal(a.field3, 6), a.field1, a.field2, a.field3 from \"..\\..\\..\\Testing\\DS\\test_int_decimal.txt\" as a"),
            new HqlTestInfo(63, ResultDirectory, String.Format("select field1 from (select 'bob{0}joe{0}susan' from dual) with delimiter='\\t'", "\t")),

            new HqlTestInfo(65, ResultDirectory, "select field1, decode(field1, 'A', 11, 'B', 22, 'C', 10), sum(decode(field1, 'A', 1, 0)) from '..\\..\\..\\Testing\\DS\\test_join1.txt' group by field1"),

            // compare number tests
            new HqlTestInfo(600, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 < 0"),
            new HqlTestInfo(601, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 = 0"),
            new HqlTestInfo(602, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 != 0"),
            new HqlTestInfo(603, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 > 0"),
            new HqlTestInfo(604, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 <> 0"),
            new HqlTestInfo(605, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like 1"),
            new HqlTestInfo(606, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '1'"),
            new HqlTestInfo(607, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '1%'"),

            new HqlTestInfo(620, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 < 30"),
            new HqlTestInfo(621, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 = 30"),
            new HqlTestInfo(622, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 != 30"),
            new HqlTestInfo(623, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 > 30"),
            new HqlTestInfo(624, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 <> 30"),
            new HqlTestInfo(625, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like 30"),
            new HqlTestInfo(626, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '30'"),
            new HqlTestInfo(627, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '30%'"),

            new HqlTestInfo(640, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 < 30.123"),
            new HqlTestInfo(641, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 = 30.123"),
            new HqlTestInfo(642, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 != 30.123"),
            new HqlTestInfo(643, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 > 30.123"),
            new HqlTestInfo(644, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 <> 30.123"),
            new HqlTestInfo(645, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like 30.123"),
            new HqlTestInfo(646, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '30.123'"),
            new HqlTestInfo(647, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '30.123%'"),

            new HqlTestInfo(660, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 < -30"),
            new HqlTestInfo(661, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 = -30"),
            new HqlTestInfo(662, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 != -30"),
            new HqlTestInfo(663, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 > -30"),
            new HqlTestInfo(664, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 <> -30"),
            new HqlTestInfo(665, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like -30"),
            new HqlTestInfo(666, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '-30'"),
            new HqlTestInfo(667, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '-30%'"),

            new HqlTestInfo(680, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 < -30.123"),
            new HqlTestInfo(681, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 = -30.123"),
            new HqlTestInfo(682, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 != -30.123"),
            new HqlTestInfo(683, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 > -30.123"),
            new HqlTestInfo(684, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 <> -30.123"),
            new HqlTestInfo(685, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like -30.123"),
            new HqlTestInfo(686, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '-30.123'"),
            new HqlTestInfo(687, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_numbers.txt' where field1 like '-30.123%'"),

            new HqlTestInfo(66, ResultDirectory, "select decimal(sum(field1), 0), decimal(sum(field1), 1), decimal(sum(field1), 7), sum(field1) from (select '$1,234,112.52', rownum from dual)"),
            new HqlTestInfo(67, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from '..\\..\\..\\Testing\\DS\\test_skip1.txt' as a skip 1 full outer join '..\\..\\..\\Testing\\DS\\test_skip1.txt' as b on a.field1 = b.field1"),
            new HqlTestInfo(68, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from '..\\..\\..\\Testing\\DS\\test_skip1.txt' as a skip 1 full outer join '..\\..\\..\\Testing\\DS\\test_skip1.txt' as b skip = 2 on a.field1 = b.field1"),
            new HqlTestInfo(69, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from '..\\..\\..\\Testing\\DS\\test_skip1.txt' as a skip 1 full outer join '..\\..\\..\\Testing\\DS\\test_skip1.txt' as b skip = 2 on a.field1 = b.field1 order by a.field1"),

            new HqlTestInfo(70, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a full outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1 where a.field1 IS NULL"),
            new HqlTestInfo(71, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a full outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1 where a.field1 IS NOT NULL"),
            new HqlTestInfo(72, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a full outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1 where b.field1 IS NULL"),
            new HqlTestInfo(73, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from \"..\\..\\..\\Testing\\DS\\test_leftrightjoin1.txt\" as a full outer join \"..\\..\\..\\Testing\\DS\\test_leftrightjoin2.txt\" as b on a.field1 = b.field1 where b.field1 IS NOT NULL"),

            new HqlTestInfo(200, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 = b.field1"), // good2 matches oracle 
            new HqlTestInfo(201, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 = \"\""), // good2 BECAUSE \"\" IS NULL IN THIS, doesn't match oracle because '' != NULL select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 = b.field2 where a.field1 = ''
            new HqlTestInfo(202, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 is null"), // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 = b.field2 where a.field1 is null
            new HqlTestInfo(203, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 = b.field1"),  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 = b.field1
            new HqlTestInfo(204, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 != b.field1 order by b.field1"),  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 != b.field1
            new HqlTestInfo(205, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 != b.field1 or a.field1 = b.field1"),  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 != b.field1 or a.field1 = b.field1
            new HqlTestInfo(206, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 != b.field1 and a.field1 = b.field1"),  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 != b.field1 and a.field1 = b.field1
            new HqlTestInfo(207, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2"),  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field2 = b.field2
            new HqlTestInfo(208, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 != b.field2"),  // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field2 != b.field2
            new HqlTestInfo(209, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field1 is null"), // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field1 is null
            new HqlTestInfo(210, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on a.field2 = b.field2 where a.field1 is not null"), // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on a.field2 = b.field2 where a.field1 is not null
            new HqlTestInfo(211, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on 'A' = 'A'"), // good2 matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on 'A' = 'A'
            new HqlTestInfo(212, ResultDirectory, "select a.field1, a.field2, b.field1, b.field2 from (select 'A|B' from dual) as a full outer join (select 'A|C' from dual) as b on 'A' = 'B'"), // matches oracle select a.field1, a.field2, b.field1, b.field2 from (select 'A' as field1, 'B' as field2 from dual) a full outer join (select 'A' as field1, 'C' as field2 from dual) b on 'A' = 'B'

            new HqlTestInfo(401, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(402, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(403, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(404, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(405, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(406, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(407, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 10"),
            new HqlTestInfo(408, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(409, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(410, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(411, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(412, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(413, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(414, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(415, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(416, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(417, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(418, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(419, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(420, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(421, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(422, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(423, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(424, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(425, ResultDirectory, "SELECT field1, len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(426, ResultDirectory, "SELECT field1, len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(427, ResultDirectory, "SELECT len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(428, ResultDirectory, "SELECT sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(429, ResultDirectory, "SELECT sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(430, ResultDirectory, "SELECT len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(431, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field1"),
            new HqlTestInfo(432, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field1"),
            new HqlTestInfo(433, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field1"),
            new HqlTestInfo(434, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field1"),
            new HqlTestInfo(435, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field1"),
            new HqlTestInfo(436, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field1"),
            new HqlTestInfo(437, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field2"),
            new HqlTestInfo(438, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field2"),
            new HqlTestInfo(439, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field2"),
            new HqlTestInfo(440, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 ORDER BY field1"),
            new HqlTestInfo(441, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 ORDER BY field1"),
            new HqlTestInfo(442, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 ORDER BY field1"),
            new HqlTestInfo(443, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1, field2"),
            new HqlTestInfo(444, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1, field2"),
            new HqlTestInfo(445, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1, field2"),
            new HqlTestInfo(446, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1, field2"),
            new HqlTestInfo(447, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1, field2"),
            new HqlTestInfo(448, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1, field2"),
            new HqlTestInfo(449, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 HAVING sum(field1) > 0"),
            new HqlTestInfo(450, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 HAVING sum(field1) > 0"),
            new HqlTestInfo(451, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 HAVING sum(field1) > 0"),
            new HqlTestInfo(452, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 HAVING sum(field1) > 0"),
            new HqlTestInfo(453, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 HAVING sum(field1) > 0"),
            new HqlTestInfo(454, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 HAVING sum(field1) > 0"),
            new HqlTestInfo(455, ResultDirectory, "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 ORDER BY field1"),
            new HqlTestInfo(456, ResultDirectory, "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 ORDER BY field1"),
            new HqlTestInfo(457, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 ORDER BY field1"),
            new HqlTestInfo(458, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 ORDER BY field1"),
            new HqlTestInfo(459, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 ORDER BY field1"),
            new HqlTestInfo(460, ResultDirectory, "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1 ORDER BY field1"),
            new HqlTestInfo(461, ResultDirectory, "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 HAVING sum(field1) > 0"),
            new HqlTestInfo(462, ResultDirectory, "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 HAVING sum(field1) > 0"),
            new HqlTestInfo(463, ResultDirectory, "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 HAVING sum(field1) > 0"),
            new HqlTestInfo(464, ResultDirectory, "SELECT field1, len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(465, ResultDirectory, "SELECT field1, len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(466, ResultDirectory, "SELECT len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(467, ResultDirectory, "SELECT sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(468, ResultDirectory, "SELECT sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(469, ResultDirectory, "SELECT len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(470, ResultDirectory, "SELECT field1, len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(471, ResultDirectory, "SELECT field1, sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(472, ResultDirectory, "SELECT field1, len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(473, ResultDirectory, "SELECT field1, sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(474, ResultDirectory, "SELECT field1, len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(475, ResultDirectory, "SELECT len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(476, ResultDirectory, "SELECT len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(477, ResultDirectory, "SELECT len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(478, ResultDirectory, "SELECT len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(479, ResultDirectory, "SELECT sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(480, ResultDirectory, "SELECT sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(481, ResultDirectory, "SELECT sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(482, ResultDirectory, "SELECT len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(483, ResultDirectory, "SELECT len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(484, ResultDirectory, "SELECT sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field1"),
            new HqlTestInfo(485, ResultDirectory, "SELECT sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(486, ResultDirectory, "SELECT sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(487, ResultDirectory, "SELECT len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(488, ResultDirectory, "SELECT field1, len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(489, ResultDirectory, "SELECT field1, len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(490, ResultDirectory, "SELECT len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(491, ResultDirectory, "SELECT field1, len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(492, ResultDirectory, "SELECT sum(field1), len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            
            new HqlTestInfo(493, "", "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(494, "", "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(495, "", "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),

            new HqlTestInfo(496, "", "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 ORDER BY field1"),
            new HqlTestInfo(497, "", "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 ORDER BY field1"),
            new HqlTestInfo(498, "", "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 ORDER BY field1"),

            new HqlTestInfo(499, "", "SELECT sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(500, "", "SELECT sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(501, "", "SELECT len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),


            new HqlTestInfo(213, ResultDirectory, "select z.rownum, z.field1, a.rownum, b.rownum, c.rownum, d.rownum, e.rownum, f.rownum, g.rownum, h.rownum, a.field1, a.field2, b.field1, b.field2, c.field1, c.field2, d.field1, d.field2, e.field1, e.field2, f.field1, f.field2, g.field1, g.field2, h.field1, h.field2 from (select field1 from '..\\..\\..\\Testing\\DS\\Qacct_*.txt' group by field1 order by field1 with delimiter=',') as z full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2008-11-06.txt' as a on a.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2008-12-09.txt' as b on b.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-01-09.txt' as c on c.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-02-09.txt' as d on d.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-03-09.txt' as e on e.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-04-08.txt' as f on f.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-05-08.txt' as g on g.field1 = z.field1 full outer join '..\\..\\..\\Testing\\DS\\Qacct_AmtDisputed_2009-06-08.txt' as h on h.field1 = z.field1 order by z.rownum with delimiter=',' "),
            new HqlTestInfo(214, ResultDirectory, "select replace(a.filename, '..\\..\\..\\Testing\\', ''), a.rownum, replace(b.filename, '..\\..\\..\\Testing\\', ''), b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\pdt_1_search.txt' as a left outer join '..\\..\\..\\Testing\\DS\\pdt_1_into.txt' as b on a.field1 = b.field1 and substr(a.field2, 1, 10) = substr(b.field2, 1, 10) order by a.rownum"),
            new HqlTestInfo(215, ResultDirectory, "select * from '..\\..\\..\\Testing\\DS\\test_order_by_decimal.txt' order by field3 asc, field1"),
            new HqlTestInfo(216, ResultDirectory, "select * from '..\\..\\..\\Testing\\DS\\test_order_by_decimal.txt' where trim(field1) not in (select trim(field1) from '..\\..\\..\\Testing\\DS\\test_order_by_decimal2.txt' group by field1)"),

            new HqlTestInfo(217, ResultDirectory, "select field1 from '..\\..\\..\\Testing\\DS\\test_xls.txt' group by field1 having sum(field1) > 0 order by sum(field1)"),

            new HqlTestInfo(218, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a left outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1"),
            new HqlTestInfo(219, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a left outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1 order by a.rownum"),
            new HqlTestInfo(220, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a left outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1 order by b.rownum"),
            new HqlTestInfo(221, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a full outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1 where a.rownum IS NOT NULL and b.rownum IS NULL"),
            new HqlTestInfo(222, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a full outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1 where a.rownum IS NULL and b.rownum IS NOT NULL"),
            new HqlTestInfo(223, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a left outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1 order by a.rownum, b.rownum"),
            new HqlTestInfo(224, ResultDirectory, "select a.rownum, b.rownum, a.*, b.* from '..\\..\\..\\Testing\\DS\\test_multijoin1.txt' as a left outer join  '..\\..\\..\\Testing\\DS\\test_multijoin2.txt' as b on a.field1 = b.field1 order by b.rownum, a.rownum"),

            new HqlTestInfo(225, ResultDirectory, "select a.* from '..\\..\\..\\Testing\\DS\\test_groupby.txt' as a skip 1"),
            new HqlTestInfo(226, ResultDirectory, "select a.* from '..\\..\\..\\Testing\\DS\\test_groupby.txt' skip 1 as a"),

                        // TODO, i bet there is a bug with NULL and the "ONJOIN" piece of this
        };
        #endregion

        #region fail_tests
        static HqlTestInfo[] fail_tests = new HqlTestInfo[] {
            new HqlTestInfo(0, "", ""),
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            
            
            
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT field1 FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0 ORDER BY field1 "),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0 ORDER BY field1 "),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0 ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0 ORDER BY field1 "),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT field1, sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' WHERE field1 > 0"),
            new HqlTestInfo(0, "", "SELECT field1, len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT field1, sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT field1, len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' GROUP BY field2"),
            new HqlTestInfo(0, "", "SELECT field1, len(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1, sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1, len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' HAVING sum(field1) > 0"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT field1, sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt' ORDER BY field1"),
            new HqlTestInfo(0, "", "SELECT field1, len(field1), sum(field1) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, len(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, len(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT field1, sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1), len(sum(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(field1), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)), sum(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(field1), sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT sum(field1), len(sum(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT sum(field1), sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
            new HqlTestInfo(0, "", "SELECT len(sum(field1)), sum(len(field1)), len(len(field1)) FROM '..\\..\\..\\Testing\\DS\\test_xls.txt'"),
        };
        #endregion

        static public void PrintAllTestsToStdout()
        {
            for (int i = 0; i < tests.Length; ++i)
            {
                if (Int32.Parse(tests[i].ResultNumber) < 1000)
                    Console.WriteLine(tests[i].Query);
            }
        }

        static public bool RunAndGetOutput(HqlTestInfo test, bool printStack, out string OutResults, bool printException)
        {
            OutResults = String.Empty;

            try
            {   
                StringBuilder sb = new StringBuilder();
                HqlCs hql = new HqlCs(test.Query);
                hql.Compile();
                for (; ; )
                {
                    string s = hql.ReadLine();
                    if (s.Length == 0 && hql.EndOfStream)
                        break;
                    sb.Append(s);
                    sb.Append(System.Environment.NewLine);
                }
                OutResults = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                if (printException)
                {
                    Console.Error.WriteLine(ex.Message + " in query " + test.ResultNumber + " value: " + test.Query);
                    if (printStack)
                    {
                        Console.Error.WriteLine(ex.ToString());
                    }
                }
                return false;
            }
        }

        static public bool CreateOutput()
        {
            Console.Error.WriteLine("Entering CreateOutput");
            bool success = true;
            
            for (int i = 0; i < tests.Length; ++i)
            {
                try
                {
                    if (i == 0)
                    {
                        if (Directory.Exists(tests[i].GetResultDirectory()))
                        {
                            Directory.Move(tests[i].GetResultDirectory(), tests[i].GetNewResultDirectory());
                            Directory.CreateDirectory(tests[i].GetResultDirectory());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CreateOutput() - Unable to move current results directory to backup: " + ex.ToString());
                    success = false;
                    break;
                }
                        
                try
                {
                    string result;
                    if (!RunAndGetOutput(tests[i], true, out result, true))
                    {
                        success = false;
                        continue;
                    }
                    StreamWriter sw = new StreamWriter(tests[i].GetResultFilename());
                    sw.Write(result);
                    sw.Close();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CreateOutput(): " + ex.ToString());
                    success = false;
                }
            }
            return success;
        }

        static public bool CheckOutput(bool printQueryAndResults)
        {
            Console.Error.WriteLine("Entering CheckOutput");
            bool success = true;
            for (int i = 0; i < tests.Length; ++i)
            {
                try
                {
                    string result;
                    if (!RunAndGetOutput(tests[i], true, out result, true))
                    {
                        success = false;
                        continue;
                    }                    
                    StreamReader sr = new StreamReader(tests[i].GetResultFilename());
                    string priorresult = sr.ReadToEnd();
                    sr.Close();

                    if (!result.Equals(priorresult))
                    {
                        success = false;
                        Console.Error.WriteLine("=============================================");
                        Console.Error.WriteLine("REGRESSION FAIL! Test |result_{0}|. Query |{1}|", tests[i].ResultNumber, tests[i].Query);
                        Console.Error.WriteLine("----Expected--------------");
                        Console.Error.Write(priorresult);
                        Console.Error.WriteLine("----Received--------------");
                        Console.Error.Write(result);
                        Console.Error.WriteLine("----END-------------------");
                    }
                    else if (printQueryAndResults)
                    {
                        Console.Error.WriteLine("Test |result_{0}|. Query |{1}|", tests[i].ResultNumber, tests[i].Query);
                        Console.Error.WriteLine("----Received--------------");
                        Console.Error.Write(result);
                        Console.Error.WriteLine("----END-------------------");
                    }                        
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CheckOutput(): " + ex.ToString());
                    success = false;
                }
            }

            for (int i = 0; i < fail_tests.Length; ++i)
            {
                try
                {
                    string result;
                    bool b = RunAndGetOutput(fail_tests[i], false, out result, false);
                    if (b)
                    {
                        success = false;
                        Console.Error.WriteLine("=============================================");
                        Console.Error.WriteLine("REGRESSION FAIL! Test Fail_Query {0} |{1}|", i, fail_tests[i].Query);
                        Console.Error.WriteLine("----END-------------------");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CheckOutput_Fail_Query(): " + ex.ToString());
                    success = false;
                }
            }

            return success;
        }

        // FOR BOOLEAN TESTS, the directory is expected value back
        static HqlTestInfo[] BooleanTests = new HqlTestInfo[]
        {
            new HqlTestInfo(1, "1", "select '1' from dual where (1 > 0 or (1 > 0))"), // T

            // trues
            new HqlTestInfo(1, "1", "select '1' from dual where (1 > 0)"), // T
                new HqlTestInfo(11, "1", "select '1' from dual where 1 > 0 or 1 < 0"), // T or F
                    new HqlTestInfo(111, "1", "select '1' from dual where ((1 > 0) or (1 < 0)) or 1 < 0"), // (T or F) or F
                    new HqlTestInfo(112, "1", "select '1' from dual where (1 > 0 or 1 < 0) or 1 < 0"), // (T or F) or F
                    new HqlTestInfo(113, "1", "select '1' from dual where ((1 < 0) or (1 > 0)) or 1 < 0"), // (F or T) or F
                    new HqlTestInfo(114, "1", "select '1' from dual where (1 < 0 or 1 > 0) or 1 < 0"), // (F or T) or F
                    new HqlTestInfo(115, "1", "select '1' from dual where ((1 > 0) and (1 > 0)) or 1 < 0"), // (T and T) or F
                    new HqlTestInfo(116, "1", "select '1' from dual where (1 > 0 and 1 > 0) or 1 < 0"), // (T and T) or F
                new HqlTestInfo(12, "1", "select '1' from dual where ((1 > 0) or (1 < 0))"), // T or F                    
                new HqlTestInfo(13, "1", "select '1' from dual where 1 < 0 or 1 > 0"), // F or T
                    new HqlTestInfo(131, "1", "select '1' from dual where 1 < 0 or ((1 > 0) or (1 < 0))"), // F or (T or F)
                    new HqlTestInfo(132, "1", "select '1' from dual where (1 < 0) or (1 > 0 or 1 < 0)"), // F or (T or F)
                    new HqlTestInfo(133, "1", "select '1' from dual where 1 < 0 or ((1 < 0) or (1 > 0))"), // F or (F or T)
                    new HqlTestInfo(134, "1", "select '1' from dual where (1 < 0) or (1 < 0 or 1 > 0)"), // F or (F or T)
                    new HqlTestInfo(135, "1", "select '1' from dual where 1 < 0 or ((1 > 0) and (1 > 0))"), // F or (T and T)
                    new HqlTestInfo(136, "1", "select '1' from dual where (1 < 0) or (1 > 0 and 1 > 0)"), // F or (T and T)
                new HqlTestInfo(14, "1", "select '1' from dual where (1 < 0) or (1 > 0)"), // F or T
                new HqlTestInfo(15, "1", "select '1' from dual where 1 > 0 and 1 > 0"), // T and T
                    new HqlTestInfo(151, "1", "select '1' from dual where ((1 > 0) or (1 < 0)) and (1 > 0)"), // (T or F) and T
                    new HqlTestInfo(152, "1", "select '1' from dual where ((1 > 0) or (1 < 0)) and 1 > 0"), // (T or F) and T
                    new HqlTestInfo(153, "1", "select '1' from dual where ((1 < 0) or (1 > 0)) and (1 > 0)"), // (F or T) and T
                    new HqlTestInfo(154, "1", "select '1' from dual where ((1 < 0) or (1 > 0)) and 1 > 0"), // (F or T) and T
                    new HqlTestInfo(155, "1", "select '1' from dual where ((1 > 0) and (1 > 0)) and (1 > 0)"), // (T and T) and T
                    new HqlTestInfo(156, "1", "select '1' from dual where ((1 > 0) and (1 > 0)) and 1 > 0"), // (T and T) and T
                new HqlTestInfo(16, "1", "select '1' from dual where (1 > 0) and (1 > 0)"), // T and T

            // falses
            new HqlTestInfo(2, "", "select '1' from dual where (1 < 0)"), // F
                new HqlTestInfo(21, "", "select '1' from dual where 1 > 0 and 1 < 0"), // T and F
                    new HqlTestInfo(211, "", "select '1' from dual where ((1 > 0) or (1 < 0)) and 1 < 0"), // (T or F) and F
                    new HqlTestInfo(212, "", "select '1' from dual where (1 > 0 or 1 < 0) and 1 < 0"), // (T or F) or F
                    new HqlTestInfo(213, "", "select '1' from dual where ((1 < 0) or (1 > 0)) and 1 < 0"), // (F or T) or F
                    new HqlTestInfo(214, "", "select '1' from dual where (1 < 0 or 1 > 0) and 1 < 0"), // (F or T) or F
                    new HqlTestInfo(215, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or 1 < 0"), // (F or F) or F
                    new HqlTestInfo(216, "", "select '1' from dual where (1 < 0 or 1 < 0) or 1 < 0"), // (F or F) or F
                new HqlTestInfo(22, "", "select '1' from dual where ((1 > 0) and (1 < 0))"), // T and F                    
                new HqlTestInfo(23, "", "select '1' from dual where 1 < 0 and 1 > 0"), // F and T
                    new HqlTestInfo(231, "", "select '1' from dual where 1 < 0 and ((1 > 0) or (1 < 0))"), // F and (T or F)
                    new HqlTestInfo(232, "", "select '1' from dual where (1 < 0) and (1 > 0 or 1 < 0)"), // F and (T or F)
                    new HqlTestInfo(233, "", "select '1' from dual where 1 < 0 and ((1 < 0) or (1 > 0))"), // F and (F or T)
                    new HqlTestInfo(234, "", "select '1' from dual where (1 < 0) and (1 < 0 or 1 > 0)"), // F and (F or T)
                    new HqlTestInfo(235, "", "select '1' from dual where 1 < 0 or ((1 < 0) or (1 < 0))"), // F or (F or F)
                    new HqlTestInfo(236, "", "select '1' from dual where (1 < 0) or (1 < 0 or 1 < 0)"), // F or (F or F)
                new HqlTestInfo(24, "", "select '1' from dual where (1 < 0) and (1 > 0)"), // F and T
                new HqlTestInfo(25, "", "select '1' from dual where 1 < 0 or 1 < 0"), // F or F
                    new HqlTestInfo(251, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or (1 < 0)"), // (F or F) or F
                    new HqlTestInfo(252, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or 1 < 0"), // (F or F) or F
                    new HqlTestInfo(253, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or (1 < 0)"), // (F or F) or F
                    new HqlTestInfo(254, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or 1 < 0"), // (F or F) or F
                    new HqlTestInfo(255, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or (1 < 0)"), // (F or F) or F
                    new HqlTestInfo(256, "", "select '1' from dual where ((1 < 0) or (1 < 0)) or 1 < 0"), // (F or F) or F
                new HqlTestInfo(26, "", "select '1' from dual where (1 < 0) or (1 < 0)"), // F or F

        };

        static public bool CheckLike()
        {
            bool success = true;
            Console.Error.WriteLine("Entering CheckLike");
            object[] tests = new object[]
            {
                "mno", "m%", 0,
                "mno", "mn%", 0,
                "mno", "mno%", 0,
                "mno", "abc%", -1,
                "mno", "xyz%", 1,
                "mno", "ABC%", -1, // due to case-sensitivity
                "mno", "XYZ%", -1, // due to case-sensitivity
                "mno", "%abc", -1,
                "mno", "%m%", 0,
                "mno", "%M%", -1,
                "mno", "%mn%", 0,
                "mno", "%mno%", 0,
                "mno", "%", 0,
                "mno", "", -1,
                "mno", "mn", -1,
                "mno", "n123123123131", 1,
                "mno", "mnoo", 1,
                "mno", "mno", 0,
            };
            for (int i = 0; i < tests.Length; i += 3)
            {
                string f = (string)tests[i];
                string w = (string)tests[i + 1];
                int n = (int)tests[i + 2];
                int result = HqlCompareToken.Like(f, w);
                if (
                    (n == 0 && result != 0) ||
                    (n < 0 && result >= 0) ||
                    (n > 0 && result <= 0)
                    )
                {
                    Console.WriteLine("Comparing |{0}|{1}| Expected {2} Got {3}", f, w, n, result);
                    success = false; 
                }
            }
            Console.Error.WriteLine("Ending CheckBoolean with {0}", (success ? "success" : "failure"));
            return success;
        }


        static public bool CheckBoolean()
        {
            Console.Error.WriteLine("Entering CheckBoolean");
            bool success = true;
            for (int i = 0; i < BooleanTests.Length; ++i)
            {
                HqlTestInfo t = BooleanTests[i];
                try
                {
                    string result;
                    if (!RunAndGetOutput(t, true, out result, true))
                    {
                        success = false;
                        continue;
                    }
                    result = result.Trim();
                    
                    if (!result.Equals(t.ResultDirectory))
                    {
                        success = false;
                        Console.Error.WriteLine("=============================================");
                        Console.Error.WriteLine("REGRESSION FAIL! Test |result_{0}|. Query |{1}|", t.ResultNumber, t.Query);
                        Console.Error.WriteLine("----Expected--------------");
                        Console.Error.WriteLine(t.ResultDirectory);
                        Console.Error.WriteLine("----Received--------------");
                        Console.Error.WriteLine(result);
                        Console.Error.WriteLine("----END-------------------");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CheckOutput(): " + ex.ToString());
                    success = false;
                }
            }
            Console.WriteLine("Ending CheckBoolean with {0}", (success?"success":"failure"));
            return success;
        }

        static string[] datestr = new string[] {
            "2009-06-01",
            "20090601",
            "2009/06/01",
            "june 1, 2009",
            "june 01, 2009",
            "june 01 2009",
            "6/1/2009",
            "06/01/2009",
        };
        static string[] datepairs = new string[] {
            "09-jun", "yy-MMM",
            "june-09", "MMMM-yy",
            "jun 09", "MMM yy",
            "09-06-01", "yy-MM-dd",
        };

        static public bool CheckToDate()
        {
            Console.Error.WriteLine("Entering CheckToDate");
            bool success = true;
            DateTime june12009 = new DateTime(2009, 6, 1);
            for (int i = 0; i < datestr.Length; ++i)
            {
                string date = datestr[i];
                try
                {
                    HqlScalarToDate hqldate = new HqlScalarToDate(null);
                    object o = hqldate.Evaluate(date, "?", null);
                    Console.WriteLine("Type: {0} Value: {1}", o.GetType().ToString(), o.ToString());
                    if (!(o is DateTime) || !((DateTime)o).Equals(june12009))
                        throw new Exception("Date parse failed");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CheckToDate(): " + ex.ToString());
                    success = false;
                }
            }

            for (int i = 0; i < datepairs.Length; i += 2)
            {
                string date = datepairs[i];
                string format = datepairs[i + 1];

                try
                {
                    HqlScalarToDate hqldate = new HqlScalarToDate(null);
                    object o = hqldate.Evaluate(date, format, null);
                    Console.WriteLine("Type: {0} Value: {1}", o.GetType().ToString(), o.ToString());
                    if (!(o is DateTime) || !((DateTime)o).Equals(june12009))
                        throw new Exception("Date parse failed");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error in CheckOutput(): " + ex.ToString());
                    success = false;
                }
            }
            Console.WriteLine("Ending CheckBoolean with {0}", (success ? "success" : "failure"));
            return success;
        }
    }
}
