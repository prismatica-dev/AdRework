# AdRework
Remove spotify ads by seamlessly and automatically muting and skipping them!
## [download latest version here](https://github.com/uDMBK/AdRework/releases/latest)

## Features
* Seamless, Automatic Ad Muting and Skipping
* Starts with Windows Automatically
* Extremely Small File Size (~2mb Total)
* Configurable Settings

## Troubleshooting
### It ain't openin
if it isn't opening and you are not using a custom configuration file (or you are with ForceRun enabled) then chances are you are missing the redistributables to be able to run this software, please select one below that is suitable for your system
* x86: [vc_redist.x86.exe](https://aka.ms/vs/16/release/vc_redist.x86.exe) (32-bit)
* x64: [vc_redist.x64.exe](https://aka.ms/vs/16/release/vc_redist.x64.exe) (64-bit)
* ARM64: [vc_redist.arm64.exe](https://aka.ms/vs/16/release/VC_redist.arm64.exe)

### I can't hear normal songs in Spotify!
as of version 0.2.1, this should **NEVER** occur unless you have set the fallbackvolume in the configuration to 0!
this will almost **ALWAYS** occur on version 0.2.0 due to a bug with with fallbackvolume

~this is a very rare bug that may occur where AdRework never unmutes the program after an ad, to fix this just go to the volume mixer and turn it back up again. If it is happening consistently then change the Mute Ads setting in the config file to false.~

### It's not starting up with Windows!
AdRework adds a registry key to startup with windows as well as a shortcut in the shell:startup folder. If it isn't starting up with windows check if the shortcut is there in shell:startup. If it isn't there, then add it manually, alternatively run AdRework as an administrator in case this process requires elevated permissions on your system.

### In my config file, it says 'CONFIG RESET DUE TO LOADING ERROR' - why?
if you are seeing this in your config file, then shockingly, your configuration file was reset due to, you guessed it, a loading error. if you set an invalid value for any config option or update to a newer version with changes to the configuration this will be added to the start of the file. it wont break AdRework or anything, its only there to inform you in case you had a custom config that it was reset.

### There is one particular song that is always muted and skipped by AdRework!
this shouldn't occur however as there are countless songs on spotify i cannot possibly test them all, if this occurs please contact me on discord (@dmbk#0255) with the song name and artist or (preferably) with a link to the song

### AdRework isn't in my task manager / AdRework is giving me a crash message whenever started
in either of these circumstances, AdRework has crashed! make sure you check your background processes first just in case (unless you are literally getting a crash message) this should never occur. if it is happening i genuinely have no clue whats happening. the only possible scenario i can think of is if the FallbackVolume config setting is set beyond 100 or less than 0, or if an anti-virus is for some reason interfering with the program being falsely detected as a virus (it's not - the releases are not obfuscated and can be reversed with dnSpy to prove this as well as AdRework literally being open source)

## Configuration Editing
AdRework creates a folder within your user's Roaming AppData folder (%AppData%\dmbk\AdRework) upon first startup.
In this folder you will find config.ini containing the following settings:
* SkipAds='True'
* MuteAds='True'
* BypassAds='True'
* ImmediateSkip='True'
* RegistryStartup='True'
* ForceRun='False'
* MuteAll='True'
* FallbackVolume='100'
* AdInterval='100'
* IntegrityInterval='450'
#### You can modify these values to either true or false (except FallbackVolume requiring an integer value between 0 - 100 as well as AdInterval and IntegrityInterval requiring a valid 32-bit signed integer above 0), disabling SkipAds, MuteAds and AdBypass will cause the program to almost instantly terminate whenever started unless ForceRun is set to true.

### Skip Ads / Mute Ads
specifies if AdRework should skip/mute ads respectively

### Bypass Ads
if spotify should be muted when no song name is returned (results in spotify also being muted whenever paused but this is not noticable as well,, there isnt any music playing)

### Immediate Skip
disabling this will result in AdRework waiting 5 seconds to skip all and any ads. not recommended, but if you want the multimedia popup to show up for less time or have an absolute hate-fueled rage for the 'wait to skip ads' banner in spotify then this option is for you

### Registry Startup
some people dont like AdRework touching the registry. this is understandable and disabling this will disable AdRework attempting to run at startup through the registry (doesn't remove any existing keys created though)

### Force Run
if bypass ads, skip ads and mute ads are all disabled then the program will immediately terminate. this setting overrides that if set to true and will keep the program running regardless and is currently the only setting set to disabled by default. additionally it will still perform the integrity check if the fallback volume is set to 0%

### Mute All
if this is set to true, AdRework will attempt to mute **ALL** spotify processes, not just the main one. this is required to mute video ads, and whilst i can't see any issues occuring from this, it's provided as a 'just in case it doesn't work' option

### Fallback Volume
this setting is to fix the previously extremely rare bug that would case spotify to be muted during actual music, setting the volume back to a 'fallback' volume instead of being muted. setting this to 0 will disable it however is not recommended to be used with BypassAds on as it will result in spotify being almost always muted

### Ad + Integrity Check Intervals
these are integer settings specifying the millisecond interval that AdRework should check for ads at and also check that the program is not muted, by default these values are fine but if you wish to reduce the cpu load further increase the interval at which the integrity check occurs as it can be rather demanding on the system, unlike the ad check which doesn't really cause that much additional load.

#### It is my dream and life goal to run Version 0.2.0, however it's always muted, what can I do to restore it to a usable, but still buggy state?
set your configuration file to the following:
* SkipAds='True'
* MuteAds='True'
* BypassAds='False'
* ImmediateSkip='True'
* RegistryStartup='True'
* FallbackVolume='1'
##### the adbypass wont do anything in this version and fallback volume must be set to 1 as it specifies 100% in 0.2.0
