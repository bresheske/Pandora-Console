# Pandora-Console
A small console tool which provides some basic functionality to the pandora API. Also hooks into [Youtube-dl](https://rg3.github.io/youtube-dl) and [FFMpeg](https://ffmpeg.org/download.html).

## Dependencies
- [Youtube-dl](https://rg3.github.io/youtube-dl)
- [FFMpeg](https://ffmpeg.org/download.html)

These binaries should be installed somewhere on your harddrive and available through the system environment's PATH variable. Meaning, you should be able to open up 'cmd' and type 'ffmpeg' and it should list out ffmpeg's help menu regardless of your working directory.

## Features
The main feature is to download all of the music from a pandora station which has been marked as 'thumbs up'.
Additionally, the tool also:
- Lists out the user's stations.
- Lists out the songs marked 'thumbs up' from a station.
- Performs a search based on an input string.

## Examples
- To show the help menu
```
Pandora.Console.exe -h
```
- To list all of your stations
```
Pandora.Console.exe -u (pandora_username) -p (pandora_password) -l
```
- To search for other artists/songs
```
Pandora.Console.exe -u (pandora_username) -p (pandora_password) -search (search_string)
```
- To list all songs marked thumbs up in one of your stations
```
Pandora.Console.exe -u (pandora_username) -p (pandora_password) -l -s (station_name)
```
- To download all songs marked thumbs up in one of your stations
```
Pandora.Console.exe -u (pandora_username) -p (pandora_password) -dl -s (station_name)
```
