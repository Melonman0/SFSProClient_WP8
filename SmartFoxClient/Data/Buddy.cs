using System.Collections.Generic;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>Buddy is the class representing a players buddy.<br/>
     * This class is used internally by the <see cref="SmartFoxClient"/> class; also, Buddy objects are returned by various methods and events of the SmartFoxServer API.</summary>
     * 
     * <remarks>
     * <para><b>NOTE:</b><br/>
     * in the provided examples, <c>buddy</c> always indicates a Buddy instance.</para>
     * 
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

    public class Buddy
    {
        private int id;
        private string name;
        private bool isOnline;
        private bool isBlocked;
        private Dictionary<string, object> variables;

        /**
         * <summary><see cref="Buddy(int, string, bool, bool, Dictionary&lt;string, object&gt;)"/></summary>
         */
        public Buddy(int id, string name, bool isOnline, bool isBlocked) : this(id, name, isOnline, isBlocked, new Dictionary<string, object>()) { }
        /**
         * <summary>
         * Buddy constructor.
         * </summary>
         * 
         * <param name="id">the buddy id</param>
         * <param name="name">the buddy name</param>
         * <param name="isOnline"><c>true</c> if the buddy is online</param>
         * <param name="isBlocked"><c>true</c> if the buddy is blocked</param>
         * <param name="variables">buddy variables</param>
         */
        public Buddy(int id, string name, bool isOnline, bool isBlocked, Dictionary<string, object> variables)
        {
            this.id = id;
            this.name = name;
            this.isOnline = isOnline;
            this.isBlocked = isBlocked;
            this.variables = variables;
        }
        /**
         * <summary>
         * Get the id of the buddy.
         * </summary>
         * 
         * <returns>The id of the buddy</returns>
         * 
         * <example>
         * <code>
         * 			Trace.WriteLine("Buddy id:" + buddy.GetId());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetName"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetId()
        {
            return id;
        }

        /**
         * <summary>
         * Get the name of the buddy.
         * </summary>
         * 
         * <returns>The name of the buddy</returns>
         * 
         * <example><code>
         * 			Trace.WriteLine("Buddy name:" + buddy.GetName());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetId"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public string GetName()
        {
            return name;
        }

        /**
         * <summary>
         * A boolean flag indicating if the user is online
         * </summary>
         * 
         * <returns><c>true</c> if the buddy is online</returns>
         * 
         * <example>
         * <code>
         * 			if (buddy.IsOnline())
         * 				Trace.WriteLine("Buddy is online");
         * 			</code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsOnline()
        {
            return isOnline;
        }

        /**
         * <summary>
         * A boolean flag indicating if the user is blocked
         * </summary>
         * 
         * <returns><c>true</c> if the buddy is blocked</returns>
         * 
         * <example>
         * <code>
         * 			if (buddy.IsBlocked())
         * 				Trace.WriteLine("Buddy is blocked");
         * 			</code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsBlocked()
        {
            return isBlocked;
        }

        /**
         * <summary>
         * Get the buddy variables.
         * </summary>
         * 
         * <returns>The buddy variables</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public Dictionary<string, object> GetVariables()
        {
            return variables;
        }

        /**
         * <summary>
         * Get the buddy variables.
         * </summary>
         * 
         * <returns>The buddy variable for the given key</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public object GetVariable(string key)
        {
            return variables[key];
        }

        /**
         * <summary>
         * Adds or changes a given buddy variable
         * </summary>
         * 
         * <example>
         * <code>
         * buddy.SetVariable("nickname", "Jester");
         * </code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void SetVariable(string key, object value)
        {
            variables[key] = value;
        }

        /**
         * <summary>
         * Overrides the buddy variables with a complete new set of variables
         * </summary>
         * 
         * <example>
         * <code>
         * Dictionary&lt;string, object6gt; newVariables = new Dictionary&lt;string, object&gt;();  
         * buddy.SetVariables(newVariables);
         * </code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void SetVariables(Dictionary<string, object> variables)
        {
            this.variables = variables;
        }

        /**
         * <summary>
         * Sets blocked status for buddy
         * </summary>
         * 
         * <example>
         * <code>
         * buddy.SetBlocked(true);
         * </code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void SetBlocked(bool status)
        {
            isBlocked = status;
        }
    }
}
