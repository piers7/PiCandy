#!/bin/sh
sudo cp PiCandy.ServerHost/picandy.sh /etc/init.d/
sudo chmod +x /etc/init.d/picandy.sh
sudo update-rc.d picandy.sh defaults