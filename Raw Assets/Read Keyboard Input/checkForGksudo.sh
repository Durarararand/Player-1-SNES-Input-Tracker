#! /bin/bash

# This script returns 1 if gksudo is installed, 0 otherwise.
checkString=$(whereis gksudo);
checkString=$(echo "$checkString" | sed 's/.*://' | sed 's/ //g');

if [[ "$checkString" == "" ]]
then
	echo "0";
else
	echo "1";
fi;

