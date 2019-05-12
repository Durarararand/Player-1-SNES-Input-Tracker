#!/bin/bash
function kill(){
    killall joystick_SNESInputTracker;
}
trap kill SIGINT SIGQUIT SIGTERM SIGKILL;

stdbuf -oL ./joystick_SNESInputTracker;

