echo "Installing pre-reqs"
sudo apt-get update
sudo apt-get install scons mono-complete nuget -y

echo "Setup trusted roots, or nuget package restore won't work"
# see http://stackoverflow.com/questions/15181888/nuget-on-linux-error-getting-response-stream
# sudo mozroots --import --machine --sync
# Updated: https://stackoverflow.com/a/30970569/26167
sudo mozroots --import --machine --sync --url https://hg.mozilla.org/mozilla-central/raw-file/tip/security/nss/lib/ckfw/builtins/certdata.txt

# following need 'Y's sent to them - maybe simplest as a manual step
sudo certmgr -ssl -m https://go.microsoft.com
sudo certmgr -ssl -m https://nugetgallery.blob.core.windows.net
sudo certmgr -ssl -m https://nuget.org