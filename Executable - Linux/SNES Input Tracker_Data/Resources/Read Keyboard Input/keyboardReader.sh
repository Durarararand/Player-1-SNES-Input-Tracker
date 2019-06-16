#!/bin/bash
function kill(){
    killall keyboardReader;
}
trap kill SIGINT SIGQUIT SIGTERM SIGKILL;

message="Enabling keyboard reading requires administrator privileges.  We will not use your input outside of this gamepad input tracker and never for malicious reasons.  If you have any unease with sharing your keyboard input with another program, please press Cancel below.";

gksudo --message "$message" ./keyboardReader;

