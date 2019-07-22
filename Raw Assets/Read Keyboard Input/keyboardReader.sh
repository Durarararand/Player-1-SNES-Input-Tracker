#!/bin/bash

message="Note: Enabling keyboard reading requires administrator privileges.  We will not use your input outside of this gamepad input tracker and never for malicious reasons.  If you are uncomfortable with sharing your keyboard input with the SNES Input Tracker, please press Cancel below.";

gksudo --message "$message" ./keyboardReader;

