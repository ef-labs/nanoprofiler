NanoProfiler
============

NanoProfiler is a light weight profiling library written in C# which requires .NET 4.0+. It was inspired by the MiniProfiler project, but is designed for high performance, big-data analytics, and is easy to be used for both sync & async programming model. It has been used in EF (Education First) projects generating billions of profiling events per day with trivial performance penalty.

NanoProfiler itself implements the core profiling feature and a simple implementation for persisting results via slf4net. If you want better profiling result management feature, you could implement the IProfilingStorage interface by yourself.

NanoProfiler also provides a wonderful view-result Web UI supports view latest profiling results in a tree-timeline view (simply visit ~/nanoprofiler/view in your web application). 

Installing NanoProfiler
-----------------------
Find NanoProfiler on nuget.org: http://nuget.org/packages?q=NanoProfiler

You will find at least 6 nuget packages:

* NanoProfiler - The core profiling feature which works for non-web applications without DB profiling support;
* NanoProfiler.Data - Depends on NanoProfiler package, provides additional DB profiling support;
* NanoProfiler.Web - Depends on NanoProfiler package, provides additional web application profiling support;
* NanoProfiler.Unity - Depends on NanoProfiler package, NanoProfiler's Unity extension for Unity IoC container based deep profiling support;
* NanoProfiler.Wcf - Depends on NanoProfiler.Web package, provides additional WCF profiling support;
* NanoProfiler.Web.Extensions - Depends on NanoProfiler.Web package, provides additional web components for log parsing, export & import;

Which packages should I add reference to my project?

* If you want to use NanoProfiler for profiling a non-web application, please at least add reference to NanoProfiler;
* If you want to use NanoProfiler for profiling a web application, please at least add reference to NanoProfiler.Web;
* If you also want to do DB profling, please also add reference to NanoProfiler.Data;
* If you want to enable Unity container based deep profiling, please also add reference to NanoProfiler.Unity;
* If you also want to do WCF client or server side profiling, please also add reference to NanoProfiler.Wcf;

How to use NanoProfiler in code?
-----------------------------------
For profiling a web application, 

**First**, you need to add code to your HttpModule or Global.asax.cs for starting a profling in BeginRequest event handler and stopping a profling in EndRequest event handler:

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
        ProfilingSession.Start(Request.Url.ToString());
    }

    protected void Application_EndRequest(object sender, EventArgs e)
    {
        ProfilingSession.Stop();
    }

**Second**, in your application code, you could add profiling steps by calling the Step() method:

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        using (ProfilingSession.Current.Step("ProcessRequestAsync"))
        {
            await ExecuteTask(context);

            // it supports to use a delegate for getting the step name
            // so that, in case profiling is disabled, there is no string concat cost
            var data = GetDataForProcessing();
            using (ProfilingSession.Current.Step(() =>BuildStepNameFromData(data)))
            {
                // do something with data
                //...
            }
        }
    }

How to set global profiling settings?
-------------------------------------

In application_start event handler, usually you might want to overwrite some of the default global profiling settings.

For example, the code below is an demo Application Start event handler, which sets some global settings of NanoProfiler:

	protected void Application_Start(object sender, EventArgs e)
	{          
		// register profiling filters to exclude some URLs from profiling
		ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("_tools/"));
		ProfilingSession.ProfilingFilters.Add(new FileExtensionProfilingFilter("jpg", "js", "css"));
	}

How to enable DB profiling?
------------------------------------
To enable DB profiling, you need to wrap your code for DB operations with the ProfiledDbConnection, ProfiledDbCommand, etc, classes provided by NanoProfiler.Data.

For most cases, you only need to wrap your IDbConnection instances with the ProfiledDbConnection class. For example, if you have a data layer DB operation below:

    public class DemoDBDataService : IDemoDBDataService
    {
        public List<DemoData> LoadActiveDemoData()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoData"))
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select Id, Name from [Table] where IsActive = @IsActive";
                        cmd.Parameters.Add(new SqlParameter("@IsActive", 1));

                        using (var reader = cmd.ExecuteReader())
                        {
                            var results = new List<DemoData>();
                            while (reader.Read())
                            {
                                results.Add(new DemoData { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                            }
                            return results;
                        }
                    }
                }
            }
        }

        private IDbConnection GetConnection()
        {
            var conn = new SqlConnection(@"Server=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\DemoDB.mdf;Database=DemoDB;Trusted_Connection=Yes;");
            return conn;
        }
    }

