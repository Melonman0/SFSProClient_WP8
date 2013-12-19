using System.Collections.Generic;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>SFS object class</summary>
     * 
     * <remarks>
     * <para><b>Version:</b><br/>
     * 1.0.0</para>
     * 
     * <para><b>Author:</b><br/>
     * Thomas Hentschel Lund<br/>
     * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
     * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
     * (c) 2008-2009 gotoAndPlay()<br/>
     *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
     * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
     * </para>
     * </remarks>
     */
    public class SFSObject
    {
        private Dictionary<object, object> map;

        /**
         * <summary>
         * SFSObject constructor
         * </summary>
         */
        public SFSObject()
        {
            map = new Dictionary<object, object>();
        }


        //::::::::::::::::::::::::::::::::::::::::::::::::
        // put data
        //::::::::::::::::::::::::::::::::::::::::::::::::

        /**
         * <summary>
         * Put generic object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="value">Value to put into SFSObject</param>
         */
        public void Put(object key, object value)
        {
            map.Add(key.ToString(), value);
        }

        /**
         * <summary>
         * Put number object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="value">Value to put into SFSObject</param>
         */
        public void PutNumber(object key, double value)
        {
            map.Add(key.ToString(), value);
        }

        /**
         * <summary>
         * Put bool object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="value">Value to put into SFSObject</param>
         */
        public void PutBool(object key, bool value)
        {
            map.Add(key.ToString(), value);
        }

        /**
         * <summary>
         * Put List object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="collection">Value to put into SFSObject</param>
         */
        public void PutList(object key, IList<object> collection)
        {
            PopulateList(new SFSObject(), key.ToString(), collection);
        }

        /**
         * <summary>
         * Put Dictionary object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="collection">Value to put into SFSObject</param>
         */
        public void PutDictionary(object key, IDictionary<object, object> collection)
        {
            PopulateDictionary(new SFSObject(), key.ToString(), collection);
        }

        private void PopulateList(SFSObject aobj, string key, IList<object> collection)
        {
            int count = 0;

            if (aobj != this)
                Put(key, aobj);

            foreach (object element in collection)
            {
                if (element is IList<object>)
                    aobj.PutList(count, (IList<object>)element);

                else if (element is IDictionary<object, object>)
                    aobj.PutDictionary(count, (IDictionary<object, object>)element);

                else
                    aobj.Put(count, element);

                ++count;
            }
        }

        private void PopulateDictionary(SFSObject aobj, string key, IDictionary<object, object> collection)
        {
            object itemKey = null;
            object itemValue = null;

            if (aobj != this)
                Put(key, aobj);

            foreach (KeyValuePair<object, object> element in collection)
            {
                itemKey = element.Key;
                itemValue = element.Value;

                if (itemValue is IList<object>)
                    aobj.PutList(itemKey, (IList<object>)itemValue);

                else if (itemValue is IDictionary<object, object>)
                    aobj.PutDictionary(itemKey, (IDictionary<object, object>)itemValue);

                else
                    aobj.Put(itemKey, itemValue);

            }
        }

        //::::::::::::::::::::::::::::::::::::::::::::::::
        // get data
        //::::::::::::::::::::::::::::::::::::::::::::::::

        /**
         * <summary>
         * Get generic object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public object Get(object key)
        {
            return map[key];
        }

        /**
         * <summary>
         * Get string object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public string GetString(object key)
        {
            return (string)map[key];
        }

        /**
         * <summary>
         * Get double object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public double GetNumber(object key)
        {
            return (double)map[key];
        }

        /**
         * <summary>
         * Get bool object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public bool GetBool(object key)
        {
            return (bool)map[key];
        }

        /**
         * <summary>
         * Get SFSObject object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public SFSObject GetObj(object key)
        {
            return (SFSObject)map[key];
        }

        /**
         * <summary>
         * Get SFSObject object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public SFSObject GetObj(int key)
        {
            return (SFSObject)map[key];
        }

        /**
         * <summary>
         * Get number of values in this SFSObject
         * </summary>
         * 
         * <returns>Number of values</returns>
         */
        public int Size()
        {
            return map.Count;
        }

        /**
         * <summary>
         * Get all keys with values in this SFSObject
         * </summary>
         * 
         * <returns>All keys</returns>
         */
        public ICollection<object> Keys()
        {
            return map.Keys;
        }

        /**
         * <summary>
         * Remove object value from SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         */
        public void Remove(object key)
        {
            map.Remove(key);
        }

    }
}
