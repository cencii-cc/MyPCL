using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyPCL.Utils
{
    public static class SystemUtil
    {


    }

    public class SafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// 线程安全字典
        /// </summary>
        private readonly object SyncRoot = new object();

        /// <summary>
        /// 实际字典
        /// </summary>
        private readonly Dictionary<TKey, TValue> _Dictionary = new Dictionary<TKey, TValue>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public SafeDictionary() { }

        public SafeDictionary(IEnumerable<KeyValuePair<TKey, TValue>> Data)
        {
            foreach (var item in Data)
            {
                _Dictionary.Add(item.Key, item.Value);
            }
        }

        //线程安全的方法实现

        public void Add(TKey key, TValue value)
        {
            lock (SyncRoot)
            {
                _Dictionary.Add(key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (SyncRoot)
            {
                _Dictionary.Add(item.Key, item.Value);
            }
        }

        public TValue this[TKey key] 
        { 
            get
            {
                lock (SyncRoot)
                {
                    return _Dictionary[key];
                }
            }
            set 
            {
                lock (SyncRoot) 
                {
                    _Dictionary[key] = value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock(SyncRoot)
                {
                    return new List<TKey>(_Dictionary.Keys);
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (SyncRoot)
                {
                    return new List<TValue>(_Dictionary.Values);
                }
            }
        }



        public int Count
        {
            get
            {
                lock(SyncRoot)
                {
                    return _Dictionary.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Clear()
        {
            lock (SyncRoot)
            {
                _Dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (SyncRoot)
            {
                return ((IDictionary<TKey, TValue>)_Dictionary).Contains(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (SyncRoot)
            {
                return _Dictionary.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                ((IDictionary<TKey, TValue>)_Dictionary).CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return ((IDictionary<TKey, TValue>)_Dictionary).GetEnumerator();
            }
        }

        public bool Remove(TKey key)
        {
            lock (SyncRoot)
            {
                return _Dictionary.Remove(key);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (SyncRoot)
            {
                return ((IDictionary<TKey, TValue>)_Dictionary).Remove(item);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (SyncRoot)
            {
                return _Dictionary.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
