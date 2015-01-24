using System;
using System.Collections.Generic;


namespace ZXMAK2.Hardware.Adlers.Views
{
    public class DictionarySafe<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private static readonly object Lock = new object();
        private static Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        private TValue Fetch(TKey key)
        {
            lock (Lock)
            {
                TValue returnValue;
                if (_dict.TryGetValue(key, out returnValue))
                    return returnValue;

                //returnValue = "find the new value";
                _dict = new Dictionary<TKey, TValue>(_dict) { { key, returnValue } };

                return returnValue;
            }
        }

        public TValue GetValue(TKey key)
        {
            TValue returnValue;

            return _dict.TryGetValue(key, out returnValue)? returnValue : Fetch(key);
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            lock (Lock)
            {
                _dict.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (Lock)
            {
                return _dict.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get { lock (Lock) { return _dict.Keys; } }
        }

        public bool Remove(TKey key)
        {
            lock (Lock)
            {
                if( _dict.ContainsKey(key) )
                    return _dict.Remove(key);
                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (Lock)
            {
                return _dict.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get { lock (Lock) { return _dict.Values; } }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (Lock)
                {
                    TValue value;
                    if (_dict.TryGetValue(key, out value))
                        return value;
                    else
                        throw new CommandParseException("Breakpoint with ID:" + key + " not found !" );
                }
            }
            set
            {
                lock (Lock)
                {
                    _dict[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (Lock)
            {
                _dict.Add(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            lock (Lock)
            {
                _dict.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { lock (Lock) { return _dict.Count; } }
        }

        public bool IsReadOnly
        {
            get { lock (Lock) { return false; } }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (Lock)
            {
                return _dict.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
