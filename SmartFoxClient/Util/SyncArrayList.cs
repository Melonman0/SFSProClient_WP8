using System.Collections.Generic;

namespace SmartFoxClientAPI.Util
{
    /// <summary>
    /// Synchronized wrapper around the unsynchronized ArrayList class
    /// </summary>
    /// 
    /// <remarks>
    /// From here: http://www.c-sharpcorner.com/UploadFile/alexfila/ThreadSafe11222005234917PM/ThreadSafe.aspx
    /// </remarks>
    public class SyncArrayList
    {
        private List<object> me;
        private System.Object listLock = new System.Object();

        /// <summary>
        /// 
        /// </summary>
        public List<object> ToArrayList()
        {
            return me;
        }

        /// <summary>
        /// 
        /// </summary>
        public SyncArrayList()
        {
            //
            // TODO: Add constructor logic here
            //
            me = new List<object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(object item)
        {
            lock (listLock)
            {
                me.Add(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public object ObjectAt(int index)
        {
            lock (listLock)
            {
                return me[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        public void AddRange(ICollection<object> c)
        {
            lock (listLock)
            {
                me.AddRange(c);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Capacity()
        {
            lock (listLock)
            {
                return me.Capacity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            lock (listLock)
            {
                me.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(object item)
        {
            lock (listLock)
            {
                return me.Contains(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            lock (listLock)
            {
                return me.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Value"></param>
        public void Insert(int index, object Value)
        {
            lock (listLock)
            {
                me.Insert(index, Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public void Remove(object obj)
        {
            lock (listLock)
            {
                me.Remove(obj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            lock (listLock)
            {
                me.RemoveAt(index);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            lock (listLock)
            {
                me.RemoveRange(index, count);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<object>.Enumerator GetEnumerator()
        {
            lock (listLock)
            {
                return me.GetEnumerator();
            }
        }
    }
}
