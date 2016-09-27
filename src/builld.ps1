& 'C:\Program Files (x86)\MSBuild\12.0\Bin\msbuild.exe' NanoProfiler2.sln /p:Configuration=Release

.\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe .\Tests\NanoProfiler.Tests\bin\Release\NanoProfiler.Tests.dll
.\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe /cleanup
