using SmartFoxClientAPI.Data;
using System.Collections.Generic;

namespace SmartFoxClientAPI
{

    /**
     * <summary>SFSEvent is the class representing all events dispatched by the <see>SmartFoxClient</see> instance.<br/>
     * The SFSEvent class uses delegates to callback on specific event types.
     * </summary>
     * 
     * <example>The following example show a generic usage of a SFSEvent. Please refer to the specific events for the return signature/parameters.
     * 			<code>
     * 			using SmartFoxClientAPI;
     *          using SmartFoxClientAPI.Data;
     *          ...(+ all the System.* references)
     *           
     *          public partial class Page : UserControl
     * 			{
     *				SmartFoxClient smartFox;
     *					
     *				public function MyTest()
     *				{
     *					// Create instance
     *					smartFox = new SmartFoxClient();
     *						
     *					// Add event handler for connection 
     *					SFSEvent.onConnection += OnConnection;
     *						
     *					// Connect to server
     *					smartFox.Connect("127.0.0.1", 4502)	
     *				}
     *					
     *				// Handle connection event
     *				public void OnConnection(bool success, string error)
     *				{
     *					if (success)
     *						Trace.WriteLine("Great, successfully connected!");
     *					else
     *						Trace.WriteLine("Ouch, connection failed!");
     *				}	
     * 			}
     * 			</code>
     * 			<b>NOTE</b>: in the following examples, <c>smartFox</c> always indicates a SmartFoxClient instance.
     * </example>
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
    public class SFSEvent
    {        
        #region Internal Event Constants

        internal const string onAdminMessageEvent = "OnAdminMessage";
        internal const string onBuddyListEvent = "OnBuddyList";
        internal const string onBuddyListErrorEvent = "OnBuddyListError";
        internal const string onBuddyListUpdateEvent = "OnBuddyListUpdate";
        internal const string onBuddyPermissionRequestEvent = "OnBuddyPermissionRequest";
        internal const string onBuddyRoomEvent = "OnBuddyRoom";
        internal const string onConfigLoadFailureEvent = "OnConfigLoadFailure";
        internal const string onConfigLoadSuccessEvent = "OnConfigLoadSuccess";
        internal const string onConnectionEvent = "OnConnection";
        internal const string onConnectionLostEvent = "OnConnectionLost";
        internal const string onCreateRoomErrorEvent = "OnCreateRoomError";
        internal const string onDebugMessageEvent = "OnDebugMessage";
        internal const string onExtensionResponseEvent = "OnExtensionResponse";
        internal const string onJoinRoomEvent = "OnJoinRoom";
        internal const string onJoinRoomErrorEvent = "OnJoinRoomError";
        internal const string onLoginEvent = "OnLogin";
        internal const string onLogoutEvent = "OnLogout";
        internal const string onModeratorMessageEvent = "OnModMessage";
        internal const string onObjectReceivedEvent = "OnObjectReceived";
        internal const string onPlayerSwitchedEvent = "OnPlayerSwitched";
        internal const string onPrivateMessageEvent = "OnPrivateMessage";
        internal const string onPublicMessageEvent = "OnPublicMessage";
        internal const string onRandomKeyEvent = "OnRandomKey";
        internal const string onRoomAddedEvent = "OnRoomAdded";
        internal const string onRoomDeletedEvent = "OnRoomDeleted";
        internal const string onRoomLeftEvent = "OnRoomLeft";
        internal const string onRoomListUpdateEvent = "OnRoomListUpdate";
        internal const string onRoomVariablesUpdateEvent = "OnRoomVariablesUpdate";
        internal const string onRoundTripResponseEvent = "OnRoundTripResponse";
        internal const string onSpectatorSwitchedEvent = "OnSpectatorSwitched";
        internal const string onUserCountChangeEvent = "OnUserCountChange";
        internal const string onUserEnterRoomEvent = "OnUserEnterRoom";
        internal const string onUserLeaveRoomEvent = "OnUserLeaveRoom";
        internal const string onUserVariablesUpdateEvent = "OnUserVariablesUpdate";

        #endregion

        #region Event Delegates

        /**
         * <summary>
         * Dispatched when a message from the Administrator is received.<br/>
         * Admin messages are special messages that can be sent by an Administrator to a user or group of users.
         * </summary>
         * 
         * <param name="message">the Administrator's message</param>
         * 
         * <example>The following example shows how to handle a message coming from the Administrator.
         * 			<code>
         * 			SFSEvent.onAdminMessage += OnAdminMessage;
         * 			
         * 			public void OnAdminMessage(string message)
         * 			{
         * 				Trace.WriteLine("Administrator said: " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnModeratorMessageDelegate"/>
         * 
         * <remarks>
         * <para>
         * All client applications should handle this event, or users won't be be able to receive important admin notifications!
         * </para>
         * 
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnAdminMessageDelegate(string message);

        /**
         * <summary><see cref="OnAdminMessageDelegate"/></summary>
         */
        public static OnAdminMessageDelegate onAdminMessage;

