@echo off
dotnet restore
dotnet publish --no-restore -p:Platform=x64 -r win-x64 ./Pandora.csproj -c Release --self-contained true -p:DebugType=None -p:DebugSymbols=false
dotnet publish --no-restore -p:Platform=x64 -r linux-x64 ./Pandora.csproj -c Release --self-contained true -p:DebugType=None -p:DebugSymbols=false
dotnet publish --no-restore -p:Platform=x64 -r osx-x64 ./Pandora.csproj -c Release --self-contained true -p:DebugType=None -p:DebugSymbols=false
pause