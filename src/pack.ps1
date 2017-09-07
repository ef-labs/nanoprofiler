del bin\NuGet\*.*
rd bin\NuGet
rd bin

.nuget/nuget pack NanoProfiler\NanoProfiler.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Data\NanoProfiler.Data.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Storages.Json\NanoProfiler.Storages.Json.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Web\NanoProfiler.Web.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Web.Import\NanoProfiler.Web.Import.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.Wcf\NanoProfiler.Wcf.csproj -Prop Configuration=Release -Sym
.nuget/nuget pack NanoProfiler.EF\NanoProfiler.EF.csproj -Prop Configuration=Release -Sym

md bin
md bin\NuGet
move *.nupkg bin\NuGet