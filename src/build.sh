#!/bin/bash
set -e

# download nuget command line if not present already
# if [ ! -e nuget.exe ]; then
#     echo "Downloading latest nuget.exe"
#     curl https://github.com/mono/nuget-binary/blob/master/nuget.exe > nuget.exe
#     #curl https://dist.nuget.org/win-x86-commandline/latest/nuget.exe > nuget.exe
# fi

# no - use apt-get package now!
# https://stackoverflow.com/questions/38118548/how-to-install-nuget-from-command-line-on-linux

# run it
if [ ! -d packages ]; then
    echo "Package restore..."
    # mono --runtime=v4.0 nuget.exe restore PiCandy.sln
    nuget restore PiCandy.sln
fi

if [ ! -e "/lib/libws2011.so" ]; then
    echo "Building libws2011.so (dependency)"
    pushd rpi_ws281x/mono
    ./build.sh
    sudo cp ../libws2811.so /lib
    popd
fi

echo "Building solution"
# build the solution (nb: not yet using FAKE here)
xbuild