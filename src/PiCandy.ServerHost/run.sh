#!/bin/bash
until ./PiCandy.ServerHost.exe "$@"; do
	echo $?
	sleep 5
done