Your code of GetConnection() method need to be changed to as below:

    public class DemoDBDataService : IDemoDBDataService
    {
        //...

        private IDbConnection GetConnection()
        {
            var conn = new SqlConnection(@"Server=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\DemoDB.mdf;Database=DemoDB;Trusted_Connection=Yes;");

            if (ProfilingSession.Current == null)
            {
                return conn;
            }

            var dbProfiler = new DbProfiler(ProfilingSession.Current.Profiler);
            return new ProfiledDbConnection(conn, dbProfiler);
        }
    }

How to view profiling results?
---------------------------------
If you are referencing NanoProfiler.Web, the view-result UI is enabled by default, which lists the latest 100 profiling results in a wonderful tree-timeline view (simply visit ~/nanoprofiler/view in your web application).

If you want to change number of results to be listed or if you want to disable this feature in your production environment, you could add code like below in your application_start event handler to change its default settings:

    ProfilingSession.ProfilingStorage = new CircularBufferedProfilingStorage(100, profiler => false, new JsonProfilingStorage());

* The first parameter of CircularBufferedProfilingStorage specified the max number of latest profiling results to be kept;
* The second parameter is of type Func<IProfiler, bool>, which is a delegate by which you could filter specified profiling results in the view-result page;
* The third parameter is optional, which should be the underlying storage for real persisting of profiling results;

How to enable Unity container based deep profiling?
----------------------------------------------------------
Deep profiling means to automatically profile any method calls of specified types. The Unity based deep profiling feature is implemented as a Unity extension. To enable it, you only need to simply add the extension to Unity container on application start-up with one line of code below:

	// Enable deep profiling extension for any method calls on interface types containing "DemoDBService"
	Container.AddExtension(new DeepProfilingExtension(new RegexDeepProfilingFilter("DemoDBService")));

How to enable Unity container policy injection based profiling?
---------------------------------------------------------------------------
Unity container supports policy based injection, depends on which could  either do interception declaratively with .NET attributes, or intercept via XML configuration. For details of how to configure policy injection, please check: http://msdn.microsoft.com/en-us/library/ff660915(v=pandp.20).aspx

NanoProfiler.Unity provides the PolicyInjectionProfilingCallHandler class, which is the ICallHandler implementation for profiling, and the ProfiledMethodAttribute class which supports declarative mode of policy injection.

For example, if you want to enable declarative profiling on the LoadActiveDemoData() method of IDemoDBDataService interface, we could simply mark the method with the [ProfiledMethod] attribute:

    public interface IDemoDBDataService
    {
        [ProfiledMethod]
        List<DemoData> LoadActiveDemoData();
    }

