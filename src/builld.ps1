C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild NanoProfiler2.sln /p:Configuration=Release

.\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe .\Tests\NanoProfiler.Tests\bin\Release\NanoProfiler.Tests.dll
.\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe /cleanup
