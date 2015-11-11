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
using System.Data;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    internal sealed class DbParameterCollectionWrapper : DbParameterCollection
    {
        private readonly IDataParameterCollection _parameterCollection;
        private readonly DbParameterCollection _dbParameterCollection;

        public DbParameterCollectionWrapper(IDataParameterCollection parameterCollection)
        {
            _parameterCollection = parameterCollection;
            _dbParameterCollection = parameterCollection as DbParameterCollection;
        }

        #region DbParameterCollection Members

        public override int Add(object value)
        {
            return _parameterCollection.Add(value);
        }

        public override void AddRange(Array values)
        {
            if (_dbParameterCollection != null)
            {
                _dbParameterCollection.AddRange(values);
            }
            else
            {
                foreach (var value in values)
                {
                    Add(value);
                }
            }
        }

        public override void Clear()
        {
            _parameterCollection.Clear();
        }

        public override bool Contains(string value)
        {
            return _parameterCollection.Contains(value);
        }

        public override bool Contains(object value)
        {
            return _parameterCollection.Contains(value);
        }

        public override void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                return;
            }

            _parameterCollection.CopyTo(array, index);
        }

        public override int Count
        {
            get { return _parameterCollection.Count; }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return _parameterCollection.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            if (_dbParameterCollection != null && _dbParameterCollection.Contains(parameterName))
            {
                return _dbParameterCollection[parameterName];
            }

            if (_parameterCollection.Contains(parameterName))
            {
                return new DbParameterWrapper(_parameterCollection[parameterName] as IDbDataParameter);
            }

            return null;
        }

        protected override DbParameter GetParameter(int index)
        {
            if (index < 0 || index >= _parameterCollection.Count)
            {
                return null;
            }

            if (_dbParameterCollection != null)
            {
                return _dbParameterCollection[index];
            }

            return new DbParameterWrapper(_parameterCollection[index] as IDbDataParameter);
        }

        public override int IndexOf(string parameterName)
        {
            return _parameterCollection.IndexOf(parameterName);
        }

        public override int IndexOf(object value)
        {
            return _parameterCollection.IndexOf(value);
        }

        public override void Insert(int index, object value)
        {
            _parameterCollection.Insert(index, value);
        }

        public override bool IsFixedSize
        {
            get { return _parameterCollection.IsFixedSize; }
        }

        public override bool IsReadOnly
        {
            get { return _parameterCollection.IsReadOnly; }
        }

        public override bool IsSynchronized
        {
            get { return _parameterCollection.IsSynchronized; }
        }

        public override void Remove(object value)
        {
            _parameterCollection.Remove(value);
        }

        public override void RemoveAt(string parameterName)
        {
            _parameterCollection.RemoveAt(parameterName);
        }

        public override void RemoveAt(int index)
        {
            _parameterCollection.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            _parameterCollection[parameterName] = value;
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameterCollection[index] = value;
        }

        public override object SyncRoot
        {
            get { return _parameterCollection.SyncRoot; }
        }

        #endregion
    }

}
