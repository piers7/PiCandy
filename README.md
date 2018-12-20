# PiCandy
A pixel control server, built in .Net/Mono

This is a general purpose server for controlling LED lighting systems (such as NeoPixels / WS2812) to be controlled over networks (using protocols such as OPC). It's inspired by FadeCandy.

(In brief: you can use this to **drive NeoPixels over the network via your Raspberry Pi**)

In it's first incarnation the system will support:

- Clients attaching via the [Open Pixel Control (OPC)] [opc] protocol, as used by [FadeCandy] [fadecandy]. In particular [Processing] sketches targeting FadeCandy _should just work_.
- Driving WS2812 LEDs from a [RPi 2][rpi] via my (mono) fork of the [rpi_ws281x library](https://github.com/piers7/rpi_ws281x/tree/master/mono)

In future I hope this project will support multiple protocols, and multiple (pluggable) driver implementations, with probably a LDP8806 implementation next (because I have a strip of those too) and APA102's.

## Quick Start
At the moment there's no binary distribution, so you'll have to build the source yourself. However there is a script to pull all the pre-requisites down for building on Raspbian. All you'll need up-front is git:

    sudo apt-get update
    sudo apt-get install git
    git clone https://github.com/piers7/PiCandy.git
    cd PiCandy
 
    chmod +x *.sh
 
    # you will need to Y to the certificate installs
    ./setupBuild.sh 
    
    ./build.sh
    ./run.sh

(TODO: I still need to get the nuget package restore working - the above doesn't work as is)

## Why?
[FadeCandy] is great. However I blew up my first board, and whilst I was waiting for a replacement, libraries started to emerge to use RPi's PWM to natively achieve the same thing (more or less). If I could just swap out my FadeCandy server for another OPC implementation, but keep my Processing sketches, I figured I'd be happy. Plus one thing I *don't* like about FadeCandy is the 64 pixel-per-channel limit: running both power and data to multiple strips makes for messy setups IMHO.

## Why Mono?
The .net runtime has some fantastic libraries which make writing servers like this really easy, and it's an area I'm relatively familiar with. Plus I was _trying_ to force myself to write more F# (though this is all in C# at the moment)

[opc]: http://openpixelcontrol.org/
[FadeCandy]: https://github.com/scanlime/fadecandy
[Processing]: https://processing.org/
[rpi]: http://raspberrypi.org


