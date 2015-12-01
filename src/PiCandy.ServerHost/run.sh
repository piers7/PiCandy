#!/bin/bash
until sudo ./PiCandy.ServerHost.exe "$@"; do
	echo $?
	sleep 5
done