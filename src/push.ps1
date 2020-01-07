nuget push .\bin\NuGet\*.symbols.nupkg -Source https://nuget.smbsrc.net/
del .\bin\NuGet\*.symbols.nupkg
nuget push .\bin\NuGet\*.nupkg -Source https://www.nuget.org
