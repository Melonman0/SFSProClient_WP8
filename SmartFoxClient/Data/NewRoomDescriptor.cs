using System.Collections.Generic;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>The NewRoomDescriptor class stores the properties of a new room for room creation methods.</summary>
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
    public class NewRoomDescriptor
    {
        private string name;
        private string password;
        private int maxUsers;
        private int maxSpectators;
        private bool isGame;
        private bool exitCurrentRoom;
        private bool receiveUCount;
        private List<RoomVariable> variables;
        private ExtensionDescriptor extension;

        /**
         * <summary>The ExtensionDescriptor class stores the properties of a extension (name and script) for room creation.</summary>
         */
        public class ExtensionDescriptor
        {
            private string name;
            private string script;

            /**
             * <summary>ExtensionDescriptor constructor.</summary>
             * 
             * <param name="name">the name used to reference the extension (see the SmartFoxServer server-side configuration).</param>
             * <param name="script">the file name of the extension script (for Actionscript and Python); if Java is used, the fully qualified name of the extension must be provided. The file name is relative to the root of the extension folder ("sfsExtensions/" for Actionscript and Python, "javaExtensions/" for Java).</param>
             */
            public ExtensionDescriptor(string name, string script)
            {
                this.name = name;
                this.script = script;
            }

            /**
             */
            public string Name
            {
                get { return name; }
            }

            /**
             */
            public string Script
            {
                get { return script; }
            }
        }

        /**
         * <summary>
         * <see cref="NewRoomDescriptor(string, int, bool, int, List&lt;RoomVariable&gt;, ExtensionDescriptor, string, bool, bool)"/>
         * </summary>
         */
        public NewRoomDescriptor(string name, int maxUsers) : this(name, maxUsers, false, 0, new List<RoomVariable>(), null, "", true, false) { }
        /**
         * <summary>
         * <see cref="NewRoomDescriptor(string, int, bool, int, List&lt;RoomVariable&gt;, ExtensionDescriptor, string, bool, bool)"/>
         * </summary>
         */
        public NewRoomDescriptor(string name, int maxUsers, bool isGame) : this(name, maxUsers, isGame, 0, new List<RoomVariable>(), null, "", true, false) { }
        /**
         * <summary>
         * <see cref="NewRoomDescriptor(string, int, bool, int, List&lt;RoomVariable&gt;, ExtensionDescriptor, string, bool, bool)"/>
         * </summary>
         */
        public NewRoomDescriptor(string name, int maxUsers, bool isGame, int maxSpectators, List<RoomVariable> variables, ExtensionDescriptor extension) : this(name, maxUsers, isGame, maxSpectators, variables, extension, "", true, false) { }
        /**
         * <summary>
         * NewRoomDescriptor constructor.
         * </summary>
         * 
         * <param name="name">the room name</param>
         * <param name="maxUsers">the maximum number of users that can join the room.</param>
         * <param name="isGame">if <c>true</c>, the room is a game room (optional, default value: <c>false</c>).</param>
         * <param name="maxSpectators">in game rooms only, the maximum number of spectators that can join the room (optional, default value: 0).</param>
         * <param name="variables">a List of Room Variables, as described in the <see cref="SmartFoxClient.SetRoomVariables(List&lt;RoomVariable&gt;, int, bool)"/> method documentation (optional, default: none).</param>
         * <param name="extension">a NewRoomDescriptor.ExtensionDescriptor, as descriped in <see cref="ExtensionDescriptor(string, string)"/> describing which extension should be dynamically attached to the room, as described farther on (optional, default: none).</param>
         * <param name="password">a password to make the room private (optional, default: none)</param>
         * <param name="exitCurrentRoom">if <c>true</c> and in case of game room, the new room is joined after creation (optional, default value: <c>true</c>).</param>
         * <param name="receiveUCount">if <c>true</c>, the new room will receive the <see cref="SFSEvent.OnUserCountChangeDelegate"/> notifications (optional, default <u>recommended</u> value: <c>false</c>).</param>
         */
        public NewRoomDescriptor(string name, int maxUsers, bool isGame, int maxSpectators, List<RoomVariable> variables, ExtensionDescriptor extension, string password, bool exitCurrentRoom, bool receiveUCount)
        {
            this.name = name;
            this.maxUsers = maxUsers;
            this.maxSpectators = maxSpectators;
            this.isGame = isGame;
            this.variables = variables;
            this.extension = extension;
            this.password = password;
            this.exitCurrentRoom = exitCurrentRoom;
            this.receiveUCount = receiveUCount;
        }

        /**
         */
        public string Name
        {
            get { return name; }
        }

        /**
         */
        public string Password
        {
            get { return password; }
        }

        /**
         */
        public int MaxUsers
        {
            get { return maxUsers; }
        }

        /**
         */
        public int MaxSpectators
        {
            get { return maxSpectators; }
        }

        /**
         */
        public bool IsGame
        {
            get { return isGame; }
        }

        /**
         */
        public bool ExitCurrentRoom
        {
            get { return exitCurrentRoom; }
        }

        /**
         */
        public bool ReceiveUCount
        {
            get { return receiveUCount; }
        }

        /**
         */
        public List<RoomVariable> Variables
        {
            get { return variables; }
        }

        /**
         */
        public ExtensionDescriptor Extension
        {
            get { return extension; }
        }
    }
}