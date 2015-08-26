using EF.Diagnostics.Profiling.Storages;
using ServiceStack.Text;

namespace EF.Diagnostics.Profiling.Web.Extensions.Storages
{
    /// <summary>
    /// <see cref="JsonProfilingStorage"/> extension using ServiceStack.Text for better JSON serialization performance.
    /// </summary>
    public sealed class ServiceStackJsonProfilingStorage : JsonProfilingStorage
    {
        static ServiceStackJsonProfilingStorage()
        {
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;
        }

        protected override string Serialize(object data)
        {
            if (data == null)
            {
                return null;
            }

            return JsonSerializer.SerializeToString(data);
        }
    }
}
