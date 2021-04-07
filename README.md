# AdRework
Remove spotify ads by seamlessly and automatically muting and skipping them!
## [download latest version here](https://github.com/uDMBK/AdRework/releases/latest)

## Features
* Seamless, Automatic Ad Muting and Skipping
* Starts with Windows Automatically
* Extremely Small File Size (~2mb Total)
* Configurable Settings

## Troubleshooting
### I can't hear normal songs in Spotify!
This is a very rare bug that may occur where AdRework never unmutes the program after an ad, to fix this just go to the volume mixer and turn it back up again. If it is happening consistently then change the Mute Ads setting in the config file to false.
### It's not starting up with Windows!
AdRework adds a registry key to startup with windows as well as a shortcut in the shell:startup folder. If it isn't starting up with windows check if the shortcut is there in shell:startup. If it isn't there, then add it manually.

## Configuration Editing
AdRework creates a folder within your user's Roaming AppData folder (%AppData%\dmbk\AdRework) upon first startup.
In this folder you will find config.ini containing the following 2 settings:
> SkipAds='True'
> MuteAds='True'
You can modify these values to either true or false, disabling both will cause the program to almost instantly terminate whenever started.
