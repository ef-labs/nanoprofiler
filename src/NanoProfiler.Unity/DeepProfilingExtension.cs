/*
    The MIT License (MIT)
    Copyright © 2014 Englishtown <opensource@englishtown.com>

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace EF.Diagnostics.Profiling.Unity
{
    /// <summary>
    /// A unity extension for injecting deep profiling to method executions filtered by types.
    /// </summary>
    public class DeepProfilingExtension : UnityContainerExtension
    {
        /// <summary>
        /// Gets the deep profiling filter.
        /// </summary>
        public IDeepProfilingFilter Filter { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="DeepProfilingExtension"/> instance.
        /// </summary>
        /// <param name="filter">
        ///     The <see cref="IDeepProfilingFilter"/>, 
        ///     If not specified, nothing will be profiled.
        /// </param>
        public DeepProfilingExtension(IDeepProfilingFilter filter)
        {
            Filter = filter;
        }

        /// <summary>
        /// Initializes the container with this extension's functionality.
        /// </summary>
        protected override void Initialize()
        {
            // If Filter not specified, nothing will be profiled
            if (Filter == null)
            {
                return;
            }

            // Register the DeepProfilingInterceptionStrategy to intercept profiling on methods filtered
            Context.Strategies.Add(new DeepProfilingInterceptionStrategy(Filter), UnityBuildStage.Setup);
        }
    }
}