        /**
         * <summary>
         * Dispatched when the buddy list for the current user is received or a buddy is added/removed.
         * </summary>
         * 
         * <param name="buddyList">the buddy list. Refer to the <see>buddyList</see> property for a description of the buddy object's properties.</param>
         * 
         * <example>The following example shows how to retrieve the properties of each buddy when the buddy list is received.
         * 			<code>
         * 			SFSEvent.onBuddyList += OnBuddyList;
         * 			
         * 			smartFox.LoadBuddyList();
         * 
         * 			public void OnBuddyList(List&lt;Buddy&gt; buddyList)
         * 			{
         * 				foreach (Buddy buddy in buddyList)
         * 				{
         * 					Trace.WriteLine("Buddy id: " + buddy.GetId());
         * 					Trace.WriteLine("Buddy name: " + buddy.GetName());
         * 					Trace.WriteLine("Is buddy online? " + buddy.IsOnline());
         * 					Trace.WriteLine("Is buddy blocked? " + buddy.IsBlocked());
         * 					
         * 					Trace.WriteLine("Buddy Variables:");
         * 					foreach (string v in buddy.GetVariables().Keys)
         * 						Trace.WriteLine("\t" + v + " -- " + buddy.GetVariable{v});
         * 				}
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnBuddyListErrorDelegate"/>
         * <seealso cref="OnBuddyListUpdateDelegate"/>
         * <seealso cref="OnBuddyRoomDelegate"/>
         * <seealso cref="SmartFoxClient.buddyList"/>
         * <seealso cref="SmartFoxClient.LoadBuddyList"/>
         * <seealso cref="SmartFoxClient.AddBuddy"/>
         * <seealso cref="SmartFoxClient.RemoveBuddy"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnBuddyListDelegate(List<Buddy> buddyList);


        /**
         * <summary><see cref="OnBuddyListDelegate"/></summary>
         */
        public static OnBuddyListDelegate onBuddyList;

        /**
         * <summary>
         * Dispatched when an error occurs while loading the buddy list.
         * </summary>
         * 
         * <param name="error">the error message</param>
         * 
         * <example>The following example shows how to handle a potential error in buddy list loading.
         * 			<code>
         * 			SFSEvent.onBuddyListError += OnBuddyListError;
         * 			
         * 			public void OnBuddyListError(string error)
         * 			{
         * 				Trace.WriteLine("An error occurred while loading the buddy list: " + error);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnBuddyListDelegate"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnBuddyListErrorDelegate(string error);

        /**
         * <summary><see cref="OnBuddyListErrorDelegate"/></summary>
         */
        public static OnBuddyListErrorDelegate onBuddyListError;


        /**
         * <summary>
         * Dispatched when the status or variables of a buddy in the buddy list change.
         * </summary>
         * 
         * <param name="buddy">an object representing the buddy whose status or Buddy Variables have changed. Refer to the <see cref="SmartFoxClient.buddyList"/> property for a description of the buddy object's properties.</param>
         * 
         * <example>The following example shows how to handle the online status change of a buddy.
         * 			<code>
         * 			SFSEvent.onBuddyListUpdate += OnBuddyListUpdate;
         * 			
         * 			public void OnBuddyListUpdate(Buddy buddy)
         * 			{
         * 				string name = buddy.GetName();
         * 				string status = (buddy.IsOnline()) ? "online" : "offline";
         * 
         * 				Trace.WriteLine("Buddy " + name + " is currently " + status);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnBuddyListDelegate"/>
         * <seealso cref="SmartFoxClient.buddyList"/>
         * <seealso cref="SmartFoxClient.SetBuddyBlockStatus"/>
         * <seealso cref="SmartFoxClient.SetBuddyVariables"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnBuddyListUpdateDelegate(Buddy buddy);

        /**
         * <summary><see cref="OnBuddyListUpdateDelegate"/></summary>
         */
        public static OnBuddyListUpdateDelegate onBuddyListUpdate;

        /**
         * <summary>
         * Dispatched when the current user receives a request to be added to the buddy list of another user.
         * </summary>
         * 
         * <param name="sender">the name of the user requesting to add the current user to his/her buddy list</param>
         * <param name="message">a message accompaining the permission request. This message can't be sent from the client-side, but it's part of the advanced server-side buddy list features.</param>
         * 
         * <example>The following example shows how to handle the request to be added to a buddy list.
         * 			<code>
         * 			SFSEvent.onBuddyPermissionRequest += OnBuddyPermissionRequest;
         * 			
         * 			public void OnBuddyPermissionRequest(string sender, string message)
         * 			{
         * 			    Trace.WriteLine("Buddy permission request from " + sender + " asking " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SmartFoxClient.AddBuddy"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public delegate void OnBuddyPermissionRequestDelegate(string sender, string message);

        /**
         * <summary><see cref="OnBuddyPermissionRequestDelegate"/></summary>
         */
        public static OnBuddyPermissionRequestDelegate onBuddyPermissionRequest;

        /**
         * <summary>
         * Dispatched in response to a <see cref="SmartFoxClient.GetBuddyRoom"/> request.
         * </summary>
         * 
         * <param name="idList">the list of id of the rooms in which the buddy is currently logged; if users can't be present in more than one room at the same time, the list will contain one room id only, at 0 index.</param>
         * 
         * <example>The following example shows how to join the same room in which the buddy currently is.
         * 			<code>
         * 			SFSEvent.onBuddyRoom += OnBuddyRoom;
         * 			
         * 			Buddy buddy = smartFox.GetBuddyByName("jack");
         * 			smartFox.GetBuddyRoom(buddy);
         * 			
         * 			public void OnBuddyRoom(List&lt;int&gt; idList)
         * 			{
         * 				// Reach the buddy in his room
         * 				smartFox.Join(idList[0]);
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="SmartFoxClient.GetBuddyRoom"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnBuddyRoomDelegate(List<int> idList);

        /**
         * <summary><see cref="OnBuddyRoomDelegate"/></summary>
         */
        public static OnBuddyRoomDelegate onBuddyRoom;

        /**
         * <summary>
         * Dispatched when an error occurs while loading the external SmartFoxClient configuration file.
         * </summary>
         * 
         * <param name="message">the error message.</param>
         * 
         * <example>The following example shows how to handle a potential error in configuration loading.
         * 			<code>
         * 			SFSEvent.onConfigLoadFailure += OnConfigLoadFailure;
         * 			
         * 			smartFox.LoadConfig("testEnvironmentConfig.xml");
         * 			
         * 			public void OnConfigLoadFailure(string message)
         * 			{
         * 				Trace.WriteLine("Failed loading config file: " + message);
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="OnConfigLoadSuccessDelegate"/>
         * <seealso cref="SmartFoxClient.LoadConfig(string, bool)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public delegate void OnConfigLoadFailureDelegate(string message);

        /**
         * <summary><see cref="OnConfigLoadFailureDelegate"/></summary>
         */
        public static OnConfigLoadFailureDelegate onConfigLoadFailure;

        /**
         * <summary>
         * Dispatched when the external SmartFoxClient configuration file has been loaded successfully.<br/>
         * This event is dispatched only if the <i>autoConnect</i> parameter of the <see cref="SmartFoxClient.LoadConfig(string, bool)"/> method is set to <c>true</c> otherwise the connection is made and the <see cref="OnConnectionDelegate"/> event fired.
         * </summary>
         * 
         * <example>The following example shows how to handle a successful configuration loading.
         * 			<code>
         * 			SFSEvent.onConfigLoadSuccess += OnConfigLoadSuccess;
         * 			
         * 			smartFox.LoadConfig("testEnvironmentConfig.xml", false);
         * 			
         * 			public void OnConfigLoadSuccess()
         * 			{
         * 				Trace.WriteLine("Config file loaded, now connecting...");
         * 				
         * 				smartFox.Connect(smartFox.ipAddress, smartFox.port);
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="OnConfigLoadFailureDelegate"/>
         * <seealso cref="SmartFoxClient.LoadConfig(string, bool)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.6.0</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public delegate void OnConfigLoadSuccessDelegate();

        /**
         * <summary><see cref="OnConfigLoadSuccessDelegate"/></summary>
         */
        public static OnConfigLoadSuccessDelegate onConfigLoadSuccess;

        /**
         * <summary>
         * Dispatched in response to the <see cref="SmartFoxClient.Connect(string, int)"/> request.<br/>
         * The connection to SmartFoxServer may have succeeded or failed: the <i>success</i> parameter must be checked.
         * </summary>
         * 
         * <param name="success">the connection result: <c>true</c> if the connection succeeded, <c>false</c> if the connection failed.</param>
         * <param name="error">the error message in case of connection failure.</param>
         * 
         * <example>The following example shows how to handle the connection result.
         * 			<code>
         * 			SFSEvent.onConnection += OnConnection;
         *						
         *			smartFox.Connect("127.0.0.1", 9339);
         *					
         *			public void OnConnection(bool success, string error)
         *			{
         *				if (success)
         *					Trace.WriteLine("Connection successful");
         *				else
         *					Trace.WriteLine("Connection failed. Reason: " + error);
         *			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="SmartFoxClient.Connect(string, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnConnectionDelegate(bool success, string error);

        /**
         * <summary><see cref="OnConnectionDelegate"/></summary>
         */
        public static OnConnectionDelegate onConnection;

        /**
         * <summary>
         * Dispatched when the connection with SmartFoxServer is closed (either from the client or from the server).
         * </summary>
         * 
         * <example>The following example shows how to handle a "connection lost" event.
         * 			<code>
         * 			SFSEvent.onConnectionLost += OnConnectionLost;
         * 			
         * 			public void OnConnectionLost()
         * 			{
         * 				Trace.WriteLine("Connection lost!");
         * 				
         * 				// TODO: disable application interface
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SmartFoxClient.Disconnect"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnConnectionLostDelegate();

        /**
         * <summary><see cref="OnConnectionLostDelegate"/></summary>
         */
        public static OnConnectionLostDelegate onConnectionLost;

        /**
         * <summary>
         * Dispatched when an error occurs during the creation of a room.<br/>
         * Usually this happens when a client tries to create a room but its name is already taken.
         * </summary>
         * 
         * <param name="error">the error message.</param>
         * 
         * <example>The following example shows how to handle a potential error in room creation.
         * 			<code>
         * 			SFSEvent.onCreateRoomError += OnCreateRoomError;
         * 
         * 			NewRoomDescriptor roomObj = new NewRoomDescriptor("The Cave", 15, true);
         * 			smartFox.CreateRoom(roomObj);
         * 			
         * 			public void OnCreateRoomError(string error)
         * 			{
         * 				Trace.WriteLine("Room creation error; the following error occurred: " + error);
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="SmartFoxClient.CreateRoom(NewRoomDescriptor, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnCreateRoomErrorDelegate(string error);

        /**
         * <summary><see cref="OnCreateRoomErrorDelegate"/></summary>
         */
        public static OnCreateRoomErrorDelegate onCreateRoomError;

