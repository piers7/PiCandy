sudo apt-get install scons mono-complete 

# setup trusted roots, or nuget package restore won't work
# see http://stackoverflow.com/questions/15181888/nuget-on-linux-error-getting-response-stream
sudo mozroots --import --machine --sync

# following need 'Y's sent to them - maybe simplest as a manual step
sudo certmgr -ssl -m https://go.microsoft.com
sudo certmgr -ssl -m https://nugetgallery.blob.core.windows.net
sudo certmgr -ssl -m https://nuget.org