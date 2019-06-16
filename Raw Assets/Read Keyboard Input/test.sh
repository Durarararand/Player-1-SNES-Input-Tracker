#/bin/bash
function kill(){
    killall test.sh;
}
trap kill SIGINT SIGQUIT SIGTERM SIGKILL;

gksudo echo "yo" 2> /dev/null;