        /**
         * <summary>
         * Dispatched when a debug message is traced by the SmartFoxServer API.<br/>
         * In order to receive this event you have to previously set the <see cref="SmartFoxClient.debug"/> flag to <c>true</c>.
         * </summary>
         * 
         * <param name="message">the debug message.</param>
         * 
         * <example>The following example shows how to handle a SmartFoxServer API debug message.
         * 			<code>
         * 			SFSEvent.onDebugMessage += OnDebugMessage;
         * 			
         * 			smartFox.debug = true;
         * 			
         * 			public void OnDebugMessage(string message)
         * 			{
         * 				Trace.WriteLine("[SFS DEBUG] " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SmartFoxClient.debug"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnDebugMessageDelegate(string message);

        /**
         * <summary><see cref="OnDebugMessageDelegate"/></summary>
         */
        public static OnDebugMessageDelegate onDebugMessage;

        /**
         * <summary>
         * Dispatched when a command/response from a server-side extension is received.
         * </summary>
         * 
         * <param name="dataObj">an object containing all the data sent by the server-side extension; by convention, a string property called <b>_cmd</b> should always be present, to distinguish between different responses coming from the same extension.</param>
         * <param name="type">one of the following response protocol types: <see cref="SmartFoxClient.XTMSG_TYPE_XML"/>, <see cref="SmartFoxClient.XTMSG_TYPE_STR"/>, <see cref="SmartFoxClient.XTMSG_TYPE_JSON"/>. By default <see cref="SmartFoxClient.XTMSG_TYPE_XML"/> is used.</param>
         * 
         * <example>The following example shows how to handle an extension response.
         * 			<code>
         * 			SFSEvent.onExtensionResponse += OnExtensionResponse;
         * 			
         * 			public void OnExtensionResponse(object data, string type)
         * 			{
         * 				// Handle XML responses
         * 				if (type == SmartFoxClient.XTMSG_TYPE_XML)
         * 				{
         * 					SFSObject responseData = (SFSObject)data;
         * 					// TODO: check command and perform required actions
         * 				}
         * 				
         * 				// Handle RAW responses
         * 				else if (type == SmartFoxClient.XTMSG_TYPE_STR)
         * 				{
         * 					string responseData = (string)data;
         * 					// TODO: check command and perform required actions
         * 				}
         * 				
         * 				// Handle JSON responses
         * 				else if (type == SmartFoxClient.XTMSG_TYPE_JSON)
         * 				{
         * 					JsonData responseData = (JsonData)data;
         * 					// TODO: check command and perform required actions
         * 				}
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SmartFoxClient.XTMSG_TYPE_XML"/>
         * <seealso cref="SmartFoxClient.XTMSG_TYPE_STR"/>
         * <seealso cref="SmartFoxClient.XTMSG_TYPE_JSON"/>
         * <seealso cref="SmartFoxClient.SendXtMessage(string, string, ICollection&lt;object&gt;, string, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public delegate void OnExtensionResponseDelegate(object dataObj, string type);

        /**
         * <summary><see cref="OnExtensionResponseDelegate"/></summary>
         */
        public static OnExtensionResponseDelegate onExtensionResponse;

        /**
         * <summary>
         * Dispatched when a room is joined successfully.
         * </summary>
         * 
         * <param name="room">the <see cref="Room"/> object representing the joined room.</param>
         * 
         * <example>The following example shows how to handle an successful room joining.
         * 			<code>
         * 			SFSEvent.onJoinRoom += OnJoinRoom;
         * 			
         * 			smartFox.JoinRoom("The Entrance");
         * 			
         * 			public void OnJoinRoom(Room room)
         * 			{
         * 				Trace.WriteLine("Room " + room.GetName() + " joined successfully");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnJoinRoomErrorDelegate"/>
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.JoinRoom(object, string, bool, bool, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnJoinRoomDelegate(Room room);

        /**
         * <summary><see cref="OnJoinRoomDelegate"/></summary>
         */
        public static OnJoinRoomDelegate onJoinRoom;

        /**
         * <summary>
         * Dispatched when an error occurs while joining a room.<br/>
         * This error could happen, for example, if the user is trying to join a room which is currently full.
         * </summary>
         * 
         * <param name="error">the error message.</param>
         * 
         * <example>The following example shows how to handle a potential error in room joining.
         * 			<code>
         * 			SFSEvent.onJoinRoomError += OnJoinRoomError;
         * 			
         * 			smartFox.JoinRoom("The Entrance");
         * 			
         * 			public void OnJoinRoomError(string error)
         * 			{
         * 				Trace.WriteLine("Room join error; the following error occurred: " + error);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnJoinRoomDelegate"/>
         * <seealso cref="SmartFoxClient.JoinRoom(object, string, bool, bool, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnJoinRoomErrorDelegate(string error);

        /**
         * <summary><see cref="OnJoinRoomErrorDelegate"/></summary>
         */
        public static OnJoinRoomErrorDelegate onJoinRoomError;

        /**
         * <summary>
         * Dispatched when the login to a SmartFoxServer zone has been attempted.
         * </summary>
         * 
         * <param name="success">the login result: <c>true</c> if the login to the provided zone succeeded; <c>false</c> if login failed.</param>
         * <param name="name">the user's actual username.</param>
         * <param name="error">the error message in case of login failure.</param>
         * 
         * <example>The following example shows how to handle the login result.
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
         * <seealso cref="OnLogoutDelegate"/>
         * <seealso cref="SmartFoxClient.Login"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * the server sends the username back to the client because not all usernames are valid: for example, those containing bad words may have been filtered during the login process.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnLoginDelegate(bool success, string name, string error);

        /**
         * <summary><see cref="OnLoginDelegate"/></summary>
         */
        public static OnLoginDelegate onLogin;

        /**
         * <summary>
         * Dispatched when the user logs out successfully.<br/>
         * After a successful logout the user is still connected to the server, but he/she has to login again into a zone, in order to be able to interact with the server.
         * </summary>
         * 
         * <example>The following example shows how to handle the "logout" event.
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
         * <seealso cref="OnLoginDelegate"/>
         * <seealso cref="SmartFoxClient.Logout"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.5.5</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnLogoutDelegate();

        /**
         * <summary><see cref="OnLogoutDelegate"/></summary>
         */
        public static OnLogoutDelegate onLogout;

        /**
         * <summary>
         * Dispatched when a message from a Moderator is received.
         * </summary>
         * 
         * <param name="message">the Moderator's message.</param>
         * <param name="sender">the <see cref="User"/> object representing the Moderator.</param>
         * 
         * <example>The following example shows how to handle a message coming from a Moderator.
         * 			<code>
         * 			SFSEvent.onModeratorMessage += OnModeratorMessage;
         * 			
         * 			public void OnModeratorMessage(string message, User sender)
         * 			{
         * 				Trace.WriteLine("Moderator " + sender.GetName() + " said: " + message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnAdminMessageDelegate"/>
         * <seealso cref="User"/>
         * <seealso cref="SmartFoxClient.SendModeratorMessage(string, string, int)"/>
         * 
         * <remarks>
         * <para><b>Since:</b><br/>
         * SmartFoxServer Pro v1.4.5</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnModeratorMessageDelegate(string message, User sender);

        /**
         * <summary><see cref="OnModeratorMessageDelegate"/></summary>
         */
        public static OnModeratorMessageDelegate onModeratorMessage;

        /**
         * <summary>
         * Dispatched when an SFSObject is received.
         * </summary>
         * 
         * <param name="obj">the <see cref="SFSObject"/> object received.</param>
         * <param name="sender">the <see cref="User"/> object representing the user that sent the SFSObject.</param>
         * 
         * <example>The following example shows how to handle an Actionscript object received from a user.
         * 			<code>
         * 			SFSEvent.onObjectReceived += OnObjectReceived;
         * 			
         * 			public void OnObjectReceived(SFSObject obj, User sender)
         * 			{
         * 				// Assuming another client sent his X and Y positions in two properties called px, py
         * 				Trace.WriteLine("Data received from user: " + sender.GetName());
         * 				Trace.WriteLine("X = " + obj.GetString("px") + ", Y = " + obj.GetString("py"));
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="SFSObject"/>
         * <seealso cref="User"/>
         * <seealso cref="SmartFoxClient.SendObject(SFSObject, int)"/>
         * <seealso cref="SmartFoxClient.SendObjectToGroup(SFSObject, List&lt;int&gt;, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnObjectReceivedDelegate(SFSObject obj, User sender);

        /**
         * <summary><see cref="OnObjectReceivedDelegate"/></summary>
         */
        public static OnObjectReceivedDelegate onObjectReceived;

        /**
         * <summary>
         * Dispatched in response to the <see cref="SmartFoxClient.SwitchPlayer(int)"/> request.<br/>
         * The request to turn a player into a spectator may fail if another user did the same before your request, and there was only one spectator slot available.
         * </summary>
         * 
         * <param name="success">the switch result: <c>true</c> if the player was turned into a spectator, otherwise <c>false</c>.</param>
         * <param name="newId">the player id assigned by the server to the user.</param>
         * <param name="room">the <see cref="Room"/> object representing the room where the switch occurred.</param>
         * 
         * <example>The following example shows how to check the handle the player switch.
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
         * 					Trace.WriteLine("The attempt to switch from player to spectator failed");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="User.GetPlayerId"/>
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.SwitchPlayer(int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public delegate void OnPlayerSwitchedDelegate(bool success, int newId, Room room);

        /**
         * <summary><see cref="OnPlayerSwitchedDelegate"/></summary>
         */
        public static OnPlayerSwitchedDelegate onPlayerSwitched;

        /**
         * <summary>
         * Dispatched when a private chat message is received.
         * </summary>
         * 
         * <param name="message">the private message received.</param>
         * <param name="sender">the <see cref="User"/> object representing the user that sent the message; this property is undefined if the sender isn't in the same room of the recipient.</param>
         * <param name="roomId">the id of the room where the sender is.</param>
         * <param name="userId">the user id of the sender (useful in case of private messages across different rooms, when the <c>sender</c> object is not available).</param>
         * 
         * <example>The following example shows how to handle a private message.
         * 			<code>
         * 			SFSEvent.onPrivateMessage += OnPrivateMessage;
         * 			
         * 			smartFox.SendPrivateMessage("Hallo Jack!", 22);
         * 			
         * 			public void OnPrivateMessage(string message, User sender, int roomId, int userId)
         * 			{
         * 				Trace.WriteLine("User " + sender.GetName() + " sent the following private message: " + .message);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnPublicMessageDelegate"/>
         * <seealso cref="User"/>
         * <seealso cref="SmartFoxClient.SendPrivateMessage(string, int, int)"/>
         * 
         * <remarks>
         * <para><b>History:</b><br/>
         * SmartFoxServer Pro v1.5.0 - <i>roomId</i> and <i>userId</i> parameters added.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnPrivateMessageDelegate(string message, User sender, int roomId, int userId);

        /**
         * <summary><see cref="OnPrivateMessageDelegate"/></summary>
         */
        public static OnPrivateMessageDelegate onPrivateMessage;

        /**
         * <summary>
         * Dispatched when a public chat message is received.
         * </summary>
         * 
         * <param name="message">the public message received.</param>
         * <param name="sender">the <see cref="User"/> object representing the user that sent the message.</param>
         * <param name="roomId">the id of the room where the sender is.</param>
         * 
         * @example	The following example shows how to handle a public message.
         * 			<code>
         * 			SFSEvent.onPublicMessage += OnPublicMessage;
         * 			
         * 			smartFox.SendPublicMessage("Hello world!");
         * 			
         * 			public void OnPublicMessage(string message, User sender, int roomId)
         * 			{
         * 				Trace.WriteLine("User " + sender.GetName() + " said: " + message);
         * 			}
         * 			</code>
         * 
         * <seealso cref="OnPrivateMessageDelegate"/>
         * <seealso cref="User"/>
         * <seealso cref="SmartFoxClient.SendPublicMessage(string, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnPublicMessageDelegate(string message, User sender, int roomId);

        /**
         * <summary><see cref="OnPublicMessageDelegate"/></summary>
         */
        public static OnPublicMessageDelegate onPublicMessage;

        /**
         * <summary>
         * Dispatched in response to a <see cref="SmartFoxClient.GetRandomKey"/> request.
         * </summary>
         * 
         * <param name="key">a unique random key generated by the server.</param>
         * 
         * <example>The following example shows how to handle the key received from the server.
         * 			<code>
         * 			SFSEvent.onRandomKey += OnRandomKey;
         * 			
         * 			smartFox.GetRandomKey();
         * 			
         * 			public void OnRandomKey(string key)
         * 			{
         * 				Trace.WriteLine("Random key received from server: " + key);
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SmartFoxClient.GetRandomKey"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Pro</para>
         * </remarks>
         */
        public delegate void OnRandomKeyDelegate(string key);

        /**
         * <summary><see cref="OnRandomKeyDelegate"/></summary>
         */
        public static OnRandomKeyDelegate onRandomKey;

        /**
         * <summary>
         * Dispatched when a new room is created in the zone where the user is currently logged in.
         * </summary>
         * 
         * <param name="room">the <see cref="Room"/> object representing the room that was created.</param>
         * 
         * <example>The following example shows how to handle a new room being created in the zone.
         * 			<code>
         * 			SFSEvent.onRoomAdded += OnRoomAdded;
         * 			
         * 			NewRoomDescriptor roomObj = new NewRoomDescriptor("The Cave", 15, true);
         * 			smartFox.CreateRoom(roomObj);
         * 			
         * 			public void OnRoomAdded(Room room)
         * 			{
         * 				Trace.WriteLine("Room " + room.GetName() + " was created");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnRoomDeletedDelegate"/>
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.CreateRoom(NewRoomDescriptor, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnRoomAddedDelegate(Room room);

        /**
         * <summary><see cref="OnRoomAddedDelegate"/></summary>
         */
        public static OnRoomAddedDelegate onRoomAdded;

        /**
         * <summary>
         * Dispatched when a room is removed from the zone where the user is currently logged in.
         * </summary>
         * 
         * <param name="room">the <see cref="Room"/> object representing the room that was removed.</param>
         * 
         * <example>The following example shows how to handle a new room being removed in the zone.
         * 			<code>
         * 			SFSEvent.onRoomDeleted += OnRoomDeleted;
         * 			
         * 			public void OnRoomDeleted(Room room)
         * 			{
         * 				Trace.WriteLine("Room " + room.GetName() + " was removed");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnRoomAddedDelegate"/>
         * <seealso cref="Room"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnRoomDeletedDelegate(Room room);

        /**
         * <summary><see cref="OnRoomDeletedDelegate"/></summary>
         */
        public static OnRoomDeletedDelegate onRoomDeleted;

        /**
         * <summary>
         * Dispatched when a room is left in multi-room mode, in response of a <see cref="SmartFoxClient.LeaveRoom"/> request.
         * </summary>
         * 
         * <param name="roomId">the id of the room that was left.</param>
         * 
         * <example>The following example shows how to handle the "room left" event.
         * 			<code>
         * 			SFSEvent.onRoomLeft += OnRoomLeft;
         * 			
         * 			public void OnRoomLeft(int roomId)
         * 			{
         * 				Trace.WriteLine("You left room " + roomId);
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="SmartFoxClient.LeaveRoom"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnRoomLeftDelegate(int roomId);

        /**
         * <summary><see cref="OnRoomLeftDelegate"/></summary>
         */
        public static OnRoomLeftDelegate onRoomLeft;

        /**
         * <summary>
         * Dispatched when the list of rooms available in the current zone is received.<br/>
         * If the default login mechanism provided by SmartFoxServer is used, then this event is dispatched right after a successful login.<br/>
         * This is because the SmartFoxServer API, internally, call the <see cref="SmartFoxClient.GetRoomList"/> method after a successful login is performed.<br/>
         * If a custom login handler is implemented, the room list must be manually requested to the server by calling the mentioned method.
         * </summary>
         * 
         * <param name="roomList">a list of <see cref="Room"/> objects for the zone logged in by the user.</param>
         * 
         * <example>The following example shows how to handle the list of rooms sent by SmartFoxServer.
         * 			<code>
         * 			SFSEvent.onRoomListUpdate += OnRoomListUpdate;
         * 			
         * 			smartFox.Login("simpleChat", "jack");
         * 			
         * 			public void OnRoomListUpdate(Dictionary&lt;int, Room&gt; roomList)
         * 			{
         * 				// Dump the names of the available rooms in the "simpleChat" zone
         * 				foreach (Room room in roomList.Values)
         * 					Trace.WriteLine(room.GetName());
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.GetRoomList"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnRoomListUpdateDelegate(Dictionary<int, Room> roomList);

        /**
         * <summary><see cref="OnRoomListUpdateDelegate"/></summary>
         */
        public static OnRoomListUpdateDelegate onRoomListUpdate;

