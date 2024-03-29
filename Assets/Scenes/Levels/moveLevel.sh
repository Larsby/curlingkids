#!/bin/bash
# file: moveLevel.sh

if [ "$#" -ne 3 ]; then
    echo Usage: worldIndex levelFromIndex levelToIndex
    exit
fi

if [ ! -f ./level_$1_$2.unity ]; then
    echo level_$1_$2.unity not found!
    exit
fi

if [ $2 -eq $3 ]; then 
    echo Move file to itself? No, man…
    exit
fi

if [ ! -f ./level_$1_$3.unity ]; then
    echo Target level_$1_$3.unity does not exist. Moving file without any other changes.
    mv level_$1_$2.unity level_$1_$3.unity
    exit
fi

mv level_$1_$2.unity temp.unity

if [ $3 -lt $2 ]; then 
   let CNT=$2
   let CNTMM=$2-1
   let STOP=$3
   while [  $CNT -gt $STOP ]; do
      mv level_$1_$CNTMM.unity level_$1_$CNT.unity
      let CNT=CNT-1 
      let CNTMM=CNTMM-1 
   done
else
   let CNT=$2
   let CNTPP=$2+1
   let STOP=$3
   while [  $CNT -lt $STOP ]; do
      mv level_$1_$CNTPP.unity level_$1_$CNT.unity
      let CNT=CNT+1 
      let CNTPP=CNTPP+1 
   done
fi

mv temp.unity level_$1_$3.unity