And in Global.asax.cs, we need to enable policy injection with code below (Alternatively, you could also enable policy injection by using unity's XML configuration):

            // Register types to unity container to demo unity based policy injection based profiling.
            Container.RegisterType<IDemoDBDataService, DemoDBDataService>(
                new ContainerControlledLifetimeManager()
                , new InterceptionBehavior<PolicyInjectionBehavior>()); //enable policy injection

            // Enable policy injection for interface types registered with PolicyInjectionBehavior
            Container.AddNewExtension<Interception>()
                .Configure<Interception>()
                .SetDefaultInterceptorFor<IDemoDBDataService>(new InterfaceInterceptor());

How to enable WCF profiling?
---------------------------------
To enable WCF profiling, you need to add the WcfProfilingBehavior to your WCF endpoint programatically or via endpoint configuration. Below is an exampl for both WCF service endpoint and client endpoint to use WcfProfilingBehavior:

	<system.serviceModel>
	<bindings>
	  <basicHttpBinding>
		<binding name="BasicHttpBinding_IWcfDemoService" />
	  </basicHttpBinding>
	</bindings>
	<client>
	  <endpoint address="http://localhost:64511/WcfDemoService.svc"
		binding="basicHttpBinding" bindingConfiguration="BasicHttpBinding_IWcfDemoService"
		contract="DemoService.IWcfDemoService" name="BasicHttpBinding_IWcfDemoService" />
	</client>
	<behaviors>
	  <serviceBehaviors>
		<behavior name="">
		  <serviceMetadata httpGetEnabled="true" httpsGetEnabled="true" />
		  <serviceDebug includeExceptionDetailInFaults="false" />
		</behavior>
	  </serviceBehaviors>
	  <endpointBehaviors>
		<behavior>
		  <tinyprofiler />
		</behavior>
	  </endpointBehaviors>
	</behaviors>
	<serviceHostingEnvironment aspNetCompatibilityEnabled="true"
	  multipleSiteBindingsEnabled="true" />
	<extensions>
	  <behaviorExtensions>
		<add name="tinyprofiler" type="EF.Diagnostics.Profiling.ServiceModel.Configuration.WcfProfilingBehaviorElement, NanoProfiler.Wcf"/>
	  </behaviorExtensions>
	</extensions>
	</system.serviceModel>

What happens when WCF profiling is enabled is:

- For client endpoint, each WCF service call in your code will be logged as a WCF timing as one of the custom timings, similar to DB timing, in the profiling result, both the WCF action name and the WCF request message XML is logged as properties of a WCF timing;
- For service endpoint, each WCF service method call is logged as a separate profiling session;

Please check the source code of NanoProfiler.Demos.SimpleDemo in source code of NanoProfiler on github if you want a running example.

**NOTICE:**

When you have both WCF services (using HTTP bindings) and some other web/rest services deployed in the same web application, if you enables profiling on both WCF services and web/rest services globally in application_start event, if you don't have <serviceHostingEnvironment aspNetCompatibilityEnabled="true" /> set in web.config, you will get two profiling sessions be logged.

The reason is, when NOT aspNetCompatibilityEnabled="true", for a WCF service request, the WCF method execution and application_beginrequest/application_endrequest event handler execution are in different threads. So when the profiling session is being started, it could not access the profiling session which has already been started in application_beginrequest.

Tagging of profiling
---------------------
NanoProfiler supports tagging on any profiling timings. No matter ProfilingSession.Start(name, tags), Step(name, tags) or DbProfiler.ExecuteDbCommand(executeType, command, execute, tags) has an optional tags parameter, where you could add multiple tags, which will be saved as part of the profiling results. Specifically, when wcf profiling is enabled on both client side and server side, the tags of the client profiling session will be passed to server side through wcf message header and be merged with the tags of the server side wcf profiling.

The tagging feature creates the possibility to parse/wire the persisted profiling results later by tags. For example, you could save user's login session id as a tag when starting a profiling session so that you could easily find all the profiling session results related to one login session from the persisted profiling logs.

Performance of NanoProfiler
------------------------------
Performance is one of the important focus of NanoProfiler, even when working in async programming model and in multi-threading scenarios, there is almost no performance penalty when having NanoProfiler enabled. So it is OK to enable NanoProfiler even in production environment.

The magic is, unlike some of the other profiling tools (e.g. MiniProfiler), which constructs/maintains the entire profiling result tree on stepping, the philosophy of NanoProfiler is different. NanoProfiler's stepping simply stores the raw profiling results of each step, db or wcf timing into a flat list and saves all the results in an async queue worker thread.

Since it has much better performance and is designed to consume minimal server resources (memory, cpu, io, etc) in application process, we prefer to enable profiling in production environments, centralize profiling results in e.g. elasticsearch indexes, and monitor & analyze the results with tools like http://kibana.org.

How to compile the source code?
-------------------------------------
Do the following steps before opening NanoProfiler.sln in Visual Studio 2012 Pro+

- git clone
- cd src
- run .\build.cmd

License terms
-------------
NanoProfiler is published under the [MIT license](http://englishtown.mit-license.org).