        /**
         * <summary>
         * Dispatched when Room Variables are updated.<br/>
         * A user receives this notification only from the room(s) where he/she is currently logged in. Also, only the variables that changed are transmitted.
         * </summary>
         * 
         * <param name="room">the <see cref="Room"/> object representing the room where the update took place.</param>
         * <param name="changedVars">a dictionary with the names of the changed variables as keys.</param>
         * 
         * <example>The following example shows how to handle an update in Room Variables.
         * 			<code>
         * 			SFSEvent.onRoomVariablesUpdate += OnRoomVariablesUpdate;
         * 			
         * 			public void OnRoomVariablesUpdate(Room room, Dictionary&lt;string, object&gt; changedVars)
         * 			{
         * 				// Iterate on the 'changedVars' Hashtable to check which variables were updated
         * 				foreach (string v in changedVars.Keys)
         * 					Trace.WriteLine(v + " room variable was updated; new value is: " + room.getVariable(v));
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.SetRoomVariables(List&lt;RoomVariable&gt;, int, bool)"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * the <c>changedVars</c> array contains the names of the changed variables only, not the actual values. To retrieve them the <see cref="Room.GetVariable"/> / <see cref="Room.GetVariables"/> methods can be used.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnRoomVariablesUpdateDelegate(Room room, Dictionary<string, object> changedVars);

        /**
         * <summary><see cref="OnRoomVariablesUpdateDelegate"/></summary>
         */
        public static OnRoomVariablesUpdateDelegate onRoomVariablesUpdate;

        /**
         * <summary>
         * Dispatched when a response to the <see cref="SmartFoxClient.RoundTripBench"/> request is received.<br/>
         * The "roundtrip time" represents the number of milliseconds that it takes to a message to go from the client to the server and back to the client.<br/>
         * A good way to measure the network lag is to send continuos requests (every 3 or 5 seconds) and then calculate the average roundtrip time on a fixed number of responses (i.e. the last 10 measurements).
         * </summary>
         * 
         * <param name="elapsed">the roundtrip time.</param>
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
         * 				int time = elapsed;
         * 				
         * 				// We assume that it takes the same time to the ping message to go from the client to the server
         * 				// and from the server back to the client, so we divide the elapsed time by 2.
         * 				totalPingTime += time / 2;
         * 				pingCount++;
         * 				
         * 				int avg = Math.Round(totalPingTime / pingCount);
         * 				
         * 				Trace.WriteLine("Average lag: " + avg + " milliseconds");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="SmartFoxClient.RoundTripBench"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnRoundTripResponseDelegate(int elapsed);

        /**
         * <summary><see cref="OnRoundTripResponseDelegate"/></summary>
         */
        public static OnRoundTripResponseDelegate onRoundTripResponse;

        /**
         * <summary>
         * Dispatched in response to the <see cref="SmartFoxClient.SwitchSpectator(int)"/> request.<br/>
         * The request to turn a spectator into a player may fail if another user did the same before your request, and there was only one player slot available.
         * </summary>
         * 
         * <param name="success">the switch result: <c>true</c> if the spectator was turned into a player, otherwise <c>false</c>.</param>
         * <param name="newId">the player id assigned by the server to the user.</param>
         * <param name="room">the <see cref="Room"/> object representing the room where the switch occurred.</param>
         * 
         * <example>The following example shows how to check the handle the spectator switch.
         * 			<code>
         * 			SFSEvent.onSpectatorSwitched += OnSpectatorSwitched;
         * 			
         * 			smartFox.SwitchSpectator();
         * 			
         * 			public void OnSpectatorSwitched(bool success, int newId, Room room)
         * 			{
         * 				if (success)
         * 					Trace.WriteLine("You have been turned into a player; your id is " + newId);
         * 				else
         * 					Trace.WriteLine("The attempt to switch from spectator to player failed");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="User.GetPlayerId"/>
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.SwitchSpectator(int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnSpectatorSwitchedDelegate(bool success, int newId, Room room);

        /**
         * <summary><see cref="OnSpectatorSwitchedDelegate"/></summary>
         */
        public static OnSpectatorSwitchedDelegate onSpectatorSwitched;

        /**
         * <summary>
         * Dispatched when the number of users and/or spectators changes in a room within the current zone.<br/>
         * This event allows to keep track in realtime of the status of all the zone rooms in terms of users and spectators.<br/>
         * In case many rooms are used and the zone handles a medium to high traffic, this notification can be turned off to reduce bandwidth consumption, since a message is broadcasted to all users in the zone each time a user enters or exits a room.
         * </summary>
         * 
         * <param name="room">the <see cref="Room"/> object representing the room where the change occurred.</param>
         * 
         * <example>The following example shows how to check the handle the spectator switch notification.
         * 			<code>
         * 			SFSEvent.onUserCountChange += OnUserCountChange;
         * 			
         * 			public void OnUserCountChange(Room room)
         * 			{
         * 				// Assuming this is a game room
         * 				
         * 				string roomName = room.GetName()
         * 				int playersNum = room.GetUserCount()
         * 				int spectatorsNum: = room.GetSpectatorCount()
         * 				
         * 				Trace.WriteLine("Room " + roomName + "has " + playersNum + " players and " + spectatorsNum + " spectators");
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="OnUserEnterRoomDelegate"/>
         * <seealso cref="OnUserLeaveRoomDelegate"/>
         * <seealso cref="Room"/>
         * <seealso cref="SmartFoxClient.CreateRoom(NewRoomDescriptor, int)"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnUserCountChangeDelegate(Room room);

        /**
         * <summary><see cref="OnUserCountChangeDelegate"/></summary>
         */
        public static OnUserCountChangeDelegate onUserCountChange;

