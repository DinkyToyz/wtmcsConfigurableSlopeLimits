# Slope Limits (WtM)

This is a mod for [Cities: Skylines](http://www.citiesskylines.com/).

The following is from the [steam description](http://steamcommunity.com/sharedfiles/filedetails/?id=512194601).

---------------------------------------------



Allows slope limits to be configured separately for different network types.

Compatible with [Network Extensions Project](http://steamcommunity.com/sharedfiles/filedetails/?id=478820060), but may lag behind when that mod updates with new or changed road types.

## Usage

The limits can be configured using the mod options (unfortunately the options controls were quite limited, so there's just a bunch of sliders without any display of the numeric values for now), or a configuration file.

In the game the slope limits can be toggled between stricter and looser with this button (near the snap toggle button):
![Button](http://www.truls.org/jonas/CS/ConfigurableSlopeLimitsButtonWeb.png)

The horizontal and vertical button position can be changed in the options (using awkward sliders for now).

The config file, wtmcsConfigurableSlopeLimits.xml, is stored in the folder "ModConfig" wherever the game points to with [`DataLocation.localApplicationData`]. On a Windows system that'd usually be somewhere like "C:\Users\[`UserName`]\AppData\Local\Colossal Order\Cities\_Skylines\ModConfig".

## To-do

- Text fields instead of or combined with sliders.
- Apply to [Traffic++](http://steamcommunity.com/sharedfiles/filedetails/?id=409184143) roads.

## Errors & Logging

When reporting severe errors, please upload [the games complete log file](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931) and/or the separate log file (see below) somewhere and post a link.

The mod logs to [the games normal output log](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931), and can also log to a separate log file, wtmcsConfigurableSlopeLimits.log, stored in the same directory as the settings.

Create the file wtmcsConfigurableSlopeLimits.debug in the same directory in order to enable debug log stuff (wich might slow things down quite a bit) and logging to file. To log more stuff, also create the file wtmcsConfigurableSlopeLimits.debug.dev.

## Whatever

I made this for myself, and use it. Hopefully it works for others as well, but I make no promises.
I also make no promises about updating or fixing things fast, as that depends on how busy I am with work and other stuff.

[Source code](https://github.com/DinkyToyz/wtmcsConfigurableSlopeLimits) is realesed with MIT license.