#  BlinkTorrent <img src="https://github.com/jpmikkers/BlinkTorrent/blob/main/Screenshots/BlinkyBall.svg" height="28"/>
A torrent client, built as a Blazor web based gui for monotorrent.

## Features

- runs on Windows, Linux (and quite possibly Apple, just need a volunteer to test this)
- run on your local system or central server, access the GUI via browser anywhere
- modern blazor-server based single page application
- supports torrent queue ordering, queue limits, seed ratio limits
- the torrent client can bind to specific NICs if needed
- uses [monotorrent](https://github.com/alanmcgovern/monotorrent) as the torrent client engine
- uses the [mudblazor](https://www.mudblazor.com) blazor component library
- fully managed (c#) implementation

## Build & run Instructions (Windows)

First make sure to install the dotnet 8 sdk as per these instructions: [https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net80](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net80)

To build, go to the BlinkTorrent subdirectory that contains BlinkTorrent.csproj. Then run the following commands:

    dotnet clean --configuration Release
    dotnet run --configuration Release

After that, the gui can be reached at [http://localhost:5105](http://localhost:5105)<br/>
To modify this address or port, please edit the `EndPoints` section of `BlinkTorrent.serverconfig.json`

If you run the application, it will create a folder structure at the following location `%localappdata%\blinktorrent`, which typically expands to `C:\Users\SomeUser\AppData\Local\blinktorrent`.

## Build & run Instructions (Linux)

First make sure to install the dotnet 8 sdk as per these instructions: [https://learn.microsoft.com/en-us/dotnet/core/install/linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux)

You can double check which .net sdk you have installed via the following command:

    user@BlinkUbuntu:~/.local/share$ dotnet --list-sdks

To build and run, cd to the BlinkTorrent subdirectory that contains BlinkTorrent.csproj. Then run the following commands:

    dotnet clean --configuration Release
    dotnet run --configuration Release

After that, the gui can be reached at [http://localhost:5105](http://localhost:5105)<br/>
To modify this address or port, please edit the `EndPoints` section of `BlinkTorrent.serverconfig.json`

If you run the application, it will create a folder structure at the following location `~/.local/share/blinktorrent` or `/home/<username>/.local/share/blinktorrent`

## Security

As BlinkTorrent currently does not have any login/authentication/authorization built in yet, you should probably check your router/firewall and make sure your system at port 5105 is not exposed to the outside internet. If you just want see and use the UI from your local system it's best to change the http endpoint in `BlinkTorrent.serverconfig.json` from `http://0.0.0.0:5105` to `http://localhost:5105` or `http://127.0.0.1:5105`, e.g.:

    {
      "Kestrel": {
        "Endpoints": {
          "Http": {
            "Url": "http://127.0.0.1:5105"
          }     
        }
      }
    }

## Screenshots

### Dark mode
![BlinkTorrent dark mode](https://github.com/jpmikkers/BlinkTorrent/blob/main/Screenshots/screendark.png)

### Settings screen
![BlinkTorrent dark mode](https://github.com/jpmikkers/BlinkTorrent/blob/main/Screenshots/settingsdark.png)

### Light mode
![BlinkTorrent light mode](https://github.com/jpmikkers/BlinkTorrent/blob/main/Screenshots/screenlight.png)
