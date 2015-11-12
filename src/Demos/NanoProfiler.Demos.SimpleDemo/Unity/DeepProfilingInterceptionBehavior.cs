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
using System.Collections.Generic;
using EF.Diagnostics.Profiling;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace NanoProfiler.Demos.SimpleDemo.Unity
{
    /// <summary>
    /// The <see cref="IInterceptionBehavior"/> implementation for deep profiling.
    /// </summary>
    internal class DeepProfilingInterceptionBehavior : IInterceptionBehavior
    {
        #region IInterceptionBehavior Members

        IEnumerable<Type> IInterceptionBehavior.GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        IMethodReturn IInterceptionBehavior.Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            if (ProfilingSession.Current != null)
            {
                var profiler = ProfilingSession.Current.Profiler;
                if (profiler != null)
                {
                    var method = input.MethodBase;
                    var targetType = input.Target == null ? method.ReflectedType : input.Target.GetType();


                    using (profiler.Step(targetType.FullName + "." + method.Name, null))
                    {
                        return getNext()(input, getNext);
                    }
                }
            }

            return getNext()(input, getNext);
        }

        bool IInterceptionBehavior.WillExecute
        {
            get { return true; }
        }

        #endregion
    }
}
