del hqlcs_*.txt
hqlcs "SELECT field1 FROM 'a.a'"                                                         > hqlcs_1.txt
hqlcs "SELECT len(field1) FROM 'a.a'"							 > hqlcs_2.txt
hqlcs "SELECT sum(field1) FROM 'a.a'"							 > hqlcs_3.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a'"						 > hqlcs_4.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a'"						 > hqlcs_5.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a'"						 > hqlcs_6.txt
hqlcs "SELECT field1 FROM 'a.a' WHERE field1 > 10"					 > hqlcs_7.txt
hqlcs "SELECT len(field1) FROM 'a.a' WHERE field1 > 0"					 > hqlcs_8.txt
hqlcs "SELECT sum(field1) FROM 'a.a' WHERE field1 > 0"					 > hqlcs_9.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' WHERE field1 > 0"				 > hqlcs_10.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' WHERE field1 > 0"				 > hqlcs_11.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' WHERE field1 > 0"				 > hqlcs_12.txt
hqlcs "SELECT field1 FROM 'a.a' GROUP BY field1"					 > hqlcs_13.txt
hqlcs "SELECT len(field1) FROM 'a.a' GROUP BY field1"					 > hqlcs_14.txt
hqlcs "SELECT sum(field1) FROM 'a.a' GROUP BY field1"					 > hqlcs_15.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' GROUP BY field1"				 > hqlcs_16.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' GROUP BY field1"				 > hqlcs_17.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' GROUP BY field1"				 > hqlcs_18.txt
hqlcs "SELECT sum(field1) FROM 'a.a' GROUP BY field2"					 > hqlcs_19.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' GROUP BY field2"				 > hqlcs_20.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' GROUP BY field2"				 > hqlcs_21.txt
hqlcs "SELECT field1 FROM 'a.a' ORDER BY field1"					 > hqlcs_22.txt
hqlcs "SELECT len(field1) FROM 'a.a' ORDER BY field1"					 > hqlcs_23.txt
hqlcs "SELECT sum(field1) FROM 'a.a' ORDER BY field1"					 > hqlcs_24.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' ORDER BY field1"				 > hqlcs_25.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' ORDER BY field1"				 > hqlcs_26.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' ORDER BY field1"				 > hqlcs_27.txt
hqlcs "SELECT sum(field1) FROM 'a.a' HAVING sum(field1) > 0"				 > hqlcs_28.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' HAVING sum(field1) > 0"			 > hqlcs_29.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' HAVING sum(field1) > 0"			 > hqlcs_30.txt
hqlcs "SELECT field1, len(field1) FROM 'a.a'"						 > hqlcs_31.txt
hqlcs "SELECT field1, len(len(field1)) FROM 'a.a'"					 > hqlcs_32.txt
hqlcs "SELECT len(field1), len(len(field1)) FROM 'a.a'"					 > hqlcs_33.txt
hqlcs "SELECT sum(field1), len(sum(field1)) FROM 'a.a'"					 > hqlcs_34.txt
hqlcs "SELECT sum(field1), sum(len(field1)) FROM 'a.a'"					 > hqlcs_35.txt
hqlcs "SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a'"				 > hqlcs_36.txt
hqlcs "SELECT field1 FROM 'a.a' WHERE field1 > 0 GROUP BY field1"			 > hqlcs_37.txt
hqlcs "SELECT len(field1) FROM 'a.a' WHERE field1 > 0 GROUP BY field1"			 > hqlcs_38.txt
hqlcs "SELECT sum(field1) FROM 'a.a' WHERE field1 > 0 GROUP BY field1"			 > hqlcs_39.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' WHERE field1 > 0 GROUP BY field1"		 > hqlcs_40.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' WHERE field1 > 0 GROUP BY field1"		 > hqlcs_41.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' WHERE field1 > 0 GROUP BY field1"		 > hqlcs_42.txt
hqlcs "SELECT sum(field1) FROM 'a.a' WHERE field1 > 0 GROUP BY field2"			 > hqlcs_43.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' WHERE field1 > 0 GROUP BY field2"		 > hqlcs_44.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' WHERE field1 > 0 GROUP BY field2"		 > hqlcs_45.txt
hqlcs "SELECT field1 FROM 'a.a' WHERE field1 > 0 ORDER BY field1"			 > hqlcs_46.txt
hqlcs "SELECT len(field1) FROM 'a.a' WHERE field1 > 0 ORDER BY field1"			 > hqlcs_47.txt
hqlcs "SELECT sum(field1) FROM 'a.a' WHERE field1 > 0 ORDER BY field1"			 > hqlcs_48.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' WHERE field1 > 0 ORDER BY field1"		 > hqlcs_49.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' WHERE field1 > 0 ORDER BY field1"		 > hqlcs_50.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' WHERE field1 > 0 ORDER BY field1"		 > hqlcs_51.txt
hqlcs "SELECT sum(field1) FROM 'a.a' WHERE field1 > 0 HAVING sum(field1) > 0"		 > hqlcs_52.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' WHERE field1 > 0 HAVING sum(field1) > 0"	 > hqlcs_53.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' WHERE field1 > 0 HAVING sum(field1) > 0"	 > hqlcs_54.txt
hqlcs "SELECT field1 FROM 'a.a' GROUP BY field1, field2"				 > hqlcs_55.txt
hqlcs "SELECT len(field1) FROM 'a.a' GROUP BY field1, field2"				 > hqlcs_56.txt
hqlcs "SELECT sum(field1) FROM 'a.a' GROUP BY field1, field2"				 > hqlcs_57.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' GROUP BY field1, field2"			 > hqlcs_58.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' GROUP BY field1, field2"			 > hqlcs_59.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' GROUP BY field1, field2"			 > hqlcs_60.txt
hqlcs "SELECT field1 FROM 'a.a' GROUP BY field1 ORDER BY field1"			 > hqlcs_61.txt
hqlcs "SELECT len(field1) FROM 'a.a' GROUP BY field1 ORDER BY field1"			 > hqlcs_62.txt
hqlcs "SELECT sum(field1) FROM 'a.a' GROUP BY field1 ORDER BY field1"			 > hqlcs_63.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' GROUP BY field1 ORDER BY field1"		 > hqlcs_64.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' GROUP BY field1 ORDER BY field1"		 > hqlcs_65.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' GROUP BY field1 ORDER BY field1"		 > hqlcs_66.txt
hqlcs "SELECT field1 FROM 'a.a' GROUP BY field1 HAVING sum(field1) > 0"			 > hqlcs_67.txt
hqlcs "SELECT len(field1) FROM 'a.a' GROUP BY field1 HAVING sum(field1) > 0"		 > hqlcs_68.txt
hqlcs "SELECT sum(field1) FROM 'a.a' GROUP BY field1 HAVING sum(field1) > 0"		 > hqlcs_69.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' GROUP BY field1 HAVING sum(field1) > 0"	 > hqlcs_70.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' GROUP BY field1 HAVING sum(field1) > 0"	 > hqlcs_71.txt
hqlcs "SELECT len(len(field1)) FROM 'a.a' GROUP BY field1 HAVING sum(field1) > 0"	 > hqlcs_72.txt
hqlcs "SELECT sum(field1) FROM 'a.a' GROUP BY field2 HAVING sum(field1) > 0"		 > hqlcs_73.txt
hqlcs "SELECT len(sum(field1)) FROM 'a.a' GROUP BY field2 HAVING sum(field1) > 0"	 > hqlcs_74.txt
hqlcs "SELECT sum(len(field1)) FROM 'a.a' GROUP BY field2 HAVING sum(field1) > 0"	 > hqlcs_75.txt
hqlcs "SELECT field1, len(field1) FROM 'a.a' WHERE field1 > 0"				 > hqlcs_76.txt
hqlcs "SELECT field1, len(len(field1)) FROM 'a.a' WHERE field1 > 0"			 > hqlcs_77.txt
hqlcs "SELECT len(field1), len(len(field1)) FROM 'a.a' WHERE field1 > 0"		 > hqlcs_78.txt
hqlcs "SELECT sum(field1), len(sum(field1)) FROM 'a.a' WHERE field1 > 0"		 > hqlcs_79.txt
hqlcs "SELECT sum(field1), sum(len(field1)) FROM 'a.a' WHERE field1 > 0"		 > hqlcs_80.txt
hqlcs "SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a' WHERE field1 > 0"		 > hqlcs_81.txt
hqlcs "SELECT field1, len(field1) FROM 'a.a' GROUP BY field1"				 > hqlcs_82.txt
hqlcs "SELECT field1, sum(field1) FROM 'a.a' GROUP BY field1"				 > hqlcs_83.txt
hqlcs "SELECT field1, len(sum(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_84.txt
hqlcs "SELECT field1, sum(len(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_85.txt
hqlcs "SELECT field1, len(len(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_86.txt
hqlcs "SELECT len(field1), sum(field1) FROM 'a.a' GROUP BY field1"			 > hqlcs_87.txt
hqlcs "SELECT len(field1), len(sum(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_88.txt
hqlcs "SELECT len(field1), sum(len(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_89.txt
hqlcs "SELECT len(field1), len(len(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_90.txt
hqlcs "SELECT sum(field1), len(sum(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_91.txt
hqlcs "SELECT sum(field1), sum(len(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_92.txt
hqlcs "SELECT sum(field1), len(len(field1)) FROM 'a.a' GROUP BY field1"			 > hqlcs_93.txt
hqlcs "SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a' GROUP BY field1"		 > hqlcs_94.txt
hqlcs "SELECT len(sum(field1)), len(len(field1)) FROM 'a.a' GROUP BY field1"		 > hqlcs_95.txt
hqlcs "SELECT sum(len(field1)), len(len(field1)) FROM 'a.a' GROUP BY field1"		 > hqlcs_96.txt
hqlcs "SELECT sum(field1), len(sum(field1)) FROM 'a.a' GROUP BY field2"			 > hqlcs_97.txt
hqlcs "SELECT sum(field1), sum(len(field1)) FROM 'a.a' GROUP BY field2"			 > hqlcs_98.txt
hqlcs "SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a' GROUP BY field2"		 > hqlcs_99.txt
hqlcs "SELECT field1, len(field1) FROM 'a.a' ORDER BY field1"				 > hqlcs_100.txt
hqlcs "SELECT field1, len(len(field1)) FROM 'a.a' ORDER BY field1"			 > hqlcs_101.txt
hqlcs "SELECT len(field1), len(len(field1)) FROM 'a.a' ORDER BY field1"			 > hqlcs_102.txt
hqlcs "SELECT sum(field1), len(sum(field1)) FROM 'a.a' ORDER BY field1"			 > hqlcs_103.txt
hqlcs "SELECT sum(field1), sum(len(field1)) FROM 'a.a' ORDER BY field1"			 > hqlcs_104.txt
hqlcs "SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a' ORDER BY field1"		 > hqlcs_105.txt
hqlcs "SELECT sum(field1), len(sum(field1)) FROM 'a.a' HAVING sum(field1) > 0"		 > hqlcs_106.txt
hqlcs "SELECT sum(field1), sum(len(field1)) FROM 'a.a' HAVING sum(field1) > 0"		 > hqlcs_107.txt
hqlcs "SELECT len(sum(field1)), sum(len(field1)) FROM 'a.a' HAVING sum(field1) > 0"	 > hqlcs_108.txt
hqlcs "SELECT field1, len(field1), len(len(field1)) FROM 'a.a'"				 > hqlcs_109.txt
hqlcs "SELECT sum(field1), len(sum(field1)), sum(len(field1)) FROM 'a.a'"		 > hqlcs_110.txt
