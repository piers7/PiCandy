# download nuget command line
curl https://dist.nuget.org/win-x86-commandline/latest/nuget.exe > nuget.exe

# run it (don't need to make it executable this way)
mono nuget.exe restore

# build the solution
xbuild

# if no libws2011.so need to build that too
# Build and copy the WS281x driver to lib
pushd ../
git clone https://github.com/piers7/rpi_ws281x
cd rpi_ws281x/mono
./build.sh
sudo cp ../libws2811.so /lib
popd