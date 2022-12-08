# BlinkTorrent
Blazor web based gui frontend for monotorrent. Please note: this project is still very much in development.

- modern blazor-server based single page application
- features queue ordering, queue limits, seed ratio limits
- uses [monotorrent](https://github.com/alanmcgovern/monotorrent) as the torrent client engine
- uses the [mudblazor](https://www.mudblazor.com) blazor component library
- fully managed (c#) implementation

## Build & run Instructions
To build, go to the BlinkTorrent subdirectory that contains BlinkTorrent.csproj. Then run the following commands:

    dotnet clean
    dotnet restore
    dotnet run

After that, the gui can be reached at [http://localhost:5105](http://localhost:5105)

If you run the application, it will create a folder structure at the following location `%localappdata%\blinktorrent`, which typically expands to `C:\Users\SomeUser\AppData\Local\blinktorrent`.

## Screenshots

### Dark mode
![BlinkTorrent dark mode](https://github.com/jpmikkers/BlinkTorrent/blob/main/Screenshots/screendark.png)

### Light mode
![BlinkTorrent light mode](https://github.com/jpmikkers/BlinkTorrent/blob/main/Screenshots/screenlight.png)
