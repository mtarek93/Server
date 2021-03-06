﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTools
{
    class SynchronizedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private SortedDictionary<TKey, TValue> List = new SortedDictionary<TKey, TValue>();
        private object LockObject = new object();

        public int Count
        {
            get
            {
                lock (LockObject)
                    return List.Count;
            }
        }

        public void Add(TKey Key, TValue Value)
        {
            lock (LockObject)
            {
                if (!List.ContainsKey(Key))
                    List.Add(Key, Value);
                else
                    Console.WriteLine("Entry already exists in list!");
            }
        }

        public void Remove(TKey Key)
        {
            lock (LockObject)
            {
                if (List.ContainsKey(Key))
                    List.Remove(Key);
                else
                    Console.WriteLine("Entry does not exist in list!");
            }
        }

        public bool TryGetValue(TKey Key, out TValue Value)
        {
            lock (LockObject)
                return List.TryGetValue(Key, out Value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (LockObject)
                return List.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (LockObject)
                return this.GetEnumerator();
        }

        public TValue this[TKey Key]
        {
            get
            {
                lock (LockObject)
                    return List[Key];
            }
            set
            {
                lock (LockObject)
                    List[Key] = value;
            }
        }
    }
}
