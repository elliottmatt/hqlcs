echo total lines
cat *.cs | wc -l
echo removing blank lines and commented out lines
cat *.cs | grep -v "^$" | grep -v "^[ ]*$" | grep -v "^[ ]*//.*$" | wc -l
echo removing blank, commented, and { } braces on a line by themselves
cat *.cs | grep -v "^$" | grep -v "^[ ]*$" | grep -v "^[ ]*//.*$" | grep -v "^[ ]*{[ ]*$"  | grep -v "^[ ]*}[ ]*$" | wc -l