        /**
         * <summary>
         * Dispatched when another user joins the current room.
         * </summary>
         * 
         * <param name="roomId">the id of the room joined by a user (useful in case multi-room presence is allowed).</param>
         * <param name="user">the <see cref="User"/> object representing the user that joined the room.</param>
         * 
         * <example>The following example shows how to check the handle the user entering room notification.
         * 			<code>
         * 			SFSEvent.onUserEnterRoom += OnUserEnterRoom;
         * 			
         * 			public void OnUserEnterRoom(int roomId, User user)
         * 			{
         * 				Trace.WriteLine("User " + user.GetName() + " entered the room");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnUserLeaveRoomDelegate"/>
         * <seealso cref="OnUserCountChangeDelegate"/>
         * <seealso cref="User"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnUserEnterRoomDelegate(int roomId, User user);

        /**
         * <summary><see cref="OnUserEnterRoomDelegate"/></summary>
         */
        public static OnUserEnterRoomDelegate onUserEnterRoom;

        /**
         * <summary>
         * Dispatched when a user leaves the current room.<br/>
         * This event is also dispatched when a user gets disconnected from the server.
         * </summary>
         * 
         * <param name="roomId">the id of the room left by a user (useful in case multi-room presence is allowed).</param>
         * <param name="userId">the id of the user that left the room (or got disconnected).</param>
         * <param name="userName">the name of the user.</param>
         * 
         * <example>The following example shows how to check the handle the user leaving room notification.
         * 			<code>
         * 			SFSEvent.onUserLeaveRoom += OnUserLeaveRoom;
         * 			
         * 			public void OnUserLeaveRoom(int roomId, int userId, string userName)
         * 			{
         * 				Trace.WriteLine("User " + userName + " left the room");
         * 			}
         * 			</code>
         * </example>
         * 
         * <seealso cref="OnUserEnterRoomDelegate"/>
         * <seealso cref="OnUserCountChangeDelegate"/>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnUserLeaveRoomDelegate(int roomId, int userId, string userName);

        /**
         * <summary><see cref="OnUserLeaveRoomDelegate"/></summary>
         */
        public static OnUserLeaveRoomDelegate onUserLeaveRoom;

        /**
         * <summary>
         * Dispatched when a user in the current room updates his/her User Variables.
         * </summary>
         * 
         * <param name="user">the <see cref="User"/> object representing the user who updated his/her variables.</param>
         * <param name="changedVars">a dictionary with the names of the changed variables as keys.</param>
         * 
         * <example>The following example shows how to handle an update in User Variables.
         * 			<code>
         * 			SFSEvent.onUserVariablesUpdate += OnUserVariablesUpdate;
         * 			
         * 			public void OnUserVariablesUpdate(User user, Dictionary&lt;string, object&gt; changedVars)
         * 			{
         * 				// We assume that each user has px and py variables representing the users's avatar coordinates in a 2D environment
         * 				
         * 				if (changedVars["px"] != null || changedVars["py"] != null)
         * 				{
         * 					Trace.WriteLine("User " + user.GetName() + " moved to new coordinates:");
         * 					Trace.WriteLine("\t px: " + user.GetVariable("px"));
         * 					Trace.WriteLine("\t py: " + user.GetVariable("py"));
         * 				}
         * 			}
         * 			</code>
         * 	</example>
         * 
         * <seealso cref="User"/>
         * <seealso cref="SmartFoxClient.SetUserVariables(Dictionary&lt;string, object&gt;, int)"/>
         * 
         * <remarks>
         * <para><b>NOTE:</b><br/>
         * the <c>changedVars</c> dictionary contains the names of the changed variables only, not the actual values. To retrieve them the <see cref="User.GetVariable"/> / <see cref="User.GetVariables"/> methods can be used.</para>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public delegate void OnUserVariablesUpdateDelegate(User user, Dictionary<string, object> changedVars);

        /**
         * <summary><see cref="OnUserVariablesUpdateDelegate"/></summary>
         */
        public static OnUserVariablesUpdateDelegate onUserVariablesUpdate;

        #endregion

        #region SFSEvent Constructor and Accessors

        private string type;
        private Dictionary<string, object> parameters;

        /**
         * <summary>SFSEvent contructor.</summary>
         * 
         * <param name="type">the event's type (see the constants in this class).</param>
         * <param name="parameters">the parameters object for the event.</param>
         * @exclude
         */
        public SFSEvent(string type, Dictionary<string, object> parameters)
        {
            this.type = type;
            this.parameters = parameters;
        }

        /**
         * <summary>
         * Get type of event
         * </summary>
         * 
         * <returns>Type of event</returns>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public new string GetType()
        {
            return type;
        }

        /**
         * <summary>
         * Get a specific parameter for the event
         * </summary>
         * 
         * <returns>Requested parameter</returns>
         * 
         * <remarks>
         * <para><b>Version:</b><br/>
         * SmartFoxServer Basic / Pro</para>
         * </remarks>
         */
        public object GetParameter(string key)
        {
            return parameters[key];
        }

        #endregion
    }
}