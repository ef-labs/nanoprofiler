namespace EF.Diagnostics.Profiling.Timing
{
    /// <summary>
    /// The base class of any RPC style custom timing.
    /// </summary>
    public abstract class RpcTimingBase : CustomTiming
    {
        private const string RpcPrefix = "rpc_";

        /// <summary>
        /// Initializes a <see cref="RpcTimingBase"/>.
        /// </summary>
        /// <param name="profiler">The profiler.</param>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        protected RpcTimingBase(IProfiler profiler, string type, string name)
            : base(profiler, RpcPrefix + type, name)
        {
        }
    }
}
