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

using System.Collections;
using System.ServiceModel;

namespace EF.Diagnostics.Profiling.ServiceModel
{
    /// <summary>
    /// A wrapper of OperationContext for implementing an HttpContext.Items like request-scope cache.
    /// </summary>
    internal sealed class WcfContext : IExtension<OperationContext>
    {
        /// <summary>
        /// The items.
        /// </summary>
        private readonly IDictionary _items;

        /// <summary>
        /// Prevents a default instance of the <see cref="WcfContext"/> class from being created.
        /// </summary>
        private WcfContext()
        {
            _items = new Hashtable();
        }

        /// <summary>
        /// Gets the current <see cref="WcfContext"/>.
        /// </summary>
        public static WcfContext Current
        {
            get
            {
                var wcfContext = OperationContext.Current;
                if (wcfContext == null)
                    return null;

                var context = wcfContext.Extensions.Find<WcfContext>();
                if (context == null)
                {
                    context = new WcfContext();
                    wcfContext.Extensions.Add(context);
                }

                return context;
            }
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IDictionary Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Attach an instance.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void Attach(OperationContext owner)
        {
        }

        /// <summary>
        /// Detach an instance.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void Detach(OperationContext owner)
        {
        }
    }
}
