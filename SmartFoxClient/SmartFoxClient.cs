using LitJson; // http://litjson.sourceforge.net/
using SmartFoxClientAPI.Data;
using SmartFoxClientAPI.Handlers;
using SmartFoxClientAPI.Http;
using SmartFoxClientAPI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace SmartFoxClientAPI
{
    /**
     * <summary>SmartFoxServer .Net/Mono client API<br/>
     * SmartFoxClient is the main class in the SmartFoxServer API.<br/>
     * This class is responsible for connecting to the server and handling all related events.</summary>
     * 
     * <remarks>
     * <para><b>NOTE:</b><br/>
     * in the provided examples, <c>smartFox</c> always indicates a SmartFoxClient instance.</para>
     * 
     * <para><b>Version:</b><br/>
     * 1.2.1</para>
     * 
     * <para><b>Author:</b><br/>
     * Thomas Hentschel Lund<br/>
     * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
     * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
     * (c) 2008,2009 gotoAndPlay()<br/>
     *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
     * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
     * </para>
     * </remarks>
     */
    public class SmartFoxClient : IDisposable
    {

        #region Constants

        // -------------------------------------------------------
        // Constants
        // -------------------------------------------------------

        private const int EOM = 0x00;
        private static string MSG_XML = "<";
        private static string MSG_JSON = "{";
        private static string MSG_STR = "%";

        private const int MIN_POLL_SPEED = 0;
        private const int DEFAULT_POLL_SPEED = 750;
        private const int MAX_POLL_SPEED = 10000;
        private const string HTTP_POLL_REQUEST = "poll";

        /**
         * <summary>
         * Moderator message type: "to user".<br/>
         * The Moderator message is sent to a single user.
         * </summary>
         * 
         * <seealso cref="SendModeratorMessage(string, string, int)"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public const string MODMSG_TO_USER = "u";

        /**
         * <summary>
         * Moderator message type: "to room".<br/>
         * The Moderator message is sent to all the users in a room.
         * </summary>
         * 
         * <seealso cref="SendModeratorMessage(string, string, int)"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public const string MODMSG_TO_ROOM = "r";

        /**
         * <summary>
         * Moderator message type: "to zone".<br/>
         * The Moderator message is sent to all the users in a zone.
         * </summary>
         * 
         * <seealso cref="SendModeratorMessage(string, string, int)"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public const string MODMSG_TO_ZONE = "z";

        /**
         * <summary>
         * Server-side extension request/response protocol: XML.
         * </summary>
         * 
         * <seealso cref="SendXtMessage(string, string, ICollection, string, int)"/>
         * <seealso cref="SFSEvent.OnExtensionResponseDelegate"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Pro
         * </remarks>
         */
        public const string XTMSG_TYPE_XML = "xml";

        /**
         * <summary>
         * Server-side extension request/response protocol: string (aka "raw protocol").
         * </summary>
         * 
         * <seealso cref="SendXtMessage(string, string, ICollection, string, int)"/>
         * <seealso cref="SFSEvent.OnExtensionResponseDelegate"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Pro
         * </remarks>
         */
        public const string XTMSG_TYPE_STR = "str";

        /**
         * <summary>
         * Server-side extension request/response protocol: JSON.
         * </summary>
         * 
         * <seealso cref="SendXtMessage(string, string, ICollection, string, int)"/>
         * <seealso cref="SFSEvent.OnExtensionResponseDelegate"/>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Pro
         * </remarks>
         */
        public const string XTMSG_TYPE_JSON = "json";

        /**
         * <summary>
         * Connection mode: "disconnected".<br/>
         * The client is currently disconnected from SmartFoxServer.
         * </summary>
         * 
         * <seealso cref="GetConnectionMode"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public const string CONNECTION_MODE_DISCONNECTED = "disconnected";

        /**
         * <summary>
         * Connection mode: "socket".<br/>
         * The client is currently connected to SmartFoxServer via socket.
         * </summary>
         * 
         * <seealso cref="GetConnectionMode"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * </remarks>
         */
        public const string CONNECTION_MODE_SOCKET = "socket";

        /**
         * <summary>
         * Connection mode: "http".<br/>
         * The client is currently connected to SmartFoxServer via http.
         * </summary>
         * 
         * <seealso cref="GetConnectionMode"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * </remarks>
         */
        public const string CONNECTION_MODE_HTTP = "http";

        #endregion

        #region Internal Properties

        // -------------------------------------------------------
        // Properties
        // -------------------------------------------------------

        //--- Socket related
        private Dictionary<string, object> messageHandlers = new Dictionary<string, object>();
        private Socket socket;
        private IPEndPoint endPoint;        
        private const int READ_BUFFER_SIZE = 4096;
        private byte[] byteBuffer = new byte[READ_BUFFER_SIZE];
        private string messageBuffer = "";
        private int socketPollSleep = 0;

        //--- Connection states
        private bool connected;
        private bool connecting = false;
        private bool autoConnectOnConfigSuccess = false;

        //--- Threads
        //private Thread thrSocketReader;
        private Thread thrConnect;
        private Thread thrHttpPoll;

        //--- Queue mode
        private List<SFSEvent> sfsQueuedEvents = new List<SFSEvent>();
        private System.Object sfsQueuedEventsLocker = new System.Object();

        //--- BlueBox
        private bool isHttpMode = false;								// connection mode
        private int _httpPollSpeed = DEFAULT_POLL_SPEED;				// bbox poll speed
        private HttpConnection httpConnection;							// the http connection

        //--- Misc
        private Dictionary<int, Room> roomList = new Dictionary<int, Room>();
        private DateTime benchStartTime;

        private SysHandler sysHandler;
        private ExtHandler extHandler;

        private int majVersion;
        private int minVersion;
        private int subVersion;

        private bool isDisposed = false;

        #endregion

        #region Server Settings

        /**
         * <summary>
         * The SmartFoxServer IP address.
         * </summary>
         * 
         * <seealso cref="Connect(string, int)"/>
         * <seealso cref="Connect(IPAddress, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public string ipAddress;

        /**
         * <summary>
         * The SmartFoxServer connection port.<br/>
         * The default port is <b>9339</b>.
         * </summary>
         * 
         * <seealso cref="Connect(string, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public int port = 9339;

        /**
         * <summary>
         * The default login zone.
         * </summary>
         * 
         * <seealso cref="LoadConfig(string, bool)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public string defaultZone;

        #endregion

        #region Queue Mode

        /**
		 * <summary>
		 * Enable/disable the queue mode for the API. Queue mode puts all network events onto a queue instead of directly calling the callbacks.<br/>
		 * The default value is <c>false</c>.
		 * </summary>
		 * 
		 * <seealso cref="ProcessEventQueue()"/>
		 * 
		 * <remarks>
		 * <para><b>Version:</b><br/>
		 * SmartFoxServer Basic / Pro</para>
		 * </remarks>
		 */
        public bool runInQueueMode = false;

        #endregion

        #region BlueBox

        //--- BlueBox settings (start) ---------------------------------------------------------------------

        /**
         * <summary>
         * The BlueBox IP address.
         * </summary>
         * 
         * <seealso cref="smartConnect"/>
         * <seealso cref="LoadConfig(string, bool)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public string blueBoxIpAddress;

        /**
         * <summary>
         * The BlueBox connection port.
         * </summary>
         * 
         * <seealso cref="smartConnect"/>
         * <seealso cref="LoadConfig(string, bool)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public int blueBoxPort = 0;

        /**
         * <summary>
         * A boolean flag indicating if the BlueBox http connection should be used in case a socket connection is not available.<br/>
         * The default value is <c>true</c>.
         * </summary>
         * 
         * <seealso cref="LoadConfig(string, bool)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public bool smartConnect = true;

        //--- BlueBox settings (end) ---------------------------------------------------------------------

        #endregion

        #region Core SFS variables/getter/setters

        /**
         * <summary>
         * An array containing the objects representing each buddy of the user's buddy list.<br/>
         * The buddy list can be iterated with a <c>foreach</c> loop, or a specific object can be retrieved by means of the <see cref="GetBuddyById"/> and <see cref="GetBuddyByName"/> methods.
         * </summary>
         * 
         * <example>
         * Each element in the buddy list is an object with the following properties:
         * <list type="table">
         * <listheader>
         * <term>term</term>
         * <description>description</description>
         * </listheader>
         * <item>
         * <term>id</term>
         * <description>(<b>int</b>) the buddy id.</description>
         * </item><item>
         * <term>name</term>
         * <description>(<b>string</b>) the buddy name.</description>
         * </item><item>
         * <term>isOnline</term>
         * <description>(<b>bool</b>) the buddy online status: <c>true</c> if the buddy is online; <c>false</c> if the buddy is offline.</description>
         * </item><item>
         * <term>isBlocked</term>
         * <description>(<b>bool</b>) the buddy block status: <c>true</c> if the buddy is blocked; <c>false</c> if the buddy is not blocked; when a buddy is blocked, SmartFoxServer does not deliver private messages from/to that user.</description>
         * </item><item>
         * <term>variables</term>
         * <description>(<b>object</b>) an object with extra properties of the buddy (Buddy Variables); see also <see cref="SetBuddyVariables"/></description>
         * </item>
         * </list>
         * 
         * The following example shows how to retrieve the properties of each buddy in the buddy list.
         * <code>
         * foreach (Buddy buddy in smartFox.buddyList)
         * {
         * 	// Trace buddy properties
         * 	Trace.WriteLine("Buddy id: " + buddy.GetId());
         * 	Trace.WriteLine("Buddy name: " + buddy.GetName());
         * 	Trace.WriteLine("Is buddy online? " + buddy.IsOnline());
         * 	Trace.WriteLine("Is buddy blocked? " + buddy.IsBlocked());
         * 				
         * 	// Trace all Buddy Variables
         * 	foreach (string v in buddy.GetVariables().Keys)
         * 		Trace.WriteLine("\t" + v + " -- " + buddy.GetVariable(v));
         *  }
         * 	</code>
         * </example>
         * 
         * <seealso cref="myBuddyVars"/>
         * <seealso cref="LoadBuddyList"/>
         * <seealso cref="GetBuddyById"/>
         * <seealso cref="GetBuddyByName"/>
         * <seealso cref="RemoveBuddy"/>
         * <seealso cref="SetBuddyBlockStatus"/>
         * <seealso cref="SetBuddyVariables"/>
         * <seealso cref="SFSEvent.OnBuddyListDelegate"/>
         * <seealso cref="SFSEvent.OnBuddyListUpdateDelegate"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * this property and all the buddy-related method are available only if the buddy list feature is enabled for the current zone. Check the SmartFoxServer server-side configuration.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic (except block status) / Pro</para>
         * <para><b>History:</b><br/>
         * SmartFoxServer Pro v1.6.0 - Buddy's <i>isBlocked</i> property added.</para>
         * </remarks>
         */
        public SyncArrayList buddyList = new SyncArrayList();

        /**
         * <summary>
         * The current user's Buddy Variables.<br/>
         * This is an associative array containing the current user's properties when he/she is present in the buddy lists of other users.<br/>
         * See the <see cref="SetBuddyVariables"/> method for more details.
         * </summary>
         * 
         * <example>The following example shows how to read the current user's own Buddy Variables.
         * 			<code>
         * 			foreach (string v in smartFox.myBuddyVars.Keys)
         * 				Trace.WriteLine("Variable " + v + " -- " + smartFox.myBuddyVars[v]);
         * 			</code>
         * </example>
         * 
         * <seealso cref="SetBuddyVariables"/>
         * <seealso cref="GetBuddyById"/>
         * <seealso cref="GetBuddyByName"/>
         * <seealso cref="SFSEvent.OnBuddyListDelegate"/>
         * <seealso cref="SFSEvent.OnBuddyListUpdateDelegate"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic (except block status) / Pro</para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * </remarks>
         */
        public Dictionary<string, object> myBuddyVars = new Dictionary<string, object>();

        /**
         * <summary>
         * Toggle the client-side debugging informations.<br/>
         * When turned on, the developer is able to inspect all server messages that are sent and received by the client in the Flash authoring environment.<br/>
         * This allows a better debugging of the interaction with the server during application developement.
         * </summary>
         * 
         * <example>The following example shows how to turn on SmartFoxServer API debugging.
         * 			<code>
         * 			SmartFoxClient smartFox = new SmartFoxClient();
         * 			bool runningLocally = true;
         * 			
         * 			string ip;
         * 			int port;
         * 			
         * 			if (runningLocally)
         * 			{
         * 				smartFox.debug = true;
         * 				ip = "127.0.0.1";
         * 				port = 9339;
         * 			}
         * 			else
         * 			{
         * 				smartFox.debug = false;
         * 				ip = "100.101.102.103";
         * 				port = 9333;
         * 			}
         * 			
         * 			smartFox.Connect(ip, port);
         * 			</code>
         * </example>
         * 
         * <seealso cref="SFSEvent.OnDebugMessageDelegate"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public bool debug;

        /**
         * <summary>
         * The current user's SmartFoxServer id.<br/>
         * The id is assigned to a user on the server-side as soon as the client connects to SmartFoxServer successfully.
         * </summary>
         * 
         * <example>The following example shows how to retrieve the user's own SmartFoxServer id.
         * 			<code>
         * 			Trace.WriteLine("My user ID is: " + smartFox.myUserId);
         * 			</code>
         * </example>
         * 
         * <seealso cref="myUserName"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * client-side, the <b>myUserId</b> property is available only after a successful login is performed using the default login procedure.<br/>
         * If a custom login process is implemented, this property must be manually set after the successful login! If not, various client-side modules (SmartFoxBits, RedBox, etc.) may not work properly.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public int myUserId;

        /**
         * <summary>
         * The current user's SmartFoxServer username.
         * </summary>
         * 
         * <example>The following example shows how to retrieve the user's own SmartFoxServer username.
         * 			<code>
         * 			Trace.WriteLine("I logged in as: " + smartFox.myUserName);
         * 			</code>
         * </example>
         * 
         * <seealso cref="myUserId"/>
         * <seealso cref="Login"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * client-side, the <b>myUserName</b> property is available only after a successful login is performed using the default login procedure.
         * If a custom login process is implemented, this property must be manually set after the successful login! If not, various client-side modules (SmartFoxBits, RedBox, etc.) may not work properly.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public string myUserName;

        /**
         * <summary>
         * The current user's id as a player in a game room.<br/>
         * The <b>playerId</b> is available only after the user successfully joined a game room. This id is 1-based (player 1, player 2, etc.), but if the user is a spectator or the room is not a game room, its value is -1.<br/>
         * When a user joins a game room, a player id (or "slot") is assigned to him/her, based on the slots available in the room at the moment in which the user entered it; for example:<br/>
         * <ul>
         * 	<li>in a game room for 2 players, the first user who joins it becomes player one (playerId = 1) and the second user becomes player two (player = 2);</li>
         * 	<li>in a game room for 4 players where only player three is missing, the next user who will join the room will be player three (playerId = 3);</li>
         * </ul>
         * </summary>
         * 
         * <example>The following example shows how to retrieve the user's own player id.
         * 			<code>
         * 			Trace.WriteLine("I'm player " + smartFox.playerId);
         * 			</code>
         * </example>
         * 
         * <seealso cref="Room.GetMyPlayerIndex"/>
         * <seealso cref="Room.IsGame"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * if multi-room join is allowed, this property contains only the last player id assigned to the user, and so it's useless.
         * In this case the <see cref="Room.GetMyPlayerIndex"/> method should be used to retrieve the player id for each joined room.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public int playerId;

        /**
         * <summary>
         * A boolean flag indicating if the user is recognized as Moderator.
         * </summary>
         * 
         * <example>The following example shows how to check if the current user is a Moderator in the current SmartFoxServer zone.
         * 			<code>
         * 			if (smartfox.amIModerator)
         * 				Trace.WriteLine("I'm a Moderator in this zone");
         * 			else
         * 				Trace.WriteLine("I'm a standard user");
         * 			</code>
         * </example>
         * 
         * <seealso cref="SendModeratorMessage(string, string, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public bool amIModerator;

        /**
         * <summary>
         * The property stores the id of the last room joined by the current user.<br/>
         * In most multiuser applications users can join one room at a time: in this case this property represents the id of the current room.<br/>
         * If multi-room join is allowed, the application should track the various id(s) in an array (for example) and this property should be ignored.
         * </summary>
         * 
         * <example>The following example shows how to retrieve the current room object (as an alternative to the <see cref="GetActiveRoom"/> method).
         * 			<code>
         * 			Room room = smartFox.GetRoom(smartFox.activeRoomId);
         * 			Trace.WriteLine("Current room is: " + room.GetName());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetActiveRoom"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public int activeRoomId;

        /**
         * <summary>
         * A boolean flag indicating if the process of joining a new room is in progress.
         * </summary>
         * 
         * @exclude
         */
        public bool changingRoom;

        /**
         * <summary>
         * The TCP port used by the embedded webserver.<br/>
         * The default port is <b>8080</b>; if the webserver is listening on a different port number, this property should be set to that value.
         * </summary>
         * 
         * <example>The following example shows how to retrieve the webserver's current http port.
         * 			<code>
         * 			Trace.WriteLine("HTTP port is: " + smartfox.httpPort);
         * 			</code>
         * </example>
         * 
         * <seealso cref="UploadFile(string, int, string, int)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.5.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public int httpPort = 8080;

        /**
         * <summary>
         * Get/set the character used as separator for the string (raw) protocol.<br/>
         * The default value is <b>%</b> (percentage character).
         * </summary>
         * 
         * <example>The following example shows how to set the raw protocol separator.
         * 			<code>
         * 			smartFox.GetRawProtocolSeparator() = "|";
         * 			</code>
         * </example>
         * 
         * <seealso cref="XTMSG_TYPE_STR"/>
         * <seealso cref="SendXtMessage(string, string, ICollection, string, int)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.5.5</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public string GetRawProtocolSeparator()
        {
            return MSG_STR;
        }

        /**
         * <summary><see cref="GetRawProtocolSeparator"/></summary>
         */
        public void SetRawProtocolSeparator(string value)
        {
            if (value != "<" && value != "{")
                MSG_STR = value;
        }

        /**
         * <summary>
         * A boolean flag indicating if the current user is connected to the server.
         * </summary>
         * 
         * <example>The following example shows how to check the connection status.
         * 			<code>
         * 			Trace.WriteLine("My connection status: " + (smartFox.IsConnected() ? "connected" : "not connected"));
         * 			</code>
         * </example>
         * 
         * <seealso cref="XTMSG_TYPE_STR"/>
         * <seealso cref="SendXtMessage(string, string, ICollection, string, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public bool IsConnected()
        {
            return this.connected;
        }

        /**
         * <summary><see cref="SetIsConnected"/></summary>
         */
        public void SetIsConnected(bool b)
        {
            this.connected = b;
        }

        /**
         * <summary>
         * The minimum interval between two polling requests when connecting to SmartFoxServer via BlueBox module.<br/>
         * The default value is 750 milliseconds. Accepted values are between 0 and 10000 milliseconds (10 seconds).
         * </summary>
         * 
         * <example>The following example shows how to set the polling speed.
         * 			<code>
         * 			    Trace.WriteLine("Poll speed: " + smartFox.GetHttpPollSpeed());
         * 			    smartFox.SetHttpPollSpeed(200);
         * 			</code>
         * </example>
         * 
         * <seealso cref="smartConnect"/>
         * <seealso cref="SendXtMessage(string, string, ICollection, string, int)"/>
         * 
         * <remarks>
         * <para><b>Usage Note:</b><br/>
         * <i>Which is the optimal value for polling speed?</i><br/>
         * 				A value between 750-1000 ms is very good for chats, turn-based games and similar kind of applications. It adds minimum lag to the client responsiveness and it keeps the server CPU usage low.<br/>
         * 				Lower values (200-500 ms) can be used where a faster responsiveness is necessary. For super fast real-time games values between 50 ms and 100 ms can be tried.<br/>
         * 				With settings &lt; 200 ms the CPU usage will grow significantly as the http connection and packet wrapping/unwrapping is more expensive than using a persistent connection.
         * 				Using values below 50 ms is not recommended.</para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public int GetHttpPollSpeed()
        {
            return this._httpPollSpeed;
        }

        /**
         * <summary><see cref="GetHttpPollSpeed"/></summary>
         */
        public void SetHttpPollSpeed(int sp)
        {
            // Acceptable values: 0 <= sp <= 10sec
            if (sp >= 0 && sp <= 10000)
                this._httpPollSpeed = sp;
        }

        #endregion

        #region Constructor / Destructor

        // -------------------------------------------------------
        // Constructor
        // -------------------------------------------------------
        /**
         * <summary><see cref="SmartFoxClient(bool)"/></summary>
         */
        public SmartFoxClient() : this(false) { }

        /**
         * <summary>The SmartFoxClient contructor.</summary>
         * 
         * <param name="debug">turn on the debug messages (optional).</param>
         * 
         * <example>The following example shows how to instantiate the SmartFoxClient class enabling the debug messages.
         * <code>
         * SmartFoxServer smartFox = new SmartFoxServer(true);
         * </code>
         * </example>
         */
        public SmartFoxClient(bool debug)
        {

            // Initialize properties 
            this.majVersion = 1;
            this.minVersion = 6;
            this.subVersion = 0;

            this.activeRoomId = -1;
            this.debug = debug;

            //initialize()

            this.messageHandlers.Clear();
            SetupMessageHandlers();

            Entities.Initialize();

            // Initialize HttpConnection
            httpConnection = new HttpConnection(this);
            httpConnection.AddEventListener(HttpEvent.onHttpConnect, HandleHttpConnect);
            httpConnection.AddEventListener(HttpEvent.onHttpClose, HandleHttpClose);
            httpConnection.AddEventListener(HttpEvent.onHttpError, HandleHttpError);
            httpConnection.AddEventListener(HttpEvent.onHttpData, HandleHttpData);
        }

        /**
         * Destructor
         */
        ~SmartFoxClient()
        {
            Dispose(false);
        }

        /**
         */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
         */
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed) // only dispose once!
            {
                if (disposing)
                {
                    // Dispose managed resources here
                }
                // perform cleanup for unmanaged resources here
                if (connected)
                {
                    Disconnect();
                }
                this.isDisposed = true;
            }
        }

        #endregion

        // -------------------------------------------------------
        // SFS Public Methods
        // -------------------------------------------------------

        #region Connection / Disconnection

        /**
         * <summary>
         * Get the current connection mode.
         * </summary>
         * 
         * <returns>The current connection mode, expressed by one of the following constants: <see cref="CONNECTION_MODE_DISCONNECTED"/> (disconnected), <see cref="CONNECTION_MODE_SOCKET"/> (socket mode), <see cref="CONNECTION_MODE_HTTP"/> (http mode).</returns>
         * 
         * <example>The following example shows how to check the current connection mode.
         * 			<code>
         * 			SFSEvent.onConnection += OnConnection;
         *						
         *			smartFox.Connect("127.0.0.1", 9339);
         *					
         *			public void OnConnection(bool success, string error)
         *			{
         *				Trace.WriteLine("Connection mode: " + smartFox.GetConnectionMode());
         *			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="CONNECTION_MODE_DISCONNECTED"/>
         * <seealso cref="CONNECTION_MODE_SOCKET"/>
         * <seealso cref="CONNECTION_MODE_HTTP"/>
         * <seealso cref="Connect(string, int)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public string GetConnectionMode()
        {
            string mode = CONNECTION_MODE_DISCONNECTED;

            if (this.IsConnected())
            {
                if (this.isHttpMode)
                    mode = CONNECTION_MODE_HTTP;
                else
                    mode = CONNECTION_MODE_SOCKET;
            }

            return mode;
        }

        /**
         * <summary><see cref="Connect(string, int)"/></summary>
         */
        public void Connect(string hostName)
        {
            Connect(hostName, 9339);
        }

        /**
         * <summary>
         * Establish a connection to SmartFoxServer.<br/>
         * The client usually gets connected to SmartFoxServer through a socket connection. In SmartFoxServer Pro, if a socket connection is not available and the <see cref="smartConnect"/> property is set to <c>true</c>, an http connection to the BlueBox module is attempted.<br/>
         * When a successful connection is established, the <see cref="GetConnectionMode"/> can be used to check the current connection mode.<br/>
         * </summary>
         * 
         * <param name="hostName">the SmartFoxServer host or ip address.</param>
         * <param name="port">the SmartFoxServer TCP port (optional)</param>
         * 
         * <example>The following example shows how to connect to SmartFoxServer.
         * 			<code>
         * 			smartFox.Connect("localhost", 9339);
         * 			</code>
         * </example>
         * 
         * <seealso cref="Disconnect"/>
         * <seealso cref="GetConnectionMode"/>
         * <seealso cref="smartConnect"/>
         * <seealso cref="SFSEvent.OnConnectionDelegate"/>
         * 
         * <remarks>
         * Important! Using this method does NOT work in Firefox due to a bug in Unity. It will crash on reconnect.<br/>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnConnectionDelegate"/></para>
         * <para><b>History:</b><br/>
         * SmartFoxServer Pro v1.6.0 - BlueBox connection attempt in case of socket connection not available.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic (except BlueBox connection) / Pro</para>
         * </remarks>
         */
        public void Connect(string hostName, int port)
        {
            DebugMessage("Trying to connect");
            if (!connected && !connecting)
            {
                try
                {
                    connecting = true;

                    Initialize();

                    this.ipAddress = hostName;
                    this.port = port;

                    thrConnect = new Thread(ConnectThread);
                    thrConnect.Start();
                }
                catch (FormatException)
                {
                    DebugMessage("API only accepts IP addresses for connections");
                    connecting = false;
                }
                catch (SocketException e)
                {
                    DebugMessage("SocketExc " + e.ToString());
                    connecting = false;
                    HandleIOError(e.Message);
                }
                catch (Exception e)
                {
                    DebugMessage("GeneralExc " + e.ToString());
                    connecting = false;
                    HandleIOError(e.Message);
                }
                connecting = false;
            }
            else
                DebugMessage("*** ALREADY CONNECTED ***");
        }              

        private void OnSocketConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                DebugMessage("Socket success");
                byte[] response = new byte[1024];
                e.SetBuffer(response, 0, response.Length);
                e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnSocketConnectCompleted);
                e.Completed += new EventHandler<SocketAsyncEventArgs>(OnSocketReceive);
                Socket socket = (Socket)e.UserToken;
                socket.ReceiveAsync(e);                

                connected = true;
                HandleSocketConnection(this, new EventArgs());
            }
            else
            {
                DebugMessage("SocketExc " + e.ToString());
                connecting = false;
                HandleIOError(e.SocketError.ToString());
            }
        }

        private void ConnectThread()
        {
            try
            {
                endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = socket;
                args.RemoteEndPoint = endPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSocketConnectCompleted);
                socket.ConnectAsync(args);

                //thrSocketReader = new Thread(HandleSocketData);
                //thrSocketReader.Start();
                //HandleSocketConnection(this, new EventArgs());
            }
            catch (SocketException e)
            {
                DebugMessage("SocketExc " + e.ToString());
                connecting = false;
                HandleIOError(e.Message);
            }
            connecting = false;
        }

        Object disconnectionLocker = new Object();

        /**
         * <summary>
         * Close the current connection to SmartFoxServer.
         * </summary>
         * 
         * <example>The following example shows how to disconnect from SmartFoxServer.
         * 			<code>
         * 			smartFox.Disconnect();
         * 			</code>
         * </example>
         * 
         * <seealso cref="Connect(string, int)"/>
         * <seealso cref="SFSEvent.OnConnectionLostDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnConnectionLostDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void Disconnect()
        {
            lock (disconnectionLocker)
            {
                if (!isHttpMode)
                {
                    if (connected)
                    {
                        try
                        {
#if !UNITY_IPHONE
                            socket.Shutdown(SocketShutdown.Both);
#else
                            socket.Close();
#endif
                        }
                        catch (Exception e)
                        {
                            DebugMessage("Disconnect Exception: " + e.ToString());
                        }
                        connected = false;
                    }
                }
                else
                {
                    try
                    {
                        if (thrHttpPoll != null && thrHttpPoll.IsAlive)
                        {
                            thrHttpPoll.Abort();
                        }
                        httpConnection.Close();
                    }
                    catch (Exception e)
                    {
                        DebugMessage("Disconnect Exception: " + e.ToString());
                    }
                    connected = false;
                }

                // dispatch event
                HandleSocketDisconnection();
            }
        }

        /**
         * <summary>
         * Automatically join the the default room (if existing) for the current zone.<br/>
         * A default room can be specified in the SmartFoxServer server-side configuration by adding the <c>autoJoin = "true"</c> attribute to one of the <c>&lt;Room&gt;></c> tags in a zone.<br/>
         * When a room is marked as <i>autoJoin</i> it becomes the default room where all clients are joined when this method is called.
         * </summary>
         * 
         * <example>The following example shows how to join the default room in the current zone.
         * 			<code>
         * 			smartFox.AutoJoin();
         * 			</code>
         * </example>
         * 
         * <seealso cref="JoinRoom(object, string, bool, bool, int)"/>
         * <seealso cref="SFSEvent.OnJoinRoomDelegate"/>
         * <seealso cref="SFSEvent.OnJoinRoomErrorDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnJoinRoomDelegate"/><br/>
         * <see cref="SFSEvent.OnJoinRoomErrorDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void AutoJoin()
        {
            if (!CheckRoomList())
            {
                return;
            }

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "autoJoin", this.activeRoomId, "");
        }

        #endregion

        #region Login/Logout

        /**
         * <summary>
         * Perform the default login procedure.<br/>
         * The standard SmartFoxServer login procedure accepts guest users. If a user logs in with an empty username, the server automatically creates a name for the client using the format <i>guest_n</i>, where <i>n</i> is a progressive number.<br/>
         * Also, the provided username and password are checked against the moderators list (see the SmartFoxServer server-side configuration) and if a user matches it, he is set as a Moderator.
         * </summary>
         * 
         * <param name="zone">the name of the zone to log into.</param>
         * <param name="name">the user name.</param>
         * <param name="pass">the user password.</param>
         * 
         * <example>The following example shows how to login into a zone.
         * 			<code>
         * 			SFSEvent.onLogin += OnLogin;
         * 			
         * 			smartFox.Login("simpleChat", "jack");
         * 			
         * 			public void OnLogin(bool success, string name, string error)
         * 			{
         * 				if (success)
         * 					Trace.WriteLine("Successfully logged in as " + name);
         * 				else
         * 					Trace.WriteLine("Zone login error; the following error occurred: " + error);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="Logout"/>
         * <seealso cref="SFSEvent.OnLoginDelegate"/>
         * 
         * <remarks>
         * <para><b>NOTE 1:</b><br/>
         * duplicate names in the same zone are not allowed.</para>
         * <para><b>NOTE 2:</b><br/>
         * for SmartFoxServer Basic, where a server-side custom login procedure can't be implemented due to the lack of <i>extensions</i> support, a custom client-side procedure can be used, for example to check usernames against a database using a php/asp page.<br/>
         * In this case, this should be done BEFORE calling the <b>login</b> method. This way, once the client is validated, the stadard login procedure can be used.</para>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnLoginDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void Login(string zone, string name, string pass)
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string message = "<login z='" + zone + "'><nick><![CDATA[" + name + "]]></nick><pword><![CDATA[" + pass + "]]></pword></login>";

            Send(header, "login", 0, message);
        }

        /**
         * <summary>
         * Log the user out of the current zone.<br/>
         * After a successful logout the user is still connected to the server, but he/she has to login again into a zone, in order to be able to interact with the server.
         * </summary>
         * 
         * <example>The following example shows how to logout from a zone.
         * 			<code>
         * 			SFSEvent.onLogout += OnLogout;
         * 			
         * 			smartFox.Logout();
         * 			
         * 			public void OnLogout()
         * 			{
         * 				Trace.WriteLine("Logged out successfully");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="Login"/>
         * <seealso cref="SFSEvent.OnLogoutDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnLogoutDelegate"/></para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.5.5</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void Logout()
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "logout", -1, "");
        }

        #endregion

        #region Load Configuration

        /**
         * <summary><see cref="LoadConfig(string, bool)"/></summary>
         */
        public void LoadConfig()
        {
            LoadConfig("config.xml", true);
        }

        /**
         * <summary><see cref="LoadConfig(string, bool)"/></summary>
         */
        public void LoadConfig(string configFile)
        {
            LoadConfig(configFile, true);
        }

        /**
         * <summary>
         * Load a client configuration file.<br/>
         * The SmartFoxClient instance can be configured through an external xml configuration file loaded at run-time.<br/>
         * By default, the <b>LoadConfig</b> method loads a file named "config.xml", placed in the same folder of the application file.<br/>
         * If the <i>autoConnect</i> parameter is set to <c>true</c>, on loading completion the <see cref="Connect(string, int)"/> method is automatically called by the API, otherwise the <see cref="SFSEvent.OnConfigLoadSuccessDelegate"/> event is dispatched.<br/>
         * In case of loading error, the <see cref="SFSEvent.OnConfigLoadFailureDelegate"/> event id fired.<br/>
         * </summary>
         * 
         * <param name="configFile">external xml configuration file name (optional).</param>
         * <param name="autoConnect">a boolean flag indicating if the connection to SmartFoxServer must be attempted upon configuration loading completion (optional).</param>
         * 
         * <example>The following example shows how to load an external configuration file.
         * 			<code>
         * 		    SFSEvent.onConfigLoadSuccess += OnConfigLoadSuccess;
         * 			SFSEvent.onConfigLoadFailure += OnConfigLoadFailure;
         * 			
         * 			smartFox.LoadConfig("testEnvironmentConfig.xml", false);
         * 			
         * 			public void OnConfigLoadSuccess()
         * 			{
         * 				Debug.WriteLine("Config file loaded, now connecting...");
         * 				smartFox.Connect(smartFox.ipAddress, smartFox.port);
         * 			}
         * 			
         * 			public void OnConfigLoadFailure(string message)
         * 			{
         * 				Debug.WriteLine("Failed loading config file: " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="ipAddress"/>
         * <seealso cref="port"/>
         * <seealso cref="defaultZone"/>
         * <seealso cref="debug"/>
         * <seealso cref="blueBoxIpAddress"/>
         * <seealso cref="blueBoxPort"/>
         * <seealso cref="httpPort"/>
         * <seealso cref="GetHttpPollSpeed"/>
         * <seealso cref="GetRawProtocolSeparator"/>
         * <seealso cref="SFSEvent.OnConfigLoadSuccessDelegate"/>
         * <seealso cref="SFSEvent.OnConfigLoadFailureDelegate"/>
         * <seealso cref="smartConnect"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnConfigLoadSuccessDelegate"/><br/>
         * <see cref="SFSEvent.OnConfigLoadFailureDelegate"/>
         * </para>
         * <para><b>NOTE:</b><br/>
         * the SmartFoxClient configuration file (client-side) should not be confused with the SmartFoxServer configuration file (server-side).</para>
         * <para><b>Usage Note:</b><br/>
         * The external xml configuration file has the following structure; ip, port and zone parameters are mandatory, all other parameters are optional.
         * 				<code>
         * 				<SmartFoxClient>
         * 					<ip>127.0.0.1</ip>
         * 					<port>9339</port>
         * 					<zone>simpleChat</zone>
         * 					<debug>true</debug>
         * 					<blueBoxIpAddress>127.0.0.1</blueBoxIpAddress>
         * 					<blueBoxPort>9339</blueBoxPort>
         * 					<smartConnect>true</smartConnect>
         * 					<httpPort>8080</httpPort>
         * 					<httpPollSpeed>750</httpPollSpeed>
         * 					<rawProtocolSeparator>%</rawProtocolSeparator>
         * 				</SmartFoxClient>
         * 				</code>
         * 				</para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public void LoadConfig(string configFile, bool autoConnect)
        {
            this.autoConnectOnConfigSuccess = autoConnect;

            System.Net.WebClient client = new WebClient();

            client.OpenReadCompleted += new OpenReadCompletedEventHandler(LoadConfigCompleted);
            client.OpenReadAsync(new Uri(configFile, UriKind.Relative));
        }

        private void LoadConfigCompleted(object sender, OpenReadCompletedEventArgs eArgs)
        {
            string configFileData;
            if (eArgs.Error == null && eArgs.Cancelled == false)
            {
                StreamReader sr = new StreamReader(eArgs.Result);
                configFileData = sr.ReadToEnd();
                sr.Close();
            }
            else
            {
                String message;
                if (eArgs.Error != null)
                {
                    message = eArgs.Error.Message;
                }
                else
                {
                    message = "Cancelled";
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("message", message);
                SFSEvent sfsEvt = new SFSEvent(SFSEvent.onConfigLoadFailureEvent, parameters);
                EnqueueEvent(sfsEvt);
                return;
            }

            XDocument xmlDoc = XDocument.Parse(configFileData);

            this.ipAddress = this.blueBoxIpAddress = xmlDoc.Element("ip").Value;
            this.port = int.Parse(xmlDoc.Element("port").Value);
            this.defaultZone = xmlDoc.Element("zone").Value;

            if (xmlDoc.Element("blueBoxIpAddress") != null)
                this.blueBoxIpAddress = xmlDoc.Element("blueBoxIpAddress").Value;

            if (xmlDoc.Element("blueBoxPort") != null)
                this.blueBoxPort = int.Parse(xmlDoc.Element("blueBoxPort").Value);

            if (xmlDoc.Element("debug") != null)
                this.debug = bool.Parse(xmlDoc.Element("debug").Value);

            if (xmlDoc.Element("smartConnect") != null)
                this.smartConnect = bool.Parse(xmlDoc.Element("smartConnect").Value);

            if (xmlDoc.Element("httpPort") != null)
                this.httpPort = int.Parse(xmlDoc.Element("httpPort").Value);

            if (xmlDoc.Element("httpPollSpeed") != null)
            {
                //SetHttpPollSpeed(int.Parse(xmlDoc.Element("httpPollSpeed").Value));
            }

            if (xmlDoc.Element("rawProtocolSeparator") != null)
                SetRawProtocolSeparator(xmlDoc.Element("rawProtocolSeparator").Value);

            if (autoConnectOnConfigSuccess)
                this.Connect(ipAddress, port);
            else
            {
                // Dispatch onConfigLoadSuccess event
                SFSEvent sfsEvt = new SFSEvent(SFSEvent.onConfigLoadSuccessEvent, null);
                EnqueueEvent(sfsEvt);
            }
        }

        #endregion

        #region Queue Mode        

        /**
		 * <summary>
		 * Called in queue mode to process the events that have been queued up for dispatching
		 * </summary>
		 * 
		 * <param/>
		 * 
		 * <example>The following example shows how to set the game in queued mode and how to process the queue.
		 * 			<code>
		 * 			smartFox = new SmartFoxClient();
		 *			smartFox.runInQueueMode = true;
		 *  			
		 *			void FixedUpdate() {
		 *				smartFox.ProcessEventQueue();
		 *			}
		 * 			</code>
		 * </example>
		 * 
		 * <seealso cref="runInQueueMode"/>
		 * 
		 * <remarks>
		 * <para>
		 * The queue mode is preferred in non-thread safe environments like the Unity game engine, or where you want precise control of when callbacks happen.
		 * </para>
		 * 
		 * <para><b>Version:</b><br/>
		 * SmartFoxServer Basic / Pro</para>
		 * </remarks>
		 */
        public void ProcessEventQueue()
        {
            if (!runInQueueMode)
            {
                return;
            }
            // We only want to lock the "real" queue for as little time as possible to not block new incomming events

            List<SFSEvent> sfsQueuedEventsClone = null;
            lock (sfsQueuedEventsLocker)
            {
                sfsQueuedEventsClone = new List<SFSEvent>(sfsQueuedEvents);
                sfsQueuedEvents.Clear();
            }
            // Now empty our copy
            while (sfsQueuedEventsClone.Count > 0)
            {
                SFSEvent evt = sfsQueuedEventsClone[0];
                sfsQueuedEventsClone.RemoveAt(0);
                _DispatchEvent(evt);
            }
        }

        /**
         * <summary>
         * Called in queue mode to process the events that have been queued up for dispatching
         * </summary>
         * 
         * <param/>
         * 
         * <example>The following example shows how to set the game in queued mode and how to process the queue.
         * 			<code>
         * 			smartFox = new SmartFoxClient();
         *			smartFox.runInQueueMode = true;
         *  			
         *			void FixedUpdate() {
         *				smartFox.ProcessEventQueue();
         *			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="runInQueueMode"/>
         * 
         * <remarks>
         * <para>
         * The queue mode is preferred in non-thread safe environments like the Unity game engine, or where you want precise control of when callbacks happen.
         * </para>
         * 
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void ProcessSingleEventInEventQueue()
        {
            if (!runInQueueMode)
            {
                return;
            }
            SFSEvent evt = null;
            // Only lock for as little time as possible
            lock (sfsQueuedEventsLocker)
            {
                if (sfsQueuedEvents.Count > 0)
                {
                    evt = sfsQueuedEvents[0];
                    sfsQueuedEvents.RemoveAt(0);
                }
            }
            if (evt != null)
            {
                _DispatchEvent(evt);
            }
        }

        /**
         * <summary>
         * Returns the number of events waiting to be processed while running in queue mode
         * </summary>
         * 
         * <param/>
         * 
         * <example>The following example shows how to query the number of events.
         * 			<code>
         *				Console.WriteLine("Waiting events in queue: " + smartFox.NumEventsInEventQueue();
         * 			</code>
         * </example>
         * 
         * <seealso cref="runInQueueMode"/>
         * <seealso cref="ProcessEventQueue"/>
         * 
         * <remarks>
         * <para>
         * Queue is always empty and thus returning 0 as count when not running in queue mode
         * </para>
         * 
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public int NumEventsInEventQueue()
        {
            // Not sure if its thread safe or not - so better put lock around
            int numEvents = 0;
            lock (sfsQueuedEventsLocker)
            {
                numEvents = sfsQueuedEvents.Count;
            }
            return numEvents;
        }

        #endregion

        #region Buddy Lists

        /**
         * <summary>
         * Add a user to the buddy list.<br/>
         * Since SmartFoxServer Pro 1.6.0, the buddy list feature can be configured to use a <i>basic</i> or <i>advanced</i> security mode (see the SmartFoxServer server-side configuration file).<br/>
         * Check the following usage notes for details on the behavior of the <b>AddBuddy</b> method in the two cases.
         * </summary>
         * 
         * <param name="buddyName">the name of the user to be added to the buddy list.</param>
         * 
         * <example>The following example shows how to add a user to the buddy list.
         * 			<code>
         * 			smartFox.AddBuddy("jack");
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="RemoveBuddy"/>
         * <seealso cref="SetBuddyBlockStatus"/>
         * <seealso cref="SFSEvent.OnBuddyListDelegate"/>
         * <seealso cref="SFSEvent.OnBuddyListErrorDelegate"/>
         * <seealso cref="SFSEvent.OnBuddyPermissionRequestDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnBuddyListDelegate"/><br/>
         * <see cref="SFSEvent.OnBuddyListErrorDelegate"/><br/>
         * <see cref="SFSEvent.OnBuddyPermissionRequestDelegate"/></para>
         * <para><b>Usage Note:</b><br/>
         * <i>Basic security mode</i>
         * 				When a buddy is added, if the buddy list is already full, the <see cref="SFSEvent.OnBuddyListErrorDelegate"/> event is fired; otherwise the buddy list is updated and the <see cref="SFSEvent.OnBuddyListDelegate"/> event is fired.
         * 				<hr />
         * 				<i>Advanced security mode</i>
         * 				If the <c>&lt;addBuddyPermission&gt;</c> parameter is set to <c>true</c> in the buddy list configuration section of a zone, before the user is actually added to the buddy list he/she must grant his/her permission.
         * 				The permission request is sent if the user is online only; the user receives the {@link SFSEvent#onBuddyPermissionRequest} event. When the permission is granted, the buddy list is updated and the <see cref="SFSEvent.OnBuddyListDelegate"/> event is fired.
         * 				If the permission is not granted (or the buddy didn't receive the permission request), the <b>addBuddy</b> method can be called again after a certain amount of time only. This time is set in the server configuration <c>&lt;permissionTimeOut&gt;</c> parameter.
         * 				Also, if the <c>&lt;mutualAddBuddy&gt;</c> parameter is set to <c>true</c>, when user A adds user B to the buddy list, he/she is automatically added to user B's buddy list.
         * 				Lastly, if the buddy list is full, the <see cref="SFSEvent.OnBuddyListErrorDelegate"/> event is fired.
         * </para>
         * <para><b>History:</b><br/>
         * SmartFoxServer Pro v1.6.0 - Buddy list's <i>advanced security mode</i> implemented.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic (except <i>advanced mode</i>) / Pro</para>
         * </remarks>
         */
        public void AddBuddy(string buddyName)
        {
            if (buddyName != myUserName && !CheckBuddyDuplicates(buddyName))
            {
                Dictionary<string, object> header = new Dictionary<string, object>();
                header.Add("t", "sys");
                string xmlMsg = "<n>" + buddyName + "</n>";
                Send(header, "addB", -1, xmlMsg);
            }
        }


        private bool CheckBuddyDuplicates(string buddyName)
        {
            // Check for buddy duplicates in the current buddy list

            bool res = false;

            foreach (Buddy buddy in buddyList)
            {
                if (buddy.GetName() == buddyName)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        /**
         * <summary>
         * Remove all users from the buddy list.<br/>
         * <b>Deprecated</b> In order to avoid conflits with the buddy list <i>advanced security mode</i> implemented since SmartFoxServer Pro 1.6.0, buddies should be removed one by one, by iterating through the buddy list.
         * </summary>
         * 
         * <example>The following example shows how to clear the buddy list.
         * 			<code>
         * 			smartFox.ClearBuddyList();
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="SFSEvent.OnBuddyListDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnBuddyListDelegate"/></para>
         * <para><b>History:</b><br/>
         * SmartFoxServer Pro v1.6.0 - Method deprecated.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void ClearBuddyList()
        {
            buddyList = new SyncArrayList();
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "clearB", -1, "");

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("list", buddyList);

            SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
            DispatchEvent(evt);
        }


        /**
         * <summary>
         * Get a buddy from the buddy list, using the buddy's username as key.<br/>
         * Refer to the <see cref="buddyList"/> property for a description of the buddy object's properties.
         * </summary>
         * 
         * <param name="buddyName">the username of the buddy.</param>
         * 
         * <returns>The buddy object.</returns>
         * 
         * <example>The following example shows how to retrieve a buddy from the buddy list.
         * 			<code>
         * 			Buddy buddy = smartFox.GetBuddyByName("jack");
         * 			
         * 			Trace.WriteLine("Buddy id: " + buddy.GetId());
         * 			Trace.WriteLine("Buddy name: " + buddy.GetName());
         * 			Trace.WriteLine("Is buddy online? " + buddy.IsOnline());
         * 			Trace.WriteLine("Is buddy blocked? " + buddy.IsBlocked());
         * 			
         * 			Trace.WriteLine("Buddy Variables:");
         * 			foreach (string v in buddy.GetVariables().Keys)
         * 				Trace.WriteLine("\t" + v + " -- " + buddy.GetVariable(v));
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="GetBuddyById"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public Buddy GetBuddyByName(string buddyName)
        {
            foreach (Buddy buddy in buddyList)
            {
                if (buddy.GetName() == buddyName)
                    return buddy;
            }

            return null;
        }

        /**
         * <summary>
         * Get a buddy from the buddy list, using the user id as key.<br/>
         * Refer to the <see cref="buddyList"/> property for a description of the buddy object's properties.
         * </summary>
         * 
         * <param name="id">the user id of the buddy.</param>
         * 
         * <returns>The buddy object.</returns>
         * 
         * <example>The following example shows how to retrieve a buddy from the buddy list.
         * 			<code>
         * 			Buddy buddy = smartFox.GetBuddyById(25);
         * 			
         * 			Trace.WriteLine("Buddy id: " + buddy.GetId());
         * 			Trace.WriteLine("Buddy name: " + buddy.GetName());
         * 			Trace.WriteLine("Is buddy online? " + buddy.IsOnline());
         * 			Trace.WriteLine("Is buddy blocked? " + buddy.IsBlocked());
         * 			
         * 			Trace.WriteLine("Buddy Variables:");
         * 			foreach (string v in buddy.GetVariables().Keys)
         * 				Trace.WriteLine("\t" + v + " -- " + buddy.GetVariable(v));
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="GetBuddyByName"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public Buddy GetBuddyById(int id)
        {
            foreach (Buddy buddy in buddyList)
            {
                if (buddy.GetId() == id)
                    return buddy;
            }

            return null;
        }

        /**
         * <summary>
         * Request the room id(s) of the room(s) where a buddy is currently located into.
         * </summary>
         * 
         * <param name="buddy">a buddy object taken from the <see cref="buddyList"/> Dictionary<string, object>.</param>
         * 
         * <example>The following example shows how to join the same room of a buddy.
         * 			<code>
         * 			SFSEvent.onBuddyRoom += OnBuddyRoom;
         * 			
         * 			Buddy buddy = smartFox.GetBuddyByName("jack");
         * 			smartFox.GetBuddyRoom(buddy);
         * 			
         * 			public void OnBuddyRoom(ArrayList idList)
         * 			{
         * 				// Reach the buddy in his room
         * 				smartFox.Join(idList[0]);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="SFSEvent.OnBuddyRoomDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnBuddyRoomDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void GetBuddyRoom(Buddy buddy)
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            header.Add("bid", buddy.GetId());

            // If buddy is active...
            if (buddy.GetId() != -1)
                Send(header, "roomB", -1, "<b id='" + buddy.GetId() + "' />");
        }


        /**
         * <summary>
         * Load the buddy list for the current user.
         * </summary>
         * 
         * <example>The following example shows how to load the current user's buddy list.
         * 			<code>
         * 			SFSEvent.onBuddyList += OnBuddyList;
         * 			
         * 			smartFox.LoadBuddyList();		
         * 
         * 			public void OnBuddyList(ArrayList buddyList)
         * 			{
         * 				foreach (Buddy buddy in buddyList)
         * 				{
         * 					Trace.WriteLine("Buddy id: " + buddy.GetId());
         * 					Trace.WriteLine("Buddy name: " + buddy.GetName());
         * 					Trace.WriteLine("Is buddy online? " + buddy.IsOnline());
         * 					Trace.WriteLine("Is buddy blocked? " + buddy.IsBlocked());
         * 					
         * 					Trace.WriteLine("Buddy Variables:")
         * 					for (string v in buddy.GetVariables().Keys)
         * 						Trace.WriteLine("\t" + v + " -- " + buddy.GetVariable(v));
         * 				}
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="SFSEvent.OnBuddyListDelegate"/>
         * <seealso cref="SFSEvent.OnBuddyListErrorDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnBuddyListDelegate"/><br/>
         * <see cref="SFSEvent.OnBuddyListErrorDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void LoadBuddyList()
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "loadB", -1, "");
        }


        /**
         * <summary>
         * Remove a buddy from the buddy list.<br/>
         * Since SmartFoxServer Pro 1.6.0, the buddy list feature can be configured to use a <i>basic</i> or <i>advanced</i> security mode (see the SmartFoxServer server-side configuration file).<br/>
         * Check the following usage notes for details on the behavior of the <b>removeBuddy</b> method in the two cases.
         * </summary>
         * 
         * <param name="buddyName">the name of the user to be removed from the buddy list.</param>
         * 
         * <example>The following example shows how to remove a user from the buddy list.
         * 			<code>
         * 			string buddyName = "jack";
         * 			smartFox.RemoveBuddy(buddyName);
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * <seealso cref="AddBuddy"/>
         * <seealso cref="SFSEvent.OnBuddyListDelegate"/>
         * 
         * <remarks>
         * <para><b>Usage Note:</b><br/>
         * <i>Basic security mode</i><br/>
         * 				When a buddy is removed, the buddy list is updated and the {@link SFSEvent#onBuddyList} event is fired.<br/>
         * 				<hr /><br/>
         * 				<i>Advanced security mode</i><br/>
         * 				In addition to the basic behavior, if the <c>&lt;mutualRemoveBuddy&gt;</c> server-side configuration parameter is set to {@code true}, when user A removes user B from the buddy list, he/she is automatically removed from user B's buddy list.</para>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnBuddyListDelegate"/></para>
         * <para><b>History:</b><br/>
         * SmartFoxServer Pro v1.6.0 - Buddy list's <i>advanced security mode</i> implemented.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic (except <i>advanced mode</i>) / Pro</para>
         * </remarks>
         */
        public void RemoveBuddy(string buddyName)
        {
            bool found = false;

            foreach (Buddy buddy in buddyList)
            {
                if (buddy.GetName() == buddyName)
                {
                    buddyList.Remove(buddy);
                    found = true;
                    break;
                }
            }

            if (found)
            {
                Dictionary<string, object> header = new Dictionary<string, object>();
                header.Add("t", "sys");
                string xmlMsg = "<n>" + buddyName + "</n>";

                Send(header, "remB", -1, xmlMsg);

                // Fire event!
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("list", buddyList);

                SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
                DispatchEvent(evt);
            }
        }


        /**
         * <summary>
         * Grant current user permission to be added to a buddy list.<br/>
         * If the SmartFoxServer Pro 1.6.0 <i>advanced</i> security mode is used (see the SmartFoxServer server-side configuration), when a user wants to add a buddy to his/her buddy list, a permission request is sent to the buddy.<br/>
         * Once the <see cref="SFSEvent.OnBuddyPermissionRequestDelegate"/> event is received, this method must be used by the buddy to grant or refuse permission. When the permission is granted, the requester's buddy list is updated.
         * </summary>
         * 
         * <param name="allowBuddy"><c>true</c> to grant permission, <c>false</c> to refuse to be added to the requester's buddy list.</param>
         * <param name="targetBuddy">the username of the requester.</param>
         * 
         * <example>The following example shows how to grant permission to be added to a buddy list once request is received.
         * 			<code>
         * 			SFSEvent.onBuddyPermissionRequest += OnBuddyPermissionRequest;
         * 			
         * 			bool autoGrantPermission = true;
         * 			
         * 			public void OnBuddyPermissionRequest(string sender, string message)
         * 			{
         * 				if (autoGrantPermission)
         * 				{
         * 					// Automatically grant permission
         * 					
         * 					smartFox.SendBuddyPermissionResponse(true, sender);
         * 				}
         * 				else
         * 				{
         * 					// Display a popup containing grant/refuse buttons
         * 					// TODO - make popup
         * 				}
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="AddBuddy"/>
         * <seealso cref="SFSEvent.OnBuddyPermissionRequestDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnBuddyPermissionRequestDelegate"/></para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public void SendBuddyPermissionResponse(bool allowBuddy, string targetBuddy)
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<n res='" + (allowBuddy ? "g" : "r") + "'>" + targetBuddy + "</n>";

            Send(header, "bPrm", -1, xmlMsg);
        }

        /**
         * <summary>
         * Block or unblock a user in the buddy list.<br/>
         * When a buddy is blocked, SmartFoxServer does not deliver private messages from/to that user.
         * </summary>
         * 
         * <param name="buddyName">the name of the buddy to be blocked or unblocked.</param>
         * <param name="status"><c>true</c> to block the buddy, <c>false</c> to unblock the buddy.</param>
         * 
         * <example>The following example shows how to block a user from the buddy list.
         * 			<code>
         * 			smartFox.SetBuddyBlockStatus("jack", true);
         * 			</code>
         * </example>
         * 
         * <seealso cref="buddyList"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public void SetBuddyBlockStatus(string buddyName, bool status)
        {
            Buddy b = GetBuddyByName(buddyName);

            if (b != null)
            {
                if (b.IsBlocked() != status)
                {
                    b.SetBlocked(status);

                    Dictionary<string, object> header = new Dictionary<string, object>();
                    header.Add("t", "sys");
                    string xmlMsg = "<n x='" + (status ? "1" : "0") + "'>" + buddyName + "</n>";
                    Send(header, "setB", -1, xmlMsg);

                    // Fire internal update
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("buddy", b);

                    SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListUpdateEvent, parameters);
                    DispatchEvent(evt);

                }
            }
        }

        #endregion

        #region Buddy Variables

        /**
		 * <summary>
		 * Set the current user's Buddy Variables.<br/>
		 * This method allows to set a number of properties of the current user as buddy of other users; in other words these variables will be received by the other users who have the current user as a buddy.<br/>
		 * <br/>
		 * Buddy Variables are the best way to share user's informations with all the other users having him/her in their buddy list.: for example the nickname, the current audio track the user is listening to, etc. The most typical usage is to set a variable containing the current user status, like "available", "occupied", "away", "invisible", etc.).
		 * </summary>
		 * 
		 * <param name="varList">a Dictionary<string, object>, where the key is the name of the variable and the value is the variable's value. Buddy Variables should all be strings. If you need to use other data types you should apply the appropriate type casts.</param>
		 * 
		 * <example>The following example shows how to set three variables containing the user's status, the current audio track the user listening to and the user's rank. The last one is an offline variable.
		 * 			<code>
		 * 			Dictionary<string, object> bVars = new Dictionary<string, object>();
		 * 			bVars.Add("status", "away");
		 * 			bVars.Add("track", "One Of These Days");
		 * 			bVars.Add("$rank", "guru");
		 * 			
		 * 			smartFox.SetBuddyVariables(bVars);
		 * 			</code>
		 * </example>
		 * 
		 * <seealso cref="myBuddyVars"/>
		 * <seealso cref="SFSEvent.OnBuddyListUpdateDelegate"/>
		 * 
		 * <remarks>
		 * <para><b>NOTE:</b><br/>
		 * before the release of SmartFoxServer Pro v1.6.0, Buddy Variables could not be stored, and existed during the user session only. SmartFoxServer Pro v1.6.0 introduced the ability to persist (store) all Buddy Variables and the possibility to save "offline Buddy Variables" (see the following usage notes).</para>
		 * <para><b>Usage Note:</b><br/>
		 * Let's assume that three users (A, B and C) use an "istant messenger"-like application, and user A is part of the buddy lists of users B and C.<br/>
		 * 				If user A sets his own variables (using the <see cref="SetBuddyVariables"/> method), the <see cref="myBuddyVars"/> array on his client gets populated and a <see cref="SFSEvent.OnBuddyListUpdateDelegate"/> event is dispatched to users B and C.<br/>
		 * 				User B and C can then read those variables in their own buddy lists by means of the <b>variables</b> property on the buddy object (which can be retrieved from the <see cref="buddyList"/> array by means of the <see cref="GetBuddyById"/> or <see cref="GetBuddyByName"/> methods).<br/>
		 * 				<hr />
		 * 				If the buddy list's <i>advanced security mode</i> is used (see the SmartFoxServer server-side configuration), Buddy Variables persistence is enabled: in this way regular variables are saved when a user goes offline and they are restored (and dispatched to the other users) when their owner comes back online.<br/>
		 * 				Also, setting the <c>&lt;offLineBuddyVariables&gt;</c> parameter to <c>true</c>, offline variables can be used: this kind of Buddy Variables is loaded regardless the buddy is online or not, providing further informations for each entry in the buddy list. A typical usage for offline variables is to define a buddy image or additional informations such as country, email, rank, etc.<br/>
		 * 				To creare an offline Buddy Variable, the "$" character must be placed before the variable name.</para>
		 * <para><b>Sends:</b><br/>
		 * <see cref="SFSEvent.OnBuddyListUpdateDelegate"/></para>
		 * <para><b>History:</b><br/>
		 * SmartFoxServer Pro v1.6.0 - Buddy list's <i>advanced security mode</i> implemented (persistent and offline Buddy Variables).</para>
		 * <para><b>Version:</b><br/>
		 * SmartFoxServer Basic (except <i>advanced mode</i>) / Pro</para>
		 * </remarks>
		 */
        public void SetBuddyVariables(Dictionary<string, object> varList)
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");

            // Encapsulate Variables
            string xmlMsg = "<vars>";

            // Reference to the user setting the variables
            foreach (string vName in varList.Keys)
            {
                string vValue = (string)varList[vName];

                // if variable is new or updated send it and update locally
                if ((string)myBuddyVars[vName] != vValue)
                {
                    myBuddyVars[vName] = vValue;
                    xmlMsg += "<var n='" + vName + "'><![CDATA[" + vValue + "]]></var>";
                }
            }

            xmlMsg += "</vars>";

            this.Send(header, "setBvars", -1, xmlMsg);
        }

        #endregion

        #region Rooms        

        /**
         * <summary>
         * Dynamically create a new room in the current zone.
         * </summary>
         * 
         * <param name="roomObj">a NewRoomDescriptor object with the properties described farther on.</param>
         * <param name="roomId">the id of the room from where the request is originated, in case the application allows multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>
         * The following example shows how to create a new room.
         * 			<code>
         * 			Dictionary<string, object> roomObj = new Dictionary<string, object>();
         * 			roomObj.Add("name", "The Cave");
         * 			roomObj.Add("isGame", true);
         * 			roomObj.Add("maxUsers", 15);
         * 			
         * 			ArrayList variables = new ArrayList();
         * 			variables.Add( new RoomVariable("ogres", 5, true, false) );
         * 			variables.Add( new RoomVariable("skeletons", 4) );
         * 			
         * 			roomObj.Add("vars", variables);
         * 			
         * 			smartFox.CreateRoom(roomObj);
         * 			</code>
         * </example>
         * 
         * <seealso cref="SFSEvent.OnRoomAddedDelegate"/>
         * <seealso cref="SFSEvent.OnCreateRoomErrorDelegate"/>
         * <seealso cref="SFSEvent.OnUserCountChangeDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnRoomAddedDelegate"/><br/>
         * <see cref="SFSEvent.OnCreateRoomErrorDelegate"/></para>
         * <para><b>NOTE:</b><br/>
         * if the newly created room is a game room, the user is joined automatically upon successful room creation.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void CreateRoom(NewRoomDescriptor roomObj, int roomId)
        {

            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string isGame = roomObj.IsGame ? "1" : "0";

            string exitCurrentRoom = roomObj.ExitCurrentRoom ? "1" : "0";

            string xmlMsg = "<room tmp='1' gam='" + isGame + "' spec='" + Convert.ToString(roomObj.MaxSpectators) + "' exit='" + exitCurrentRoom + "'>";

            xmlMsg += "<name><![CDATA[" + (roomObj.Name) + "]]></name>";
            xmlMsg += "<pwd><![CDATA[" + (roomObj.Password) + "]]></pwd>";
            xmlMsg += "<max>" + Convert.ToString(roomObj.MaxUsers) + "</max>";

            xmlMsg += "<uCnt>" + (roomObj.ReceiveUCount ? "1" : "0") + "</uCnt>";

            // Set extension for room
            if (roomObj.Extension != null)
            {
                xmlMsg += "<xt n='" + roomObj.Extension.Name;
                xmlMsg += "' s='" + roomObj.Extension.Script + "' />";
            }

            // Set Room Variables on creation
            if (roomObj.Variables.Count == 0)
                xmlMsg += "<vars></vars>";
            else
            {
                xmlMsg += "<vars>";

                foreach (RoomVariable rv in roomObj.Variables)
                {
                    xmlMsg += GetXmlRoomVariable(rv);
                }

                xmlMsg += "</vars>";
            }

            xmlMsg += "</room>";

            Send(header, "createRoom", roomId, xmlMsg);
        }


        /**
         * <summary><see cref="CreateRoom(Dictionary<string, object>, int)"/></summary>
         */
        public void CreateRoom(NewRoomDescriptor roomObj)
        {
            CreateRoom(roomObj, -1);
        }        

        /**
         * <summary>
         * Get the list of rooms in the current zone.<br/>
         * Unlike the <see cref="GetRoomList"/> method, this method returns the list of <see cref="Room"/> objects already stored on the client, so no request is sent to the server.
         * </summary>
         * 
         * <returns>The list of rooms available in the current zone.</returns>
         * 
         * <example>The following example shows how to retrieve the room list.
         * 			<code>
         * 			Dictionary<string, object> rooms = smartFox.GetAllRooms();
         * 			
         * 			foreach (Room room in rooms.Values)
         * 			{
         * 				Trace.WriteLine("Room: " + room.getName());
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetRoomList"/>
         * <seealso cref="Room"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public Dictionary<int, Room> GetAllRooms()
        {
            return roomList;
        }

        /**
         * @private
         */
        public void SetRoomList(Dictionary<int, Room> roomList)
        {
            this.roomList = roomList;
        }


        /**
         * <summary>
         * Get a <see cref="Room"/> object, using its id as key.
         * </summary>
         * 
         * <param name="roomId">the id of the room.</param>
         * 
         * <returns>The <see cref="Room"/> object.</returns>
         * 
         * <example>The following example shows how to retrieve a room from its id.
         * 			<code>
         * 			Room roomObj = smartFox.GetRoom(15);
         * 			Trace.WriteLine("Room name: " + roomObj.GetName() + ", max users: " + roomObj.GetMaxUsers());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetRoomByName"/>
         * <seealso cref="GetAllRooms"/>
         * <seealso cref="GetRoomList"/>
         * <seealso cref="Room"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public Room GetRoom(int roomId)
        {
            if (!CheckRoomList())
            {
                return null;
            }

            return (Room)roomList[roomId];
        }

        /**
         * <summary>
         * Get a <see cref="Room"/> object, using its name as key.
         * </summary>
         * 
         * <param name="roomName">the name of the room.</param>
         * 
         * <returns>The <see cref="Room"/> object.</returns>
         * 
         * <example>The following example shows how to retrieve a room from its name.
         * 			<code>
         * 			Room roomObj = smartFox.GetRoomByName("The Entrance");
         * 			Trace.WriteLine("Room name: " + roomObj.GetName() + ", max users: " + roomObj.GetMaxUsers());
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetRoom"/>
         * <seealso cref="GetAllRooms"/>
         * <seealso cref="GetRoomList"/>
         * <seealso cref="Room"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public Room GetRoomByName(string roomName)
        {
            if (!CheckRoomList())
            {
                return null;
            }

            Room room = null;

            foreach (Room r in roomList.Values)
            {
                if (r.GetName() == roomName)
                {
                    room = r;
                    break;
                }
            }

            return room;
        }

        /**
         * <summary>
         * Retrieve the updated list of rooms in the current zone.<br/>
         * Unlike the <see cref="GetAllRooms"/> method, this method sends a request to the server, which then sends back the complete list of rooms with all their properties and server-side variables (Room Variables).<br/>
         * 
         * If the default login mechanism provided by SmartFoxServer is used, then the updated list of rooms is received right after a successful login, without the need to call this method.<br/>
         * Instead, if a custom login handler is implemented, the room list must be manually requested to the server using this method.<br/>
         * </summary>
         * 
         * <example>The following example shows how to retrieve the room list from the server.
         * 			<code>
         * 			SFSEvent.onRoomListUpdate += OnRoomListUpdate;
         * 			
         * 			smartFox.GetRoomList()
         * 			
         * 			public void OnRoomListUpdate(Dictionary<string, object> roomList)
         * 			{
         * 				// Dump the names of the available rooms in the current zone
         * 				foreach (Room room in roomList.Values)
         * 					Trace.WriteLine(room.GetName())
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetRoom"/>
         * <seealso cref="GetRoomByName"/>
         * <seealso cref="GetAllRooms"/>
         * <seealso cref="SFSEvent.OnRoomListUpdateDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnRoomListUpdateDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void GetRoomList()
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "getRmList", activeRoomId, "");
        }

        /**
         * <summary>
         * Get the currently active {@link Room} object.<br/>
         * SmartFoxServer allows users to join two or more rooms at the same time (multi-room join). If this feature is used, then this method is useless and the application should track the various room id(s) manually, for example by keeping them in an array.
         * </summary>
         * 
         * <returns>the <see cref="Room"/> object of the currently active room; if the user joined more than one room, the last joined room is returned.</returns>
         * 
         * <example>The following example shows how to retrieve the current room object.
         * 			<code>
         * 			Room room = smartFox.GetActiveRoom();
         * 			Trace.WriteLine("Current room is: " + room.GetName());
         * 			</code>
         * </example>
         * 
         * <seealso cref="activeRoomId"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public Room GetActiveRoom()
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return null;
            }

            return (Room)roomList[activeRoomId];
        }


        /**
         * <summary><see cref="JoinRoom(object, string, bool, bool, int)"/></summary>
         */
        public void JoinRoom(object newRoom)
        {
            JoinRoom(newRoom, "", false, false, -1);
        }

        /**
         * <summary><see cref="JoinRoom(object, string, bool, bool, int)"/></summary>
         */
        public void JoinRoom(object newRoom, string pword)
        {
            JoinRoom(newRoom, pword, false, false, -1);
        }

        /**
         * <summary><see cref="JoinRoom(object, string, bool, bool, int)"/></summary>
         */
        public void JoinRoom(object newRoom, string pword, bool isSpectator)
        {
            JoinRoom(newRoom, pword, isSpectator, false, -1);
        }

        /**
         * <summary><see cref="JoinRoom(object, string, bool, bool, int)"/></summary>
         */
        public void JoinRoom(object newRoom, string pword, bool isSpectator, bool dontLeave)
        {
            JoinRoom(newRoom, pword, isSpectator, dontLeave, -1);
        }

        /**
         * <summary>
         * Join a room.
         * </summary>
         * 
         * <param name="newRoom">the name (<c>string</c>) or the id (<c>int</c>) of the room to join.</param>
         * <param name="pword">the room's password, if it's a private room (optional).</param>
         * <param name="isSpectator">a boolean flag indicating wheter you join as a spectator or not (optional).</param>
         * <param name="dontLeave">a boolean flag indicating if the current room must be left after successfully joining the new room (optional).</param>
         * <param name="oldRoom">the id of the room to leave (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>In the following example the user requests to join a room with id = 10; by default SmartFoxServer will disconnect him from the previous room.
         * 			<code>
         * 			smartFox.JoinRoom(10);
         * 			</code>
         * 			<hr />
         * 			
         * 			In the following example the user requests to join a room with id = 12 and password = "mypassword"; by default SmartFoxServer will disconnect him from the previous room.
         * 			<code>
         * 			smartFox.JoinRoom(12, "mypassword");
         * 			</code>
         * 			<hr />
         * 			
         * 			In the following example the user requests to join the room with id = 15 and passes <c>true</c> to the <i>dontLeave</i> flag; this will join the user in the new room while keeping him in the old room as well.
         * 			<code>
         * 			smartFox.JoinRoom(15, "", false, true);
         * 			</code>
         * 
         * </example>
         * 
         * <seealso cref="SFSEvent.OnJoinRoomDelegate"/>
         * <seealso cref="SFSEvent.OnJoinRoomErrorDelegate"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * the last two optional parameters enable the advanced multi-room join feature of SmartFoxServer, which allows a user to join two or more rooms at the same time. If this feature is not required, the parameters can be omitted.</para>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnJoinRoomDelegate"/><br/>
         * <see cref="SFSEvent.OnJoinRoomErrorDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void JoinRoom(object newRoom, string pword, bool isSpectator, bool dontLeave, int oldRoom)
        {
            if (!CheckRoomList())
            {
                return;
            }

            int newRoomId = -1;
            int isSpec = isSpectator ? 1 : 0;

            if (!this.changingRoom)
            {
                if (newRoom.GetType() == typeof(int))
                    newRoomId = (int)newRoom;

                else if (newRoom.GetType() == typeof(string))
                {
                    // Search the room
                    foreach (Room r in roomList.Values)
                    {
                        if (r.GetName() == (string)newRoom)
                        {
                            newRoomId = r.GetId();
                            break;
                        }
                    }
                }

                if (newRoomId != -1)
                {
                    Dictionary<string, object> header = new Dictionary<string, object>();
                    header.Add("t", "sys");

                    string leaveCurrRoom = dontLeave ? "0" : "1";

                    // Set the room to leave
                    int roomToLeave = oldRoom > -1 ? oldRoom : activeRoomId;

                    // CHECK: activeRoomId == -1 no room has already been entered
                    if (activeRoomId == -1)
                    {
                        leaveCurrRoom = "0";
                        roomToLeave = -1;
                    }

                    string message = "<room id='" + newRoomId + "' pwd='" + pword + "' spec='" + isSpec + "' leave='" + leaveCurrRoom + "' old='" + roomToLeave + "' />";

                    Send(header, "joinRoom", activeRoomId, message);
                    changingRoom = true;
                }
                else
                {
                    DebugMessage("SmartFoxError: requested room to join does not exist!");
                }
            }
        }

        /**
         * <summary>
         * Disconnect the user from the given room.<br/>
         * This method should be used only when users are allowed to be present in more than one room at the same time (multi-room join feature).
         * </summary>
         * 
         * <param name="roomId">the id of the room to leave.</param>
         * 
         * <example>The following example shows how to make a user leave a room.
         * 			<code>
         * 			smartFox.LeaveRoom(15);
         * 			</code>
         * </example>
         * 
         * <seealso cref="JoinRoom(object, string, bool, bool, int)"/>
         * <seealso cref="SFSEvent.OnRoomLeftDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnRoomLeftDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void LeaveRoom(int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<rm id='" + roomId + "' />";

            Send(header, "leaveRoom", roomId, xmlMsg);
        }

        internal void ClearRoomList()
        {
            this.roomList.Clear();
        }

        #endregion

        #region Room Variables

        /**
         * <summary><see cref="SetRoomVariables(ArrayList, int, bool)"/></summary>
         */
        public void SetRoomVariables(List<RoomVariable> varList)
        {
            SetRoomVariables(varList, -1, true);
        }

        /**
         * <summary><see cref="SetRoomVariables(ArrayList, int, bool)"/></summary>
         */
        public void SetRoomVariables(List<RoomVariable> varList, int roomId)
        {
            SetRoomVariables(varList, roomId, true);
        }

        /**
         * <summary>
         * Set one or more Room Variables.<br/>
         * Room Variables are a useful feature to share data across the clients, keeping it in a centralized place on the server. When a user sets/updates/deletes one or more Room Variables, all the other users in the same room are notified. <br/>
         * Allowed data types for Room Variables are Numbers, Strings and Booleans; in order save bandwidth, Arrays and Objects are not supported. Nevertheless, an array of values can be simulated, for example, by using an index in front of the name of each variable (check one of the following examples).<br/>
         * If a Room Variable is set to <c>null</c>, it is deleted from the server.
         * </summary>
         * 
         * <param name="varList">an array of objects with the properties described farther on.</param>
         * <param name="roomId">the id of the room where the variables should be set, in case of molti-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * <param name="setOwnership"><c>false</c> to prevent the Room Variable change ownership when its value is modified by another user (optional).</param>
         * 
         * <example>
         * Each Room Variable is an object containing the following properties:
         * <list type="table">
         * <listheader>
         * <term>term</term>
         * <description>description</description>
         * </listheader>
         * <item>
         * <term>name</term>
         * <description>(<b>string</b>) the variable name.</description>
         * </item><item>
         * <term>value</term>
         * <description>(<b>*</b>) the variable value.</description>
         * </item><item>
         * <term>isPrivate</term>
         * <description>(<b>bool</b>) if {@code true}, the variable can be modified by its creator only (optional, default value: {@code false}).</description>
         * </item><item>
         * <term>isPersistent</term>
         * <description>(<b>bool</b>) if {@code true}, the variable will exist until its creator is connected to the current zone; if {@code false}, the variable will exist until its creator is connected to the current room (optional, default value: {@code false}).</description>
         * </item>
         * </list>
         * 
         * The following example shows how to save a persistent Room Variable called "score". This variable won't be destroyed when its creator leaves the room.
         * 			<code>
         * 			ArrayList rVars = new ArrayList();
         * 			rVars.Add(new RoomVariable("score", 2500, false, true));
         * 			
         * 			smartFox.SetRoomVariables(rVars);
         * 			</code>
         * 			
         * 			<hr />
         * 			The following example shows how to save two Room Variables at once. The one called "bestTime" is private and no other user except its owner can modify it.
         * 			<code>
         * 			ArrayList rVars = new ArrayList();
         * 			rVars.Add(new RoomVariable("bestTime", 100, true, false));
         * 			rVars.Add(new RoomVariable("bestLap", 120));
         * 			
         * 			smartFox.SetRoomVariables(rVars);
         * 			</code>
         * 			
         * 			<hr />
         * 			The following example shows how to delete a Room Variable called "bestTime" by setting its value to {@code null}.
         * 			<code>
         * 			ArrayList rVars = new ArrayList();
         * 			rVars.Add(new RoomVariable("bestTime", null));
         * 			
         * 			smartFox.SetRoomVariables(rVars);
         * 			</code>
         * 			
         * 			<hr />
         * 			The following example shows how to handle the data sent in the previous example when the {@link SFSEvent#onRoomVariablesUpdate} event is received.
         * 			<code>
         * 			SFSEvent.onRoomVariablesUpdate += OnRoomVariablesUpdate;
         * 			
         *			public void OnRoomVariablesUpdate(Room room, Dictionary<string, object> changedVars)
         *			{
         *			    // Iterate on the 'changedVars' Dictionary<string, object> to check which variables were updated
         *			        foreach (string v in changedVars.Keys)
         *						Trace.WriteLine(v + " room variable was updated; new value is: " + room.GetVariable(v));
         *			}
         * 			</code>
         * 			
         * 			<hr />
         * 			The following example shows how to update a Room Variable without affecting the variable's ownership.
         * 			By default, when a user updates a Room Variable, he becomes the "owner" of that variable. In some cases it could be needed to disable this behavoir by setting the <i>setOwnership</i> property to {@code false}. 
         * 			<code>
         * 			// For example, a variable that is defined in the server-side xml configuration file is owned by the Server itself;
         * 			// if it's not set to private, its owner will change as soon as a user updates it.
         * 			// To avoid this change of ownership the setOwnership flag is set to false.
         * 			ArrayList rVars = new ArrayList();
         * 			rVars.Add(new RoomVariable("shipPosX", 100));
         * 			rVars.Add(new RoomVariable("shipPosY", 200));
         * 			
         * 			smartFox.SetRoomVariables(rVars, smartFox.GetActiveRoom().GetId(), false);
         * 			</code>
         * </example>
         * 
         * <seealso cref="Room.GetVariable"/>
         * <seealso cref="Room.GetVariables"/>
         * <seealso cref="SFSEvent.OnRoomVariablesUpdateDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnRoomVariablesUpdateDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SetRoomVariables(List<RoomVariable> varList, int roomId, bool setOwnership)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg;

            if (setOwnership)
                xmlMsg = "<vars>";
            else
                xmlMsg = "<vars so='0'>";

            foreach (RoomVariable rv in varList)
                xmlMsg += GetXmlRoomVariable(rv);

            xmlMsg += "</vars>";

            Send(header, "setRvars", roomId, xmlMsg);

        }

        private string GetXmlRoomVariable(RoomVariable rVar)
        {
            // Get properties for this var
            string vName = rVar.GetName();
            object vValue = rVar.GetValue();
            string vPrivate = (rVar.IsPrivate()) ? "1" : "0";
            string vPersistent = (rVar.IsPersistent()) ? "1" : "0";

            string t = null;

            // Check type
            if (vValue == null)
            {
                t = "x";
                vValue = "";
            }
            else if (vValue.GetType() == typeof(bool))
            {
                t = "b";
                vValue = ((bool)vValue) ? "1" : "0";			// transform in number before packing in xml
            }
            else if (vValue.GetType() == typeof(int))
            {
                t = "n";
            }
            else if (vValue.GetType() == typeof(string))
            {
                t = "s";
            }

            if (t != null)
                return "<var n='" + vName + "' t='" + t + "' pr='" + vPrivate + "' pe='" + vPersistent + "'><![CDATA[" + vValue + "]]></var>";
            else
                return "";
        }

        #endregion

        #region Chat Messaging

        /**
         * <summary><see cref="SendPublicMessage(string, int)"/></summary>
         */
        public void SendPublicMessage(string message)
        {
            SendPublicMessage(message, -1);
        }

        /**
         * <summary>
         * Send a public message.<br/>
         * The message is broadcasted to all users in the current room, including the sender.
         * </summary>
         * 
         * <param name="message">the text of the public message.</param>
         * <param name="roomId">the id of the target room, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to send and receive a public message.
         * 			<code>
         * 			SFSEvent.onPublicMessage += OnPublicMessage;
         * 			
         * 			smartFox.SendPublicMessage("Hello world!");
         * 			
         * 			public void OnPublicMessage(string message, User sender, int roomId)
         * 			{
         * 				Trace.WriteLine("User " + sender.getName() + " said: " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SendPrivateMessage(string, int, int)"/>
         * <seealso cref="SFSEvent.OnPublicMessageDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnPublicMessageDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SendPublicMessage(string message, int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<txt><![CDATA[" + Entities.EncodeEntities(message) + "]]></txt>";

            Send(header, "pubMsg", roomId, xmlMsg);
        }

        /**
         * <summary><see cref="SendPrivateMessage(string, int, int)"/></summary>
         */
        public void SendPrivateMessage(string message, int recipientId)
        {
            SendPrivateMessage(message, recipientId, -1);
        }

        /**
         * <summary>
         * Send a private message to a user.<br/>
         * The message is broadcasted to the recipient and the sender.
         * </summary>
         * 
         * <param name="message">the text of the public message.</param>
         * <param name="recipientId">the id of the recipient user.</param>
         * <param name="roomId">the id of the room from where the message is sent, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to send and receive a private message.
         * 			<code>
         * 			SFSEvent.onPrivateMessage += OnPrivateMessage;
         * 			
         * 			smartFox.SendPrivateMessage("Hallo Jack!", 22);
         * 			
         * 			public void OnPrivateMessage(string message, User sender, int roomId, int userId)
         * 			{
         * 				Trace.WriteLine("User " + sender.getName() + " sent the following private message: " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SendPublicMessage(string, int)"/>
         * <seealso cref="SFSEvent.OnPrivateMessageDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnPrivateMessageDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SendPrivateMessage(string message, int recipientId, int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<txt rcp='" + recipientId + "'><![CDATA[" + Entities.EncodeEntities(message) + "]]></txt>";
            Send(header, "prvMsg", roomId, xmlMsg);
        }

        /**
         * <summary><see cref="SendModeratorMessage(string, string, int)"/></summary>
         */
        public void SendModeratorMessage(string message, string type)
        {
            SendModeratorMessage(message, type, -1);
        }

        /**
         * <summary>
         * Send a Moderator message to the current zone, the current room or a specific user in the current room.<br/>
         * In order to send these kind of messages, the user must have Moderator's privileges, which are set by SmartFoxServer when the user logs in (see the <see cref="Login"/> method).
         * </summary>
         * 
         * <param name="message">the text of the message.</param>
         * <param name="type">the type of message. The following constants can be passed: <see cref="MODMSG_TO_USER"/>, <see cref="MODMSG_TO_ROOM"/> and <see cref="MODMSG_TO_ZONE"/>, to send the message to a user, to the current room or to the entire current zone respectively.</param>
         * <param name="id">the id of the recipient room or user (ignored if the message is sent to the zone).</param>
         * 
         * <example>The following example shows how to send a Moderator message.
         * 			<code>
         * 			smartFox.SendModeratorMessage("Greetings from the Moderator", SmartFoxClient.MODMSG_TO_ROOM, smartFox.GetActiveRoom());
         * 			</code>
         * </example>
         * 
         * <seealso cref="Login"/>
         * <seealso cref="MODMSG_TO_USER"/>
         * <seealso cref="MODMSG_TO_ROOM"/>
         * <seealso cref="MODMSG_TO_ZONE"/>
         * <seealso cref="SFSEvent.OnModeratorMessageDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnModeratorMessageDelegate"/></para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.4.5</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SendModeratorMessage(string message, string type, int id)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<txt t='" + type + "' id='" + id + "'><![CDATA[" + Entities.EncodeEntities(message) + "]]></txt>";

            Send(header, "modMsg", activeRoomId, xmlMsg);
        }

        #endregion

        #region Object Messaging

        /**
         * <summary><see cref="SendObject(SFSObject, int)"/></summary>
         */
        public void SendObject(SFSObject obj)
        {
            SendObject(obj, -1);
        }

        /**
         * <summary>
         * Send an SFSObject to the other users in the current room.<br/>
         * This method can be used to send complex/nested data structures to clients, like a game move or a game status change. Supported data types are: Strings, Booleans, Numbers, Arrays, Objects.
         * </summary>
         * 
         * <param name="obj">the SFSObject to be sent.</param>
         * <param name="roomId">the id of the target room, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to send a simple object with primitive data to the other users.
         * 			<code>
         *   SFSObject move = new SFSObject();
         *   move.Put("x", 150);
         *   move.Put("y", 250);
         *   move.Put("speed", 8);
         *   smartFox.SendObject(move);
         * 			</code>
         * </example>
         * 
         * <seealso cref="SendObjectToGroup(SFSObject, ArrayList, int)"/>
         * <seealso cref="SFSEvent.OnObjectReceivedDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnObjectReceivedDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SendObject(SFSObject obj, int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlData = "<![CDATA[" + SFSObjectSerializer.GetInstance().Serialize(obj) + "]]>";

            Send(header, "asObj", roomId, xmlData);
        }

        /**
         * <summary><see cref="SendObjectToGroup(SFSObject, ArrayList, int)"/></summary>
         */
        public void SendObjectToGroup(SFSObject obj, List<int> userList)
        {
            SendObjectToGroup(obj, userList, -1);
        }

        /**
         * <summary>
         * Send an SFSObject to a group of users in the room.
         * See <see cref="SendObject(SFSObject, int)"/> for more info.
         * </summary>
         * 
         * <param name="obj">the SFSObject to be sent.</param>
         * <param name="userList">an ArrayList containing the id(s) of the recipients.</param>
         * <param name="roomId">the id of the target room, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to send a simple object with primitive data to two users.
         * 			<code>
         *   SFSObject move = new SFSObject();
         *   move.Put("x", 150);
         *   move.Put("y", 250);
         *   move.Put("speed", 8);
         *   smartFox.SendObject(move);
         *   
         * ArrayList userList = new ArrayList();
         * userList.Add(11);
         * userList.Add(12);
         * 
         * 			smartFox.SendObjectToGroup(move, userList);
         * 			</code>
         * </example>
         * 
         * <seealso cref="SendObject(SFSObject, int)"/>
         * <seealso cref="SFSEvent.OnObjectReceivedDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnObjectReceivedDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SendObjectToGroup(SFSObject obj, List<int> userList, int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            string strList = "";

            foreach (int i in userList)
            {
                strList += i + ",";
            }

            // remove last comma
            strList = strList.Substring(0, strList.Length - 1);

            obj.Put("_$$_", strList);

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<![CDATA[" + SFSObjectSerializer.GetInstance().Serialize(obj) + "]]>";

            Send(header, "asObjG", roomId, xmlMsg);
        }

        #endregion

        #region Extension Messaging

        /**
         * <summary><see cref="SendXtMessage(string, string, ICollection, string, int)"/></summary>
         */
        public void SendXtMessage(string xtName, string cmd, object paramObj)
        {
            SendXtMessage(xtName, cmd, paramObj, SmartFoxClient.XTMSG_TYPE_XML, -1);
        }

        /**
         * <summary><see cref="SendXtMessage(string, string, ICollection, string, int)"/></summary>
         */
        public void SendXtMessage(string xtName, string cmd, object paramObj, string type)
        {
            SendXtMessage(xtName, cmd, paramObj, type, -1);
        }

        /**
         * <summary>
         * Send a request to a server side extension.<br/>
         * The request can be serialized using three different protocols: XML, JSON and string-based (aka "raw protocol"). <br/>
         * XML and JSON can both serialize complex objects with any level of nested properties, while the string protocol allows to send linear data delimited by a separator (see the <see cref="GetRawProtocolSeparator"/> property).
         * </summary>
         * 
         * <param name="xtName">the name of the extension (see also the <see cref="CreateRoom(Dictionary<string, object>, int)"/> method).</param>
         * <param name="cmd">the name of the action/command to execute in the extension.</param>
         * <param name="paramObj">an object (Dictionary<string, object> for XML and JSON, ArrayList for string) containing the data to be passed to the extension (set to empty object if no data is required).</param>
         * <param name="type">the protocol to be used for serialization (optional). The following constants can be passed: <see cref="XTMSG_TYPE_XML"/>, <see cref="XTMSG_TYPE_STR"/>, <see cref="XTMSG_TYPE_JSON"/>.</param>
         * <param name="roomId">the id of the room where the request was originated, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to notify a multiplayer game server-side extension that a game action occurred.
         * 			<code>
         * 			// A bullet is being fired
         *   Dictionary<string, object> bulletInfo = new Dictionary<string, object>();
         *   bulletInfo["type"] = "bullet";
         *   bulletInfo["posx"] = 100;
         *   bulletInfo["posy"] = 200;
         *   bulletInfo["speed"] = 10;
         *   bulletInfo["angle"] = 45;
         * 			
         * 			// Invoke "fire" command on the extension called "gameExt", using JSON protocol
         * 			smartFox.SendXtMessage("gameExt", "fire", bulletInfo, SmartFoxClient.XTMSG_TYPE_JSON);
         * 			</code>
         * </example>
         * 
         * <seealso cref="GetRawProtocolSeparator"/>
         * <seealso cref="XTMSG_TYPE_XML"/>
         * <seealso cref="XTMSG_TYPE_JSON"/>
         * <seealso cref="XTMSG_TYPE_STR"/>
         * <seealso cref="SFSEvent.OnExtensionResponseDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnExtensionResponseDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public void SendXtMessage(string xtName, string cmd, object paramObj, string type, int roomId)
        {
            if (!CheckRoomList())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            // Verify paramObj type
            // Dictionary<string, object> required for JSON and XML
            // ArrayList for String
            if (paramObj != null && (type == SmartFoxClient.XTMSG_TYPE_JSON || type == SmartFoxClient.XTMSG_TYPE_XML))
            {
                if (paramObj.GetType() != typeof(Dictionary<object, object>))
                {
                    DebugMessage("ERROR sending JSON or XML Xt message. Parameter object is not a Dictionary<string, object>. Message has NOT been sent.");
                    return;
                }
            }
            if (paramObj != null && type == SmartFoxClient.XTMSG_TYPE_STR)
            {
                if (paramObj.GetType() != typeof(List<object>))
                {
                    DebugMessage("ERROR sending STR Xt message. Parameter object is not an ArrayList. Message has NOT been sent.");
                    return;
                }
            }

            // If paramObj is null, then create empty one
            if (paramObj == null && (type == SmartFoxClient.XTMSG_TYPE_JSON || type == SmartFoxClient.XTMSG_TYPE_XML)) paramObj = new Dictionary<string, object>();
            if (paramObj == null && type == SmartFoxClient.XTMSG_TYPE_STR) paramObj = new List<object>();

            // Send XML
            if (type == XTMSG_TYPE_XML)
            {
                Dictionary<string, object> header = new Dictionary<string, object>();
                header.Add("t", "xt");

                // Encapsulate message
                SFSObject xtReq = new SFSObject();
                xtReq.Put("name", xtName);
                xtReq.Put("cmd", cmd);
                xtReq.PutDictionary("param", (Dictionary<object, object>)paramObj);
                string xmlmsg = "<![CDATA[" + SFSObjectSerializer.GetInstance().Serialize(xtReq) + "]]>";

                Send(header, "xtReq", roomId, xmlmsg);
            }

            // Send raw/string
            else if (type == XTMSG_TYPE_STR)
            {
                string hdr = MSG_STR + "xt" + MSG_STR + xtName + MSG_STR + cmd + MSG_STR + roomId + MSG_STR;

                foreach (Object param in (List<object>)paramObj)
                    hdr += param.ToString() + MSG_STR;

                SendString(hdr);
            }

            // Send JSON
            else if (type == XTMSG_TYPE_JSON)
            {
                Dictionary<string, object> body = new Dictionary<string, object>();
                body.Add("x", xtName);
                body.Add("c", cmd);
                body.Add("r", roomId);
                body.Add("p", paramObj);

                Dictionary<string, object> obj = new Dictionary<string, object>();
                obj.Add("t", "xt");
                obj.Add("b", body);

                string msg = JsonMapper.ToJson(obj);
                SendJson(msg);
            }
        }

        #endregion

        #region User Variables

        /**
         * <summary><see cref="SetUserVariables(Dictionary<string, object>, int)"/></summary>
         */
        public void SetUserVariables(Dictionary<string, object> varObj)
        {
            SetUserVariables(varObj, -1);
        }

        /**
         * <summary>
         * Set on or more User Variables.<br/>
         * User Variables are a useful tool to store user data that has to be shared with other users. When a user sets/updates/deletes one or more User Variables, all the other users in the same room are notified. <br/>
         * Allowed data types for User Variables are Numbers, Strings and Booleans; Arrays and Objects are not supported in order save bandwidth.<br/>
         * If a User Variable is set to <c>null</c>, it is deleted from the server. Also, User Variables are destroyed when their owner logs out or gets disconnected.
         * </summary>
         * 
         * <param name="varObj">an object in which each property is a variable to set/update.</param>
         * <param name="roomId">the room id where the request was originated, in case of molti-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to save the user data (avatar name and position) in an avatar chat application.
         * 			<code>
         * 			Dictionary<string, object> uVars = new Dictionary<string, object>();
         * 			uVars.Add("myAvatar", "Homer");
         * 			uVars.Add("posx", 100);
         * 			uVars.Add("posy", 200);
         * 			
         * 			smartFox.SetUserVariables(uVars);
         * 			</code>
         * </example>
         * 
         * <seealso cref="User.GetVariable"/>
         * <seealso cref="User.GetVariables"/>
         * <seealso cref="SFSEvent.OnUserVariablesUpdateDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnUserVariablesUpdateDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SetUserVariables(Dictionary<string, object> varObj, int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");

            Room currRoom = GetActiveRoom();
            User user = currRoom.GetUser(myUserId);

            // Update local client
            user.SetVariables(varObj);

            // Prepare and send message
            string xmlMsg = GetXmlUserVariable(varObj);
            Send(header, "setUvars", roomId, xmlMsg);
        }

        private string GetXmlUserVariable(Dictionary<string, object> uVars)
        {
            string xmlStr = "<vars>";
            object val;
            string t;

            foreach (string key in uVars.Keys)
            {
                val = uVars[key];
                t = null;

                // Check types
                if (val == null)
                {
                    t = "x";
                    val = "";
                }
                else if (val.GetType() == typeof(bool))
                {
                    t = "b";
                    val = ((bool)val) ? "1" : "0";
                }
                else if (val.GetType() == typeof(int))
                {
                    t = "n";
                }
                else if (val.GetType() == typeof(string))
                {
                    t = "s";
                }

                if (t != null)
                    xmlStr += "<var n='" + key + "' t='" + t + "'><![CDATA[" + val + "]]></var>";
            }

            xmlStr += "</vars>";

            return xmlStr;
        }

        #endregion

        #region Misc

        /**
         * <summary>
         * Retrieve a random string key from the server.<br/>
         * This key is also referred in the SmartFoxServer documentation as the "secret key".<br/>
         * It's a unique key, valid for the current session only. It can be used to create a secure login system.
         * </summary>
         * 
         * <example>The following example shows how to handle the request a random key to the server.
         * 			<code>
         * 			SFSEvent.onRandomKey += OnRandomKey;
         * 			
         * 			smartFox.GetRandomKey();
         * 			
         * 			public void OnRandomKey(string key)
         * 			{
         * 				Trace.WriteLine("Random key received from server: " + key)
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SFSEvent.OnRandomKeyDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnRandomKeyDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public void GetRandomKey()
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "rndK", -1, "");
        }

        /**
         * <summary>
         * Get the SmartFoxServer Flash API version.
         * </summary>
         * 
         * <returns>The current version of the SmartFoxServer client API.</returns>
         * 
         * <example>The following example shows how to trace the SmartFoxServer API version.
         * 			<code>
         * 			Trace.WriteLine("Current API version: " + smartFox.GetVersion());
         * 			</code>
         * </example>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public string GetVersion()
        {
            return this.majVersion + "." + this.minVersion + "." + this.subVersion;
        }

        /**
         * <summary>
         * Send a roundtrip request to the server to test the connection' speed.<br/>
         * The roundtrip request sends a small packet to the server which immediately responds with another small packet, and causing the {@link SFSEvent#onRoundTripResponse} event to be fired.<br/>
         * The time taken by the packet to travel forth and back is called "roundtrip time" and can be used to calculate the average network lag of the client.<br/>
         * A good way to measure the network lag is to send continuos requests (every 3 or 5 seconds) and then calculate the average roundtrip time on a fixed number of responses (i.e. the last 10 measurements).
         * </summary>
         * 
         * <example>The following example shows how to check the average network lag time.
         * 			<code>
         * 			SFSEvent.onRoundTripResponse += OnRoundTripResponse;
         * 			
         * 			int totalPingTime = 0;
         * 			int pingCount = 0;
         * 			
         * 			smartFox.RoundTripBench(); // TODO: this method must be called repeatedly every 3-5 seconds to have a significant average value
         * 			
         * 			public void OnRoundTripResponse(int elapsed)
         * 			{
         * 				
         * 				// We assume that it takes the same time to the ping message to go from the client to the server
         * 				// and from the server back to the client, so we divide the elapsed time by 2.
         * 				totalPingTime += elapsed / 2;
         * 				pingCount++;
         * 				
         * 				int avg = Math.Round(totalPingTime / pingCount);
         * 				
         * 				Trace.WriteLine("Average lag: " + avg + " milliseconds");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SFSEvent.OnRoundTripResponseDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnRoundTripResponseDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void RoundTripBench()
        {
            this.benchStartTime = DateTime.Now;
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "roundTrip", activeRoomId, "");
        }

        /**
         * <summary><see cref="SwitchSpectator(int)"/></summary>
         */
        public void SwitchSpectator()
        {
            SwitchSpectator(-1);
        }

        /**
         * <summary>
         * Turn a spectator inside a game room into a player. <br/>
         * All spectators have their <b>player id</b> property set to -1; when a spectator becomes a player, his player id gets a number > 0, representing the player number. The player id values are assigned by the server, based on the order in which the players joined the room.<br/>
         * If the user joined more than one room, the id of the room where the switch should occur must be passed to this method.<br/>
         * The switch operation is successful only if at least one player slot is available in the room.
         * </summary>
         * 
         * <param name="roomId">the id of the room where the spectator should be switched, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to turn a spectator into a player.
         * 			<code>
         * 			SFSEvent.onSpectatorSwitched += OnSpectatorSwitched;
         * 			
         * 			smartFox.SwitchSpectator();
         * 			
         * 			public void OnSpectatorSwitched(bool success, int newId, Room room)
         * 			{
         * 				if (success)
         * 					Trace.WriteLine("You have been turned into a player; your player id is " + newId);
         * 				else
         * 					Trace.WriteLine("The attempt to switch from spectator to player failed");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="User.IsSpectator"/>
         * <seealso cref="SFSEvent.OnSpectatorSwitchedDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnSpectatorSwitchedDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SwitchSpectator(int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "swSpec", roomId, "");
        }

        /**
         * <summary><see cref="SwitchPlayer(int)"/></summary>
         */
        public void SwitchPlayer()
        {
            SwitchPlayer(-1);
        }

        /**
         * <summary>
         * Turn a player inside a game room into a spectator. <br/>
         * All players have their <b>player id</b> property set to a value > 0; when a spectator becomes a player, his playerId is set to -1.<br/>
         * If the user joined more than one room, the id of the room where the switch should occurr must be passed to this method.<br/>
         * The switch operation is successful only if at least one spectator slot is available in the room.<br/>
         * </summary>
         * 
         * <param name="roomId">the id of the room where the player should be switched to spectator, in case of multi-room join (optional, default value: <see cref="activeRoomId"/>).</param>
         * 
         * <example>The following example shows how to turn a player into a spectator.
         * 			<code>
         * 			SFSEvent.onPlayerSwitched += OnPlayerSwitched;
         * 			
         * 			smartFox.SwitchPlayer();
         * 			
         * 			public void OnPlayerSwitched(bool success, int newId, Room room)
         * 			{
         * 				if (success)
         * 					Trace.WriteLine("You have been turned into a spectator; your id is " + newId);
         * 				else
         * 					Trace.WriteLine("The attempt to switch from player to spectator failed!");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="User.IsSpectator"/>
         * <seealso cref="SFSEvent.OnPlayerSwitchedDelegate"/>
         * 
         * <remarks>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnPlayerSwitchedDelegate"/></para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public void SwitchPlayer(int roomId)
        {
            if (!CheckRoomList() || !CheckJoin())
            {
                return;
            }

            if (roomId == -1)
                roomId = activeRoomId;

            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            Send(header, "swPl", roomId, "");
        }

        /**
         * <summary>
         * Get the default upload path of the embedded webserver.
         * </summary>
         * 
         * <returns>The http address of the default folder in which files are uploaded.</returns>
         * 
         * <example>The following example shows how to get the default upload path.
         * 			<code>
         * 			string path = smartFox.GetUploadPath();
         * 			</code>
         * </example>
         * 
         * <seealso cref="UploadFile(string, int, string, int)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.5.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public string GetUploadPath()
        {
            return "http://" + this.ipAddress + ":" + this.httpPort + "/default/uploads/";
        }

        /**
         * <summary><see cref="UploadFile(string, int, string, int)"/></summary>
         */
        public void UploadFile(string filePath)
        {
            UploadFile(filePath, -1, "", -1);
        }

        /**
         * <summary><see cref="UploadFile(string, int, string, int)"/></summary>
         */
        public void UploadFile(string filePath, int id)
        {
            UploadFile(filePath, id, "", -1);
        }

        /**
         * <summary><see cref="UploadFile(string, int, string, int)"/></summary>
         */
        public void UploadFile(string filePath, int id, string nick)
        {
            UploadFile(filePath, id, nick, -1);
        }

        /**
         * <summary>
         * Upload a file to the embedded webserver.
         * </summary>
         * 
         * <param name="filePath">the FileReference object (see the example).</param>
         * <param name="id">the user id (optional, default value: <see cref="myUserId"/>).</param>
         * <param name="nick">the user name (optional, default value: <see cref="myUserName"/>).</param>
         * <param name="port">the webserver's TCP port (optional, default value: <see cref="httpPort"/>).</param>
         * 
         * <example>Check the Upload Tutorial available here: <a href="http://www.smartfoxserver.com/docs/docPages/tutorials_pro/14_imageManager/">Tutorial</a>
         * </example>
         * 
         * <seealso cref="myUserId"/>
         * <seealso cref="myUserName"/>
         * <seealso cref="httpPort"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * upload events fired in response should be handled by the provided FileReference object (see the example).</para>
         * <para><b>Sends:</b><br/>
         * <see cref="SFSEvent.OnSpectatorSwitchedDelegate"/></para>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.5.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void UploadFile(string filePath, int id, string nick, int port)
        {
            //if (id == -1)
            //    id = this.myUserId;

            //if (nick == "")
            //    nick = this.myUserName;

            //if (port == -1)
            //    port = this.httpPort;

            //WebClient uploadClient = new WebClient();
            //uploadClient.UploadFile("http://" + this.ipAddress + ":" + port + "/default/Upload.py?id=" + id + "&nick=" + nick, "POST", filePath);

            //DebugMessage("[UPLOAD]: http://" + this.ipAddress + ":" + port + "/default/Upload.py?id=" + id + "&nick=" + nick);
            DebugMessage("[UPLOAD]: Can't currently upload");
        }

        internal DateTime GetBenchStartTime()
        {
            return this.benchStartTime;
        }

        /**
         * <summary>
         * Set delay in socket polling
         * </summary>
         * 
         * <param name="delay">milliseconds to sleep between reading socket data. Defaults to no delay</param>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public void SetSocketPollDelay(int delay)
        {
            socketPollSleep = delay;
        }

        #endregion

        // -------------------------------------------------------
        // SFS Internal Methods
        // -------------------------------------------------------

        #region Initialization

        internal void __Logout()
        {
            Initialize(true);
        }

        private void Initialize()
        {
            Initialize(false);
        }

        private void Initialize(bool isLogOut)
        {
            // Clear local properties
            this.changingRoom = false;
            this.amIModerator = false;
            this.playerId = -1;
            this.activeRoomId = -1;
            this.myUserId = -1;
            this.myUserName = "";

            // Clear data structures
            this.roomList.Clear();
            this.buddyList.Clear();
            this.myBuddyVars.Clear();

            //ClearMessageBuffer();
            messageBuffer = "";

            // Set connection status
            if (!isLogOut)
            {
                this.connected = false;
                this.isHttpMode = false;
            }
        }

        #endregion

        #region Backend Messaging - Receiving

        private void SetupMessageHandlers()
        {
            this.messageHandlers.Clear();
            sysHandler = new SysHandler(this);
            extHandler = new ExtHandler(this);
            AddMessageHandler("sys", sysHandler);
            AddMessageHandler("xt", extHandler);
        }


        private void AddMessageHandler(string key, IMessageHandler handler)
        {
            if (this.messageHandlers.ContainsKey(key))
            {
                this.messageHandlers.Remove(key);
            }
            this.messageHandlers[key] = handler;
        }

        /*
         * Analyze incoming message
         */
        private void HandleMessage(string msg)
        {
            if (msg != "ok")
                DebugMessage("[ RECEIVED ]: " + msg + ", (len: " + msg.Length + ")");

            //Ignore DTD stuff
            if (msg.Contains("<!DOCTYPE cross-domain-policy"))
                return;

            string type = msg.Substring(0, 1);

            if (type == MSG_XML)
            {
                XmlReceived(msg);
            }
            else if (type == MSG_STR)
            {
                StrReceived(msg);
            }
            else if (type == MSG_JSON)
            {
                JsonReceived(msg);
            }
        }

        private void XmlReceived(string msg)
        {
            // Got XML response
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                //settings. = false;                

                XDocument xmlData;
                
                using (StringReader sr = new StringReader(msg))
                {
                    using (XmlReader reader = XmlReader.Create(sr, settings))
                    {
                        xmlData = XDocument.Load(reader);
                    }
                }
                    
                string handlerId = xmlData.Element("msg").Attribute("t").Value;
                IMessageHandler handler = (IMessageHandler)messageHandlers[handlerId];
                if (handler != null)
                {
                    handler.HandleMessage(xmlData, XTMSG_TYPE_XML);
                }
            }
            catch (NullReferenceException e)
            {
                DebugMessage("XML Handler null reference exception " + e.ToString());
            }
        }

        private void JsonReceived(string msg)
        {
            // Got JSON response

            JsonData jso = JsonMapper.ToObject(msg);

            string handlerId = (string)jso["t"];
            IMessageHandler handler = (IMessageHandler)messageHandlers[handlerId];

            if (handler != null)
                handler.HandleMessage(jso["b"], XTMSG_TYPE_JSON);
        }

        private void StrReceived(string msg)
        {
            // Got string response

            string[] parameters = msg.Substring(1, msg.Length - 2).Split(MSG_STR.ToCharArray());

            string handlerId = parameters[0];
            IMessageHandler handler = (IMessageHandler)messageHandlers[handlerId];

            if (handler != null)
                handler.HandleMessage(string.Join(MSG_STR, parameters, 1, parameters.Length - 1), XTMSG_TYPE_STR);
        }

        #endregion

        #region Backend Messaging - Sending

        private void Send(Dictionary<string, object> header, string action, int fromRoom, string message)
        {

            // Setup Msg Header
            string xmlMsg = MakeXmlHeader(header);

            // Setup Body
            xmlMsg += "<body action='" + action + "' r='" + fromRoom + "'>" + message + "</body>" + CloseXmlHeader();

            DebugMessage("[Sending]: " + xmlMsg + "\n");


            if (isHttpMode)
                httpConnection.Send(xmlMsg);
            else
                WriteToSocket(xmlMsg);

        }

        private string MakeXmlHeader(Dictionary<string, object> headerObj)
        {
            string xmlData = "<msg";

            foreach (KeyValuePair<string, object> pair in headerObj)
            {
                xmlData += " " + pair.Key + "='" + pair.Value + "'";
            }

            xmlData += ">";

            return xmlData;
        }

        private string CloseXmlHeader()
        {
            return "</msg>";
        }

        internal void SendString(string strMessage)
        {
            DebugMessage("[Sending - STR]: " + strMessage + "\n");

            if (isHttpMode)
                httpConnection.Send(strMessage);
            else

                WriteToSocket(strMessage);
        }

        internal void SendJson(string jsMessage)
        {
            DebugMessage("[Sending - JSON]: " + jsMessage + "\n");

            if (isHttpMode)
                httpConnection.Send(jsMessage);
            else

                WriteToSocket(jsMessage);
        }

        #endregion

        #region Socket Communication

        private void HandleSocketConnection(object sender, EventArgs e)
        {
            Dictionary<string, object> header = new Dictionary<string, object>();
            header.Add("t", "sys");
            string xmlMsg = "<ver v='" + this.majVersion + this.minVersion + this.subVersion + "' />";

            Send(header, "verChk", 0, xmlMsg);
        }

        private void HandleSocketDisconnection()
        {
            // Clear data
            Initialize();

            // Fire event
            SFSEvent sfse = new SFSEvent(SFSEvent.onConnectionLostEvent, new Dictionary<string, object>());
            EnqueueEvent(sfse);
        }

        private void HandleIOError(string originalError)
        {
            TryBlueBoxConnection(originalError);
            DispatchConnectionError(originalError);
        }

        private void WriteToSocket(string msg)
        {
            if (!connected)
            {
                DebugMessage("WriteToSocket: Not Connected.");
                return;
            }

            try
            {
                Byte[] bytes = Encoding.UTF8.GetBytes(msg + (char)0);

                // *** If previous doesn't work try this: ---
                // Byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(msg + (char)0);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bytes, 0, bytes.Length);
                args.UserToken = socket;
                args.RemoteEndPoint = endPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                socket.SendAsync(args);


            }
            catch (NullReferenceException e)
            {
                HandleIOError(e.Message);
            }
            catch (SocketException e)
            {
                HandleIOError(e.Message);
            }

        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                HandleIOError(e.SocketError.ToString());

            }
        }

        private void OnSocketReceive(object sender, SocketAsyncEventArgs e)
        {
            StringReader sr = null;
            try
            {
                string data = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                sr = new StringReader(data);

                string msg = sr.ReadToEnd();

                if (msg.Length < 1)
                {
                    DebugMessage("Disconnect due to lost socket connection");
                    Disconnect();
                    return;
                }
                // Add the received byte message to the messageBuffer, so we can cut up that one
                messageBuffer += msg;

                // Cut up and handle each message separately
                Regex findNumMessagesRegEx = new Regex("\0");
                int numMessages = findNumMessagesRegEx.Matches(messageBuffer).Count;

                if (numMessages != 0)
                {
                    char[] delimChar = { '\0' };
                    string[] messages = messageBuffer.Split(delimChar);
                    bool restsPartOfBuffer = false;
                    for (int strCount = 0; strCount < messages.Length; strCount++)
                    {
                        // If this is the last string and its null, then we send all - nothing left for the buffer
                        // place rest in buffer else
                        if (strCount == messages.Length - 1)
                        {
                            if (messages[strCount].Length != 0)
                            {
                                messageBuffer = messages[strCount];
                                restsPartOfBuffer = true;
                            }
                            else
                            {
                                //Ignore last empty one
                                messageBuffer = "";
                                break;
                            }

                        }
                        if (!restsPartOfBuffer)
                        {
                            HandleMessage(messages[strCount]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugMessage("Exception while receiving data: " + ex.ToString());
            }
            finally
            {
                if (sr != null) sr.Close();
            }

            //Prepare to receive more data
            Socket socket = (Socket)e.UserToken;
            socket.ReceiveAsync(e);
        }

        #endregion

        #region BlueBox / Http Communication

        /*
        * New in 1.5.4
        */
        private void TryBlueBoxConnection(string originalError) // TODO ErrorEvent evt)
        {
            if (!connected)
            {
                if (smartConnect)
                {
                    DebugMessage("Socket connection failed. Trying BlueBox");

                    isHttpMode = true;
                    string __ip = blueBoxIpAddress != null ? blueBoxIpAddress : ipAddress.ToString();
                    int __port = blueBoxPort != 0 ? blueBoxPort : httpPort;

                    httpConnection.Connect(__ip, __port);
                }
                else
                    DispatchConnectionError(originalError);

            }
            else
            {
                // Dispatch back the IO error
                SFSEvent sfse = new SFSEvent(SFSEvent.onConnectionLostEvent, new Dictionary<string, object>());
                DispatchEvent(sfse);
                DebugMessage("[WARN] Connection error: " + originalError);
            }
        }

        private void StartHttpPollThread()
        {
            if (thrHttpPoll == null)
            {
                ThreadStart starter = delegate { HttpPoll(); };
                thrHttpPoll = new Thread(new ThreadStart(starter));
                thrHttpPoll.IsBackground = true;
                thrHttpPoll.Start();
            }
        }

        private void HttpPoll()
        {
            while (true)
            {
                httpConnection.Send(HTTP_POLL_REQUEST);
                Thread.Sleep(_httpPollSpeed);
            }
        }

        private void HandleHttpData(HttpEvent evt)
        {
            string data = (string)evt.GetParameter("data");
            string[] messages = data.ToString().Split('\n');
            string message;

            if (messages[0] != "")
            {
                /*
                if (messages[0] != "ok")
                    trace("  HTTP DATA ---> " + messages + " (len: " + messages.length + ")")
                */

                for (int i = 0; i < messages.Length - 1; i++)
                {
                    message = messages[i];

                    if (message.Length > 0)
                        HandleMessage(message);
                }
            }
        }

        private void HandleHttpConnect(HttpEvent evt)
        {
            StartHttpPollThread();

            this.HandleSocketConnection(null, null);

            connected = true;

            httpConnection.Send(HTTP_POLL_REQUEST);
        }

        private void HandleHttpClose(HttpEvent evt)
        {
            if (thrHttpPoll != null && thrHttpPoll.IsAlive)
            {
                thrHttpPoll.Abort();
            }

            // Clear data
            Initialize();

            // Fire event
            SFSEvent sfse = new SFSEvent(SFSEvent.onConnectionLostEvent, new Dictionary<string, object>());
            DispatchEvent(sfse);
        }

        private void HandleHttpError(HttpEvent evt)
        {
            if (!connected)
            {
                DebugMessage("HttpError: " + ((Exception)evt.GetParameter("exception")).Message);
                DispatchConnectionError(((Exception)evt.GetParameter("exception")).Message);
            }
        }

        #endregion

        #region Dispatching Event Callbacks

        private void DispatchConnectionError(string errorMessage)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("success", false);
            parameters.Add("error", "I/O Error: " + errorMessage);

            SFSEvent sfse = new SFSEvent(SFSEvent.onConnectionEvent, parameters);
            DispatchEvent(sfse);
        }

        internal void DispatchEvent(SFSEvent evt)
        {
            if (runInQueueMode)
            {
                EnqueueEvent(evt);
            }
            else
            {
                _DispatchEvent(evt);
            }
        }

        internal void EnqueueEvent(SFSEvent evt)
        {
            // We do lock here to ensure tread safety of our enqueing
            lock (sfsQueuedEventsLocker)
            {
                sfsQueuedEvents.Add(evt);
            }
        }

        internal void _DispatchEvent(SFSEvent evt)
        {
            try
            {
                switch (evt.GetType())
                {
                    case SFSEvent.onAdminMessageEvent:
                        if (SFSEvent.onAdminMessage != null)
                        {
                            SFSEvent.onAdminMessage((string)evt.GetParameter("message"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onAdminMessage, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onBuddyListEvent:
                        if (SFSEvent.onBuddyList != null)
                        {
                            SFSEvent.onBuddyList(evt.GetParameter("list") as List<Buddy>);
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onBuddyList, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onBuddyListErrorEvent:
                        if (SFSEvent.onBuddyListError != null)
                        {
                            SFSEvent.onBuddyListError((string)evt.GetParameter("error"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onBuddyListError, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onBuddyListUpdateEvent:
                        if (SFSEvent.onBuddyListUpdate != null)
                        {
                            SFSEvent.onBuddyListUpdate((Buddy)evt.GetParameter("buddy"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onBuddyListUpdate, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onBuddyPermissionRequestEvent:
                        if (SFSEvent.onBuddyPermissionRequest != null)
                        {
                            SFSEvent.onBuddyPermissionRequest((string)evt.GetParameter("sender"), (string)evt.GetParameter("message"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onBuddyPermissionRequest, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onBuddyRoomEvent:
                        if (SFSEvent.onBuddyRoom != null)
                        {
                            SFSEvent.onBuddyRoom((List<int>)evt.GetParameter("idList"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onBuddyRoom, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onConfigLoadFailureEvent:
                        if (SFSEvent.onConfigLoadFailure != null)
                        {
                            SFSEvent.onConfigLoadFailure((string)evt.GetParameter("message"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onConfigLoadFailure, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onConfigLoadSuccessEvent:
                        if (SFSEvent.onConfigLoadSuccess != null)
                        {
                            SFSEvent.onConfigLoadSuccess();
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onConfigLoadSuccess, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onConnectionEvent:
                        if (SFSEvent.onConnection != null)
                        {
                            SFSEvent.onConnection((bool)evt.GetParameter("success"), (string)evt.GetParameter("error"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onConnection, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onConnectionLostEvent:
                        if (SFSEvent.onConnectionLost != null)
                        {
                            SFSEvent.onConnectionLost();
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onConnectionLost, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onCreateRoomErrorEvent:
                        if (SFSEvent.onCreateRoomError != null)
                        {
                            SFSEvent.onCreateRoomError((string)evt.GetParameter("error"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onCreateRoomError, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onDebugMessageEvent:
                        if (SFSEvent.onDebugMessage != null)
                        {
                            SFSEvent.onDebugMessage((string)evt.GetParameter("message"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onDebugMessage, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onExtensionResponseEvent:
                        if (SFSEvent.onExtensionResponse != null)
                        {
                            SFSEvent.onExtensionResponse(evt.GetParameter("dataObj"), (string)evt.GetParameter("type"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onExtensionResponse, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onJoinRoomEvent:
                        if (SFSEvent.onJoinRoom != null)
                        {
                            SFSEvent.onJoinRoom((Room)evt.GetParameter("room"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onJoinRoom, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onJoinRoomErrorEvent:
                        if (SFSEvent.onJoinRoomError != null)
                        {
                            SFSEvent.onJoinRoomError((string)evt.GetParameter("error"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onJoinRoomError, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onLoginEvent:
                        if (SFSEvent.onLogin != null)
                        {
                            SFSEvent.onLogin((bool)evt.GetParameter("success"), (string)evt.GetParameter("name"), (string)evt.GetParameter("error"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onLogin, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onLogoutEvent:
                        if (SFSEvent.onLogout != null)
                        {
                            SFSEvent.onLogout();
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onLogout, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onModeratorMessageEvent:
                        if (SFSEvent.onModeratorMessage != null)
                        {
                            SFSEvent.onModeratorMessage((string)evt.GetParameter("message"), (User)evt.GetParameter("sender"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onModeratorMessage, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onObjectReceivedEvent:
                        if (SFSEvent.onObjectReceived != null)
                        {
                            SFSEvent.onObjectReceived((SFSObject)evt.GetParameter("obj"), (User)evt.GetParameter("sender"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onObjectReceived, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onPrivateMessageEvent:
                        if (SFSEvent.onPrivateMessage != null)
                        {
                            SFSEvent.onPrivateMessage((string)evt.GetParameter("message"), (User)evt.GetParameter("sender"), (int)evt.GetParameter("roomId"), (int)evt.GetParameter("userId"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onPrivateMessage, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onPublicMessageEvent:
                        if (SFSEvent.onPublicMessage != null)
                        {
                            SFSEvent.onPublicMessage((string)evt.GetParameter("message"), (User)evt.GetParameter("sender"), (int)evt.GetParameter("roomId"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onPublicMessage, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRandomKeyEvent:
                        if (SFSEvent.onRandomKey != null)
                        {
                            SFSEvent.onRandomKey((string)evt.GetParameter("key"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRandomKey, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRoomAddedEvent:
                        if (SFSEvent.onRoomAdded != null)
                        {
                            SFSEvent.onRoomAdded((Room)evt.GetParameter("room"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRoomAdded, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRoomDeletedEvent:
                        if (SFSEvent.onRoomDeleted != null)
                        {
                            SFSEvent.onRoomDeleted((Room)evt.GetParameter("room"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRoomDeleted, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRoomLeftEvent:
                        if (SFSEvent.onRoomLeft != null)
                        {
                            SFSEvent.onRoomLeft((int)evt.GetParameter("roomId"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRoomLeft, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRoomListUpdateEvent:
                        if (SFSEvent.onRoomListUpdate != null)
                        {
                            SFSEvent.onRoomListUpdate((Dictionary<int, Room>)evt.GetParameter("roomList"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRoomListUpdate, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRoomVariablesUpdateEvent:
                        if (SFSEvent.onRoomVariablesUpdate != null)
                        {
                            SFSEvent.onRoomVariablesUpdate((Room)evt.GetParameter("room"), (Dictionary<string, object>)evt.GetParameter("changedVars"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRoomVariablesUpdate, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onRoundTripResponseEvent:
                        if (SFSEvent.onRoundTripResponse != null)
                        {
                            SFSEvent.onRoundTripResponse((int)evt.GetParameter("elapsed"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onRoundTripResponse, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onPlayerSwitchedEvent:
                        if (SFSEvent.onPlayerSwitched != null)
                        {
                            SFSEvent.onPlayerSwitched((bool)evt.GetParameter("success"), (int)evt.GetParameter("newId"), (Room)evt.GetParameter("room"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onPlayerSwitched, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onSpectatorSwitchedEvent:
                        if (SFSEvent.onSpectatorSwitched != null)
                        {
                            SFSEvent.onSpectatorSwitched((bool)evt.GetParameter("success"), (int)evt.GetParameter("newId"), (Room)evt.GetParameter("room"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onSpectatorSwitched, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onUserCountChangeEvent:
                        if (SFSEvent.onUserCountChange != null)
                        {
                            SFSEvent.onUserCountChange((Room)evt.GetParameter("room"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onUserCountChange, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onUserEnterRoomEvent:
                        if (SFSEvent.onUserEnterRoom != null)
                        {
                            SFSEvent.onUserEnterRoom((int)evt.GetParameter("roomId"), (User)evt.GetParameter("user"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onUserEnterRoom, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onUserLeaveRoomEvent:
                        if (SFSEvent.onUserLeaveRoom != null)
                        {
                            SFSEvent.onUserLeaveRoom((int)evt.GetParameter("roomId"), (int)evt.GetParameter("userId"), (string)evt.GetParameter("userName"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onUserLeaveRoom, but no callback is registered");
                        }
                        break;
                    case SFSEvent.onUserVariablesUpdateEvent:
                        if (SFSEvent.onUserVariablesUpdate != null)
                        {
                            SFSEvent.onUserVariablesUpdate((User)evt.GetParameter("user"), (Dictionary<string, object>)evt.GetParameter("changedVars"));
                        }
                        else
                        {
                            Console.Error.WriteLine("Trying to call onUserVariablesUpdate, but no callback is registered");
                        }
                        break;
                    default:
                        DebugMessage("Unknown event dispatched " + evt.GetType());
                        break;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR: Exception thrown dispatching event. Exception: " + e.ToString());
                if (SFSEvent.onDebugMessage != null)
                {
                    SFSEvent.onDebugMessage("ERROR: Exception thrown dispatching event. Exception: " + e.ToString());
                }
            }
        }
        #endregion

        #region Helpers

        internal void DebugMessage(string message)
        {
            if (this.debug)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("message", message);
                SFSEvent evt = new SFSEvent(SFSEvent.onDebugMessageEvent, parameters);
                DispatchEvent(evt);
            }
        }

        internal bool CheckRoomList()
        {
            bool success = true;

            if (roomList == null || roomList.Count == 0)
            {
                success = false;
                ErrorTrace("The room list is empty!\nThe client API cannot function properly until the room list is populated.\nPlease consult the documentation for more infos.");
            }
            return success;
        }

        internal bool CheckJoin()
        {
            bool success = true;

            if (activeRoomId < 0)
            {
                success = false;
                ErrorTrace("You haven't joined any rooms!\nIn order to interact with the server you should join at least one room.\nPlease consult the documentation for more infos.");

            }
            return success;
        }

        internal void ErrorTrace(String msg)
        {
            Console.WriteLine("\n****************************************************************");
            Console.WriteLine("Warning:");
            Console.WriteLine(msg);
            Console.WriteLine("****************************************************************");
            DebugMessage(msg);
        }

        #endregion

    }
}

