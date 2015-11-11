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

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace EF.Diagnostics.Profiling.Unity
{
    /// <summary>
    /// The attribute to support policy injection based profiling of a method declaratively.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ProfiledMethodAttribute : HandlerAttribute
    {
        /// <summary>
        /// Creates the <see cref="ICallHandler"/> for the actual execution of the profiling.
        /// </summary>
        /// <param name="container">The <see cref="IUnityContainer"/>.</param>
        /// <returns></returns>
        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new PolicyInjectionProfilingCallHandler {Order = Order};
        }
    }
}
