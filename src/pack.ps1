del bin\NuGet\*.*
rd bin\NuGet
rd bin

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild NanoProfiler2.sln /p:Configuration=Release
.\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe .\Tests\NanoProfiler.Tests\bin\Release\NanoProfiler.Tests.dll
.\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe /cleanup

.nuget/nuget pack NanoProfiler\NanoProfiler.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Data\NanoProfiler.Data.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Storages.Json\NanoProfiler.Storages.Json.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Web\NanoProfiler.Web.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Web.Import\NanoProfiler.Web.Import.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Wcf\NanoProfiler.Wcf.csproj -Prop Configuration=Release -Sym

nuget push *.symbols.nupkg 123 -Source http://nuget.gw.symbolsource.org/Public/NuGet
del *.symbols.nupkg

md bin
md bin\NuGet
move *.nupkg bin\NuGet