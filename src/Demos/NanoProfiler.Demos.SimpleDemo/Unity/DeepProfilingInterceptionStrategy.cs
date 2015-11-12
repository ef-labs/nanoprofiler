/*
    The MIT License (MIT)
    Copyright © 2015 Englishtown <opensource@englishtown.com>

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

using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace NanoProfiler.Demos.SimpleDemo.Unity
{
    /// <summary>
    /// The interception strategy for deep profiling method calls of interface types.
    /// </summary>
    internal class DeepProfilingInterceptionStrategy : BuilderStrategy
    {
        private readonly IDeepProfilingFilter _filter;
        private readonly DeepProfilingInterceptionBehavior _deepProfilingBehavior = new DeepProfilingInterceptionBehavior();

        /// <summary>
        /// Initializes a <see cref="DeepProfilingInterceptionStrategy"/>.
        /// </summary>
        /// <param name="filter">The <see cref="IDeepProfilingFilter"/>.</param>
        public DeepProfilingInterceptionStrategy(IDeepProfilingFilter filter)
        {
            _filter = filter;
        }

        public override void PostBuildUp(IBuilderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!(context.Existing is IInterceptingProxy))
            {
                var typeToIntercept = context.OriginalBuildKey.Type;

                // only intercept profiling for public interface types
                if (!typeToIntercept.IsInterface || typeToIntercept.IsNotPublic)
                {
                    return;
                }

                if (_filter != null && _filter.ShouldBeProfiled(typeToIntercept))
                {
                    context.Existing = Intercept.ThroughProxy(
                        typeToIntercept, context.Existing, new InterfaceInterceptor(), new[] {_deepProfilingBehavior });
                }
            }
        }
    }
}
