using System.Collections.Generic;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>The User class stores the properties of each user.<br/>
     * This class is used internally by the <see cref="SmartFoxClient"/> class; also, User objects are returned by various methods and events of the SmartFoxServer API.</summary>
     * 
     * <remarks>
     * <para><b>NOTE:</b><br/>
     * in the provided examples, <c>user</c> always indicates a User instance.</para>
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
    public class User
    {
        private int id;
        private string name;
        private Dictionary<string, object> variables;
        private bool isSpec;
        private bool isMod;
        private int pId;

        /**
         * <summary>
         * User constructor.
         * </summary>
         * 
         * <param name="id">the user id</param>
         * <param name="name">the user name</param>
         * @exclude
         */
        public User(int id, string name)
        {
            this.id = id;
            this.name = name;
            this.variables = new Dictionary<string, object>();
            this.isSpec = false;
            this.isMod = false;
        }

        /**
         * <summary>
         * Get the id of the user.
         * </summary>
         * 
         * <returns>The id of the user</returns>
         * 
         * <example>
         * <code>
         * 			Trace.WriteLine("User id:" + user.GetId());
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
            return this.id;
        }

        /**
         * <summary>
         * Get the name of the user.
         * </summary>
         * 
         * <returns>The name of the user</returns>
         * 
         * <example><code>
         * 			Trace.WriteLine("User name:" + user.GetName());
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
            return this.name;
        }

        /**
         * <summary>
         * Retrieve a User Variable.
         * </summary>
         * 
         * <param name="varName">the name of the variable to retrieve.</param>
         * 
         * <returns>The User Variable's value.</returns>
         * 
         * <example>
         * <code>
         * 			int age = (int)user.GetVariable("age");
         * </code>
         * </example>
         * 
         * <seealso cref="GetVariables"/>
         * <seealso cref="SmartFoxClient.SetUserVariables(Dictionary&lt;string, object&gt;, int)"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public object GetVariable(string varName)
        {
            return this.variables[varName];
        }

        /**
         * <summary>
         * Retrieve the list of all User Variables.
         * </summary>
         * 
         * <returns>A dictionary containing User Variables' values, where the key is the variable name.</returns>
         * 
         * <example>
         * <code>
         * 			Dictionary&lt;string, object&gt; userVars = user.getVariables();
         * 			
         * 			foreach (string v in userVars.Keys)
         * 				Trace.WriteLine("Name:" + v + " | Value:" + userVars[v]);
         * </code>
         * </example>			
         * 
         * <seealso cref="GetVariable"/>
         * <seealso cref="SmartFoxClient.SetUserVariables(Dictionary&lt;string, object&gt;, int)"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public Dictionary<string, object> GetVariables()
        {
            return this.variables;
        }

        /**
         * <summary>
         * Set the User Variables.
         * </summary>
         * 
         * <param name="o">an object containing variables' key-value pairs.</param>
         * 
         * @exclude
         */
        public void SetVariables(Dictionary<string, object> o)
        {
            /*
            * TODO: only string, number (int, uint) and boolean should be allowed
            */
            foreach (string key in o.Keys)
            {
                object v = o[key];
                if (v != null)
                    this.variables[key] = v;

                else
                    this.variables.Remove(key);
            }
        }

        /**
         * <summary>
         * Reset User Variabless.
         * </summary>
         * 
         * @exclude
         */
        internal void ClearVariables()
        {
            this.variables.Clear();
        }

        /**
         * <summary>
         * Set the <see cref="IsSpectator"/> property.
         * </summary>
         * 
         * <param name="b"><c>true</c> if the user is a spectator.</param>
         * 
         * @exclude
         */
        public void SetIsSpectator(bool b)
        {
            this.isSpec = b;
        }

        /**
         * <summary>
         * A boolean flag indicating if the user is a spectator in the current room.
         * </summary>
         * 
         * <returns><c>true</c> if the user is a spectator.</returns>
         * 
         * <example>
         * <code>
         * 			if (user.IsSpectator())
         * 				Trace.WriteLine("The user is a spectator");
         * </code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsSpectator()
        {
            return this.isSpec;
        }

        /**
         * <summary>
         * Set the <see cref="IsModerator"/> property.
         * </summary>
         * 
         * <param name="b"><c>true</c> if the user is a Moderator.</param>
         * 
         * @exclude
         */
        public void SetModerator(bool b)
        {
            this.isMod = b;
        }

        /**
         * <summary>
         * A boolean flag indicating if the user is a Moderator in the current zone.
         * </summary>
         * 
         * <returns><c>true</c> if the user is a Moderator</returns>
         * 
         * <example>
         * <code>
         * 			if (user.IsModerator())
         * 				Trace.WriteLine("The user is a Moderator");
         * </code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsModerator()
        {
            return this.isMod;
        }

        /**
         * <summary>
         * Retrieve the player id of the user.<br/>
         * The user must be a player inside a game room for this method to work properly.<br/>
         * This id is 1-based (player 1, player 2, etc.), but if the user is a spectator its value is -1.
         * </summary>
         * 
         * <returns>The current player id</returns>
         * 
         * <example><code>
         * 			Trace.WriteLine("The user's player id is " + user.GetPlayerId());
         * </code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetPlayerId()
        {
            return this.pId;
        }

        /**
         * <summary>
         * Set the playerId property.
         * </summary>
         * 
         * <param name="pid">the playerId value</param>
         * 
         * @exclude
         */
        public void SetPlayerId(int pid)
        {
            this.pId = pid;
        }
    }
}