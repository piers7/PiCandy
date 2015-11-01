# PiCandy
A pixel control server, built in .Net/Mono

**NOT EVEN ALPHA. Nothing to see here. Move along.**

This is a general purpose .net/mono server allowing LED lighting systems such as WS2812 to be controlled over networks, using protocols such as OPC. It's inspired by FadeCandy.

In it's first incarnation the system will support:

- Clients attaching via the [Open Pixel Control (OPC)] [opc] protocol, as used by [FadeCandy] [fadecandy] (ie [Processing] sketches targeting FadeCandy should 'just work').
- Driving WS2812 LEDs from a [RPi 2][rpi] via my (mono) fork of the [rpi_ws281x library](https://github.com/piers7/rpi_ws281x/tree/master/mono)

In future I hope this project will support multiple protocols, and multiple (pluggable) driver implementations, with probably a LDP8806 implementation next (because I have a strip of those too).

## Why?
[FadeCandy] [fadecandy] is great. However I blew up my first board, and whilst I was waiting for a replacement, libraries started to emerge to use RPi's PWM to natively achieve the same thing (more or less). If I could just swap out my FadeCandy server for another OPC implementation, but keep my Processing sketches, I figured I'd be happy. Plus one thing I *don't* like about FadeCandy is the 64 pixel-per-channel limit..

##Why Mono?
The .net runtime has some fantastic libraries which make writing servers like this really easy, and it's an area I'm relatively familiar with. Plus I was trying to force myself to write more F#

[opc]: http://openpixelcontrol.org/
[FadeCandy]: https://github.com/scanlime/fadecandy
[Processing]: https://processing.org/
[rpi]: http://raspberrypi.org