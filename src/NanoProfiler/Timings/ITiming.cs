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

namespace EF.Diagnostics.Profiling.Timings
{
    /// <summary>
    /// Represents a generic timing.
    /// </summary>
    public interface ITiming
    {
        /// <summary>
        /// Gets or sets the type of the timing.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the identity of the timing.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identity of the parent timing.
        /// </summary>
        Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the timing.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the UTC time of when the timing is started.
        /// </summary>
        DateTime Started { get; set; }

        /// <summary>
        /// Gets or sets the start milliseconds since the start of the profling session.
        /// </summary>
        long StartMilliseconds { get; set; }
        
        /// <summary>
        /// Gets or sets the duration milliseconds of the timing.
        /// </summary>
        long DurationMilliseconds { get; set; }
        
        /// <summary>
        /// Gets or sets the tags of the timing.
        /// </summary>
        TagCollection Tags { get; set; }

        /// <summary>
        /// Gets or sets the ticks of this timing for sorting.
        /// </summary>
        long Sort { get; set; }

        /// <summary>
        /// Gets or sets addtional data of the timing.
        /// </summary>
        Dictionary<string, string> Data { get; set; }
    }
}
