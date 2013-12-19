using System.Collections.Generic;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>The Room class stores the properties of each server room.<br/>
     * This class is used internally by the <see cref="SmartFoxClient"/> class; also, Room objects are returned by various methods and events of the SmartFoxServer API.</summary>
     * 
     * <remarks>
     * <para><b>NOTE:</b><br/>
     * in the provided examples, <c>room</c> always indicates a Room instance.</para>
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
    public class Room
    {
        private int id;
        private string name;
        private int maxUsers;
        private int maxSpectators;
        private bool temp;
        private bool game;
        private bool priv;
        private bool limbo;
        private int userCount;
        private int specCount;

        private int myPlayerIndex;

        private Dictionary<int, User> userList;
        private Dictionary<string, object> variables;

        /**
         * <summary>
         * <see cref="Room(int, string, int, int, bool, bool, bool, bool, int, int)"/>
         * </summary>
         */
        public Room(int id, string name, int maxUsers, int maxSpectators, bool isTemp, bool isGame, bool isPrivate, bool isLimbo) : this(id, name, maxUsers, maxSpectators, isTemp, isGame, isPrivate, isLimbo, 0, 0) { }
        /**
         * <summary>
         * <see cref="Room(int, string, int, int, bool, bool, bool, bool, int, int)"/>
         * </summary>
         */
        public Room(int id, string name, int maxUsers, int maxSpectators, bool isTemp, bool isGame, bool isPrivate, bool isLimbo, int userCount) : this(id, name, maxUsers, maxSpectators, isTemp, isGame, isPrivate, isLimbo, userCount, 0) { }
        /**
         * <summary>
         * Room constructor.
         * </summary>
         * 
         * <param name="id">the room id</param>
         * <param name="name">the room name</param>
         * <param name="maxUsers">the maximum number of users that can join the room simultaneously</param>
         * <param name="maxSpectators">the maximum number of spectators in the room (for game rooms only)</param>
         * <param name="isTemp"><c>true</c> if the room is temporary</param>
         * <param name="isGame"><c>true</c> if the room is a "game room"</param>
         * <param name="isPrivate"><c>true</c> if the room is private (password protected)</param>
         * <param name="isLimbo"><c>true</c> if the room is a "limbo room"</param>
         * <param name="userCount"></param>
         * <param name="specCount"></param>
         */
        public Room(int id, string name, int maxUsers, int maxSpectators, bool isTemp, bool isGame, bool isPrivate, bool isLimbo, int userCount, int specCount)
        {
            this.id = id;
            this.name = name;
            this.maxSpectators = maxSpectators;
            this.maxUsers = maxUsers;
            this.temp = isTemp;
            this.game = isGame;
            this.priv = isPrivate;
            this.limbo = isLimbo;

            this.userCount = userCount;
            this.specCount = specCount;
            this.userList = new Dictionary<int, User>();
            this.variables = new Dictionary<string, object>();
        }

        /**
         * <summary>
         * Add a user to the room.
         * </summary>
         * 
         * <param name="u">the <see cref="User"/> object</param>
         * <param name="id">the user id</param>
         */
        public void AddUser(User u, int id)
        {
            userList[id] = u;

            if (this.game && u.IsSpectator())
                specCount++;
            else
                userCount++;
        }

        /**
         * <summary>
         * Remove a user from the room.
         * </summary>
         * 
         * <param name="id">the user id</param>
         */
        public void RemoveUser(int id)
        {
            User u = (User)userList[id];

            if (this.game && u.IsSpectator())
                specCount--;
            else
                userCount--;

            userList.Remove(id);
        }

        /**
         * <summary>
         * Get the list of users currently inside the room.<br/>
         * As the returned list is a Dictionary with user id(s) as keys, in order to iterate it <c>foreach</c> loop should be used.
         * </summary>
         * 
         * <returns>A list of <see cref="User"/> objects</returns>
         * 
         * <example>
         * <code>
         * Dictionary&lt;int, User&gt; users = room.GetUserList();
         * 			
         * foreach (User u in users.Values)
         *  Trace.WriteLine(u.GetName());
         * </code>
         * </example>
         * 
         * <seealso cref="GetUser"/>
         * <seealso cref="User"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public Dictionary<int, User> GetUserList()
        {
            return this.userList;
        }

        /**
         * <summary>
         * Retrieve a user currently in the room.
         * </summary>
         * 
         * <param name="userId">the user name (<c>string</c>) or the id (<c>int</c>) of the user to retrieve</param>
         * 
         * <returns>A {<see cref="User"/>} object</returns>
         * 
         * <example>
         * <code>
         * 			User user = room.GetUser("jack");
         * </code>
         * </example>
         * 
         * <seealso cref="GetUserList"/>
         * <seealso cref="User"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public User GetUser(object userId)
        {
            User user = null;

            if (userId.GetType() == typeof(int))
            {
                user = (User)userList[(int)userId];
            }

            else if (userId.GetType() == typeof(string))
            {
                foreach (User u in userList.Values)
                {
                    if (u.GetName() == (string)userId)
                    {
                        user = u;
                        break;
                    }
                }
            }

            return user;
        }

        /**
         * <summary>
         * Reset users list.
         * </summary>
         * 
         * @exclude
         */
        public void ClearUserList()
        {
            this.userList.Clear();
            this.userCount = 0;
            this.specCount = 0;
        }

        /**
         * <summary>
         * Retrieve a Room Variable.
         * </summary>
         * 
         * <param name="varName">the name of the variable to retrieve</param>
         * 
         * <returns>The Room Variable's value</returns>
         * 
         * <example>
         * <code>
         * 			string location = (string)room.GetVariable("location");
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetVariables"/>
         * <seealso cref="SmartFoxClient.SetRoomVariables(List&lt;RoomVariable&gt;, int, bool)"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public object GetVariable(string varName)
        {
            return variables[varName];
        }

        /**
         * <summary>
         * Retrieve the list of all Room Variables.
         * </summary>
         * 
         * <returns>A dictionary containing Room Variables' values, where the key is the variable name</returns>
         * 
         * <example>
         * <code>
         * 			Dictionary&lt;string, object&gt; roomVars = room.GetVariables();
         * 			
         * 			foreach (string v in roomVars.Keys)
         * 				Trace.WriteLine("Name:" + v + " | Value:" + room.GetVariable(v));
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetVariable"/>
         * <seealso cref="SmartFoxClient.SetRoomVariables(List&lt;RoomVariable&gt;, int, bool)"/>
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
         * Set the Room Variables.
         * </summary>
         * 
         * <param name="vars">a dictionary of Room Variables</param>
         * 
         * @exclude
         */
        public void SetVariables(Dictionary<string, object> vars)
        {
            this.variables = vars;
        }

        /**
         * <summary>
         * Reset Room Variables.
         * </summary>
         * 
         * @exclude
         */
        public void ClearVariables()
        {
            this.variables.Clear();
        }

        /**
         * <summary>
         * Get the name of the room.
         * </summary>
         * 
         * <returns>The name of the room</returns>
         * 
         * <example><code>
         * 			Trace.WriteLine("Room name:" + room.GetName());
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
         * Get the id of the room.
         * </summary>
         * 
         * <returns>The id of the room</returns>
         * 
         * <example>
         * <code>
         * 			Trace.WriteLine("Room id:" + room.GetId());
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
         * A boolean flag indicating if the room is dynamic/temporary.<br/>
         * This is always true for rooms created at runtime on client-side.
         * </summary>
         * 
         * <returns><c>true</c> if the room is a dynamic/temporary room</returns>
         * 
         * <example><code>
         * 			if (room.IsTemp())
         * 				Trace.WriteLine("Room is temporary");
         * 			</code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsTemp()
        {
            return this.temp;
        }

        /**
         * <summary>
         * A boolean flag indicating if the room is a "game room".
         * </summary>
         * 
         * <returns><c>true</c> if the room is a "game room"</returns>
         * 
         * <example>
         * <code>
         * 			if (room.IsGame())
         * 				Trace.WriteLine("This is a game room");
         * 			</code>
         * </example>
         * 
         * <seealso cref="IsLimbo"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsGame()
        {
            return this.game;
        }

        /**
         * <summary>
         * A boolean flag indicating if the room is private (password protected).
         * </summary>
         * 
         * <returns><c>true</c> if the room is private</returns>
         * 
         * <example>
         * <code>
         * 			if (room.IsPrivate())
         * 				Trace.WriteLine("Password required for this room");
         * 			</code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsPrivate()
        {
            return this.priv;
        }

        /**
         * <summary>
         * Retrieve the number of users currently inside the room.
         * </summary>
         * 
         * <returns>The number of users in the room</returns>
         * 
         * <example>
         * <code>
         * 			int usersNum = room.GetUserCount();
         * 			Trace.WriteLine("There are " + usersNum + " users in the room");
         * </code>
         * </example>
         * 
         * <seealso cref="GetSpectatorCount"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetUserCount()
        {
            return this.userCount;
        }

        /**
         * <summary>
         * Retrieve the number of spectators currently inside the room.
         * </summary>
         * 
         * <returns>The number of spectators in the room</returns>
         * 
         * <example>
         * <code>
         * 			int specsNum = room.GetSpectatorCount();
         * 			Trace.WriteLine("There are " + specsNum + " spectators in the room");
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetUserCount"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetSpectatorCount()
        {
            return this.specCount;
        }

        /**
         * <summary>
         * Retrieve the maximum number of users that can join the room.
         * </summary>
         * 
         * <returns>The maximum number of users that can join the room</returns>
         * 
         * <example>
         * <code>
         * 			Trace.WriteLine("Max users allowed to join the room: " + room.GetMaxUsers());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetMaxSpectators"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetMaxUsers()
        {
            return this.maxUsers;
        }

        /**
         * <summary>
         * Retrieve the maximum number of spectators that can join the room.<br/>
         * Spectators can exist in game rooms only.
         * </summary>
         * 
         * <returns>The maximum number of spectators that can join the room</returns>
         * 
         * <example>
         * <code>
         * 			if (room.IsGame())
         * 				Trace.WriteLine("Max spectators allowed to join the room: " + room.GetMaxSpectators());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetMaxUsers"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetMaxSpectators()
        {
            return this.maxSpectators;
        }


        /**
         * <summary>
         * <para>
         * Set the myPlayerId property.<br/>
         * Each room where the current client is connected contains a myPlayerId (if the room is a gameRoom).<br/>
         * myPlayerId == -1 ... user is a spectator<br/>
         * myPlayerId  > 0  ...	user is a player<br/>
         * </para>
         * </summary>
         * 
         * <param name="id">the myPlayerId value</param>
         * 
         * @exclude
         */
        public void SetMyPlayerIndex(int id)
        {
            this.myPlayerIndex = id;
        }

        /**
         * <summary>
         * Retrieve the player id for the current user in the room.<br/>
         * This id is 1-based (player 1, player 2, etc.), but if the user is a spectator its value is -1.
         * </summary>
         * 
         * <returns>The player id for the current user</returns>
         * 
         * <example>
         * <code>
         * 			if (room.IsGame())
         * 				Trace.WriteLine("My player id in this room: " + room.GetMyPlayerIndex());
         * 			</code>
         * </example>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public int GetMyPlayerIndex()
        {
            return this.myPlayerIndex;
        }

        /**
         * <summary>
         * Set the <see>isLimbo</see> property.
         * </summary>
         * 
         * <param name="b"><c>true</c>if the room is a "limbo room"</param>
         * 
         * @exclude
         */
        public void SetIsLimbo(bool b)
        {
            this.limbo = b;
        }

        /**
         * <summary>
         * A boolean flag indicating if the room is in "limbo mode".
         * </summary>
         * 
         * <returns><c>true</c> if the room is in "limbo mode"</returns>
         * 
         * <example>
         * <code>
         * 			if (room.IsLimbo())
         * 				Trace.WriteLine("This is a limbo room");
         * </code>
         * </example>
         * 
         * <seealso cref="IsGame"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsLimbo()
        {
            return this.limbo;
        }

        /**
         * <summary>
         * See the number of users in the room.
         * </summary>
         * 
         * <param name="n">the number of users</param>
         * 
         * @exclude
         */
        public void SetUserCount(int n)
        {
            this.userCount = n;
        }

        /**
         * <summary>
         * See the number of spectators in the room.
         * </summary>
         * 
         * <param name="n">the number of spectators</param>
         * 
         * @exclude
         */
        public void SetSpectatorCount(int n)
        {
            this.specCount = n;
        }
    }
}