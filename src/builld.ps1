& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe' NanoProfiler2.Net45.sln /p:Configuration=Release
Remove-Item .\NanoProfiler.Data\bin\net45 -recurse -force
Copy-Item .\NanoProfiler.Data\bin\Release .\NanoProfiler.Data\bin\net45 -recurse

& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe' NanoProfiler2.sln /p:Configuration=Release
Remove-Item .\NanoProfiler.Data\bin\net40 -recurse -force
Copy-Item .\NanoProfiler.Data\bin\Release .\NanoProfiler.Data\bin\net40 -recurse

.\packages\NUnit.ConsoleRunner.3.10.0\tools\nunit3-console.exe .\Tests\NanoProfiler.Tests\bin\Release\NanoProfiler.Tests.dll

