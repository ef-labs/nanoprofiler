del bin\NuGet\*.*
rd bin\NuGet
rd bin

nuget pack NanoProfiler\NanoProfiler.csproj -Properties Configuration=Release -Symbols
nuget pack NanoProfiler.Data\NanoProfiler.Data.csproj -Properties Configuration=Release -Symbols
nuget pack NanoProfiler.Storages.Json\NanoProfiler.Storages.Json.csproj -Properties Configuration=Release -Symbols
nuget pack NanoProfiler.Web\NanoProfiler.Web.csproj -Properties Configuration=Release -Symbols
nuget pack NanoProfiler.Web.Import\NanoProfiler.Web.Import.csproj -Properties Configuration=Release -Symbols
nuget pack NanoProfiler.Wcf\NanoProfiler.Wcf.csproj -Properties Configuration=Release -Symbols
nuget pack NanoProfiler.EF\NanoProfiler.EF.csproj -Properties Configuration=Release -Symbols

md bin
md bin\NuGet
move *.nupkg bin\NuGet
