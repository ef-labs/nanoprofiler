NanoProfiler
============

NanoProfiler is a light weight profiling library written in C# which requires .NET 4.0+. It was inspired by the MiniProfiler project, but is designed for high performance, big-data analytics, and is easy to be used for both sync & async programming model. It has been used in EF (Education First) projects generating billions of profiling events per day with trivial performance penalty.

NanoProfiler itself implements the core profiling feature and a simple implementation for persisting results via slf4net. If you want better profiling result management feature, you could implement the IProfilingStorage interface by yourself.

NanoProfiler also provides a wonderful view-result Web UI supports view latest profiling results in a tree-timeline view (simply visit ~/nanoprofiler/view in your web application). 

For more documentations, please check out wiki pages: https://github.com/englishtown/nanoprofiler/wiki

How to compile the source code?
-------------------------------
Do the following steps before opening NanoProfiler.sln in Visual Studio 2012 +

- git clone
- cd src
- run .\build.ps1

License terms
-------------
NanoProfiler is published under the [MIT license](http://englishtown.mit-license.org).

About v2.0
----------

Changes since latest version of v1:

- Profiling engine redesign for better performance & less memory concumption;
- Enhanced View-Results UI to display all custom profiling tags & fields;
- Simplified profiling configuration (zero configuration for simple use cases);
- New configuration provider interface for easier integration with existing frameworks;
- New NanoProfiler.Web.Import components for importing and visualizing profiling sessions from log files or elasticsearch;
- New generic custom web request timing support;