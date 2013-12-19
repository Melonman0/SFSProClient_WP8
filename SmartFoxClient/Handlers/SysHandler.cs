using SmartFoxClientAPI.Data;
using SmartFoxClientAPI.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SmartFoxClientAPI.Handlers
{
    /**
     * <summary>SysHandler class: handles "sys" type messages.</summary>
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
    public class SysHandler : IMessageHandler
    {
        private SmartFoxClient sfs;

        private const int DEFAULT_INT = -1;
        private const bool DEFAULT_BOOL = false;
        private const string DEFAULT_STRING = "undefined";

        /**
         * <summary>
         * SysHandler constructor.
         * </summary>
         * 
         * <param name="sfs">the smart fox client</param>
         */
        public SysHandler(SmartFoxClient sfs)
        {
            this.sfs = sfs;
        }

        /**
         * <summary>
         * Handle messages
         * </summary>
         * 
         * <param name="msgObj">the message object to handle</param>
         * <param name="type">type of message</param>
         */
        public void HandleMessage(object msgObj, string type)
        {
            XDocument xmlDoc = (XDocument)msgObj;
            XElement xml = xmlDoc.Element("msg");
            string action = "UNDEFINED";
            try
            {
                action = xml.Element("body").Attribute("action").Value;
            }
            catch
            {
            }

            // Send message to handler method
            switch (action)
            {
                case "apiOK":
                    this.HandleApiOK(xml);
                    break;
                case "apiKO":
                    this.HandleApiKO(xml);
                    break;
                case "logOK":
                    this.HandleLoginOk(xml);
                    break;
                case "logKO":
                    this.HandleLoginKo(xml);
                    break;
                case "logout":
                    this.HandleLogout(xml);
                    break;
                case "rmList":
                    this.HandleRoomList(xml);
                    break;
                case "uCount":
                    this.HandleUserCountChange(xml);
                    break;
                case "joinOK":
                    this.HandleJoinOk(xml);
                    break;
                case "joinKO":
                    this.HandleJoinKo(xml);
                    break;
                case "uER":
                    this.HandleUserEnterRoom(xml);
                    break;
                case "userGone":
                    this.HandleUserLeaveRoom(xml);
                    break;
                case "pubMsg":
                    this.HandlePublicMessage(xml);
                    break;
                case "prvMsg":
                    this.HandlePrivateMessage(xml);
                    break;
                case "dmnMsg":
                    this.HandleAdminMessage(xml);
                    break;
                case "modMsg":
                    this.HandleModMessage(xml);
                    break;
                case "dataObj":
                    this.HandleSFSObject(xml);
                    break;
                case "rVarsUpdate":
                    this.HandleRoomVarsUpdate(xml);
                    break;
                case "roomAdd":
                    this.HandleRoomAdded(xml);
                    break;
                case "roomDel":
                    this.HandleRoomDeleted(xml);
                    break;
                case "rndK":
                    this.HandleRandomKey(xml);
                    break;
                case "roundTripRes":
                    this.HandleRoundTripBench(xml);
                    break;
                case "uVarsUpdate":
                    this.HandleUserVarsUpdate(xml);
                    break;
                case "createRmKO":
                    this.HandleCreateRoomError(xml);
                    break;
                case "bList":
                    this.HandleBuddyList(xml);
                    break;
                case "bUpd":
                    this.HandleBuddyListUpdate(xml);
                    break;
                case "bAdd":
                    this.HandleBuddyAdded(xml);
                    break;
                case "roomB":
                    this.HandleBuddyRoom(xml);
                    break;
                case "leaveRoom":
                    this.HandleLeaveRoom(xml);
                    break;
                case "swSpec":
                    this.HandleSpectatorSwitched(xml);
                    break;
                case "bPrm":
                    this.HandleAddBuddyPermission(xml);
                    break;
                case "remB":
                    this.HandleRemoveBuddy(xml);
                    break;
                case "swPl":
                    this.HandlePlayerSwitched(xml);
                    break;
                default:
                    sfs.DebugMessage("Unknown sys command: " + action); // TODO - remove?
                    break;
            }
        }

        /**
         * <summary>
         * Handle correct API
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleApiOK(XElement xml)
        {
            sfs.SetIsConnected(true);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("success", true);
            parameters.Add("error", null);
            SFSEvent evt = new SFSEvent(SFSEvent.onConnectionEvent, parameters);
            sfs.EnqueueEvent(evt);
        }


        /**
         * <summary>
         * Handle obsolete API
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleApiKO(XElement xml)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("success", false);
            parameters.Add("error", "API are obsolete, please upgrade");

            SFSEvent evt = new SFSEvent(SFSEvent.onConnectionEvent, parameters);
            sfs.EnqueueEvent(evt);
        }


        /**
         * <summary>
         * Handle successful login
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleLoginOk(XElement xml)
        {
            int uid = DEFAULT_INT;
            try
            {
                uid = int.Parse(xml.Element("body").Element("login").Attribute("id").Value);
            }
            catch { }

            int mod = DEFAULT_INT;
            try
            {
                mod = int.Parse(xml.Element("body").Element("login").Attribute("mod").Value);
            }
            catch { }

            string name = DEFAULT_STRING;
            try
            {
                name = xml.Element("body").Element("login").Attribute("n").Value;
            }
            catch { }

            sfs.amIModerator = (mod == 1);
            sfs.myUserId = uid;
            sfs.myUserName = name;
            sfs.playerId = -1;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("success", true);
            parameters.Add("name", name);
            parameters.Add("error", "");

            // Request room list
            sfs.GetRoomList();

            SFSEvent evt = new SFSEvent(SFSEvent.onLoginEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle unsuccessful login
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleLoginKo(XElement xml)
        {
            string error = DEFAULT_STRING;
            try
            {
                error = xml.Element("body").Element("login").Attribute("e").Value;
            }
            catch { }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("success", false);
            parameters.Add("name", error);
            parameters.Add("error", error);

            SFSEvent evt = new SFSEvent(SFSEvent.onLoginEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle successful logout
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleLogout(XElement xml)
        {
            sfs.__Logout();

            SFSEvent evt = new SFSEvent(SFSEvent.onLogoutEvent, null);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Populate the room list for this zone and fire the event
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleRoomList(XElement xml)
        {
            sfs.ClearRoomList();

            Dictionary<int, Room> roomList = sfs.GetAllRooms();
            Dictionary<int, Room> tempList = new Dictionary<int, Room>();

            foreach (XElement roomXml in xml.Element("body").Element("rmList").Elements("rm"))
            {
                int roomId = DEFAULT_INT;
                try
                {
                    roomId = int.Parse(roomXml.Attribute("id").Value);
                }
                catch { }

                string name = DEFAULT_STRING;
                try
                {
                    name = roomXml.Element("n").Value;
                }
                catch { }

                int maxu = DEFAULT_INT;
                try
                {
                    maxu = int.Parse(roomXml.Attribute("maxu").Value);
                }
                catch { }

                int maxs = DEFAULT_INT;
                try
                {
                    maxs = int.Parse(roomXml.Attribute("maxs").Value);
                }
                catch { }

                bool temp = DEFAULT_BOOL;
                try
                {
                    temp = bool.Parse(roomXml.Attribute("temp").Value);
                }
                catch { }

                bool game = DEFAULT_BOOL;
                try
                {
                    game = bool.Parse(roomXml.Attribute("game").Value);
                }
                catch { }

                bool priv = DEFAULT_BOOL;
                try
                {
                    priv = bool.Parse(roomXml.Attribute("priv").Value);
                }
                catch { }

                bool lmb = DEFAULT_BOOL;
                try
                {
                    lmb = bool.Parse(roomXml.Attribute("lmb").Value);
                }
                catch { }

                int ucnt = DEFAULT_INT;
                try
                {
                    ucnt = int.Parse(roomXml.Attribute("ucnt").Value);
                }
                catch { }

                int scnt = DEFAULT_INT;
                try
                {
                    scnt = int.Parse(roomXml.Attribute("scnt").Value);
                }
                catch { }

                Room room = new Room(roomId,
                                        name,
                                        maxu,
                                        maxs,
                                        temp,
                                        game,
                                        priv,
                                        lmb,
                                        ucnt,
                                        scnt
                                    );

                // Handle Room Variables
                if (roomXml.Element("vars") != null)
                {
                    PopulateVariables(room.GetVariables(), roomXml);
                }

                // Add room
                tempList[roomId] = room;
            }

            sfs.SetRoomList(tempList);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("roomList", tempList);

            SFSEvent evt = new SFSEvent(SFSEvent.onRoomListUpdateEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle the user count change in a room
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleUserCountChange(XElement xml)
        {
            int uCount = DEFAULT_INT;
            try
            {
                uCount = int.Parse(xml.Element("body").Attribute("u").Value);
            }
            catch { }

            int sCount = DEFAULT_INT;
            try
            {
                sCount = int.Parse(xml.Element("body").Attribute("s").Value);
            }
            catch { }

            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            Room room = (Room)sfs.GetAllRooms()[roomId];

            if (room != null)
            {
                room.SetUserCount(uCount);
                room.SetSpectatorCount(sCount);

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("room", room);

                SFSEvent evt = new SFSEvent(SFSEvent.onUserCountChangeEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }


        /**
         * <summary>
         * Successfull room Join
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleJoinOk(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            XElement userListXml = xml.Element("body").Element("uLs");

            int playerId = DEFAULT_INT;
            try
            {
                playerId = int.Parse(xml.Element("body").Element("pid").Attribute("id").Value);
            }
            catch { }

            // Set current active room
            sfs.activeRoomId = roomId;

            // get current Room and populates usrList
            Room currRoom = sfs.GetRoom(roomId);

            if (currRoom == null)
            {
                sfs.DebugMessage("WARNING! JoinOk tries to join an unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                // Clear the old data, we need to start from a clean list
                currRoom.ClearUserList();

                // Set the player ID
                // -1 = no game room
                sfs.playerId = playerId;

                // Also set the myPlayerId in the room
                // for multi-room applications
                currRoom.SetMyPlayerIndex(playerId);

                // Handle Room Variables
                try
                {
                    if (xml.Element("body").Element("vars") != null)
                    {
                        currRoom.ClearVariables();
                        PopulateVariables(currRoom.GetVariables(), xml.Element("body"));
                    }
                }
                catch { }


                // Populate Room userList
                foreach (XElement usr in userListXml.Elements("u"))
                {
                    // grab the user properties
                    string name = DEFAULT_STRING;
                    try
                    {
                        name = usr.Element("n").Value;
                    }
                    catch { }

                    int id = DEFAULT_INT;
                    try
                    {
                        id = int.Parse(usr.Attribute("i").Value);
                    }
                    catch { }

                    bool isMod = DEFAULT_BOOL;
                    try
                    {
                        isMod = bool.Parse(usr.Attribute("m").Value);
                    }
                    catch { }

                    bool isSpec = DEFAULT_BOOL;
                    try
                    {
                        isSpec = bool.Parse(usr.Attribute("s").Value);
                    }
                    catch { }

                    int pId = DEFAULT_INT;
                    try
                    {
                        pId = usr.Attribute("p") == null ? -1 : int.Parse(usr.Attribute("p").Value);
                    }
                    catch { }

                    // Create and populate User
                    User user = new User(id, name);
                    user.SetModerator(isMod);
                    user.SetIsSpectator(isSpec);
                    user.SetPlayerId(pId);

                    // Handle user variables
                    try
                    {
                        if (usr.Element("vars") != null)
                        {
                            PopulateVariables(user.GetVariables(), usr);
                        }
                    }
                    catch { }

                    // Add user
                    currRoom.AddUser(user, id);
                }

                // operation completed, release lock
                sfs.changingRoom = false;

                // Fire event!
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("room", currRoom);

                SFSEvent evt = new SFSEvent(SFSEvent.onJoinRoomEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }

        /**
         * <summary>
         * Failed room Join
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleJoinKo(XElement xml)
        {
            sfs.changingRoom = false;
            string error = DEFAULT_STRING;
            try
            {
                error = xml.Element("body").Element("error").Attribute("msg").Value;
            }
            catch { }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("error", error);

            SFSEvent evt = new SFSEvent(SFSEvent.onJoinRoomErrorEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * New user enters the room
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleUserEnterRoom(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            // Get params
            int usrId = DEFAULT_INT;
            try
            {
                usrId = int.Parse(xml.Element("body").Element("u").Attribute("i").Value);
            }
            catch { }

            string usrName = DEFAULT_STRING;
            try
            {
                usrName = xml.Element("body").Element("u").Element("n").Value;
            }
            catch { }

            bool isMod = DEFAULT_BOOL;
            try
            {
                isMod = bool.Parse(xml.Element("body").Element("u").Attribute("m").Value);
            }
            catch { }

            bool isSpec = DEFAULT_BOOL;
            try
            {
                isSpec = bool.Parse(xml.Element("body").Element("u").Attribute("s").Value);
            }
            catch { }

            int pid = DEFAULT_INT;
            try
            {
                int.Parse(xml.Element("body").Element("u").Attribute("p").Value);
            }
            catch { }

            XElement varList = xml.Element("body").Element("u").Element("vars");

            Room currRoom = sfs.GetRoom(roomId);

            if (currRoom == null)
            {
                sfs.DebugMessage("WARNING! UserEnterRoom tries to enter an unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                // Create new user object
                User newUser = new User(usrId, usrName);
                newUser.SetModerator(isMod);
                newUser.SetIsSpectator(isSpec);
                newUser.SetPlayerId(pid);

                // Add user to room
                currRoom.AddUser(newUser, usrId);

                // Populate user vars
                if (varList != null)
                {
                    PopulateVariables(newUser.GetVariables(), xml.Element("body").Element("u"));
                }

                // Fire event!
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("roomId", roomId);
                parameters.Add("user", newUser);

                SFSEvent evt = new SFSEvent(SFSEvent.onUserEnterRoomEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }

        /**
         * <summary>
         * User leaves a room
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleUserLeaveRoom(XElement xml)
        {
            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            // Get room
            Room theRoom = sfs.GetRoom(roomId);

            if (theRoom == null)
            {
                sfs.DebugMessage("WARNING! UserLeaveRoom tries to leave an unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                // Get user name
                string uName = theRoom.GetUser(userId).GetName();

                // Remove user
                theRoom.RemoveUser(userId);

                // Fire event!
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("roomId", roomId);
                parameters.Add("userId", userId);
                parameters.Add("userName", uName);

                SFSEvent evt = new SFSEvent(SFSEvent.onUserLeaveRoomEvent, parameters);
                sfs.EnqueueEvent(evt);

            }
        }

        /**
         * <summary>
         * Handle public message
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandlePublicMessage(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            string message = DEFAULT_STRING;
            try
            {
                message = xml.Element("body").Element("txt").Value;
            }
            catch { }

            if (sfs.GetRoom(roomId) == null)
            {
                sfs.DebugMessage("WARNING! PublicMessage received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {
                User sender = sfs.GetRoom(roomId).GetUser(userId);

                if (sender == null)
                {
                    sfs.DebugMessage("WARNING! PublicMessage received from unknown sender. Command ignored!!");
                    // TODO: We bail out here - what to do instead? Anything?

                }
                else
                {
                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("message", Entities.DecodeEntities(message));
                    parameters.Add("sender", sender);
                    parameters.Add("roomId", roomId);

                    SFSEvent evt = new SFSEvent(SFSEvent.onPublicMessageEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }

        /**
         * <summary>
         * Handle player switched
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandlePlayerSwitched(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int playerId = DEFAULT_INT;
            try
            {
                int.Parse(xml.Element("body").Element("pid").Attribute("id").Value);
            }
            catch { }

            bool isItMe = true;
            try
            {
                if (xml.Element("body").Element("pid").Attribute("u") != null)
                {
                    isItMe = false;
                }
            }
            catch
            {
                isItMe = false;
            }

            // Synch user count, if switch successful
            Room theRoom = sfs.GetRoom(roomId);

            if (theRoom == null)
            {
                sfs.DebugMessage("WARNING! PlayerSwitched received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                if (playerId == -1)
                {
                    theRoom.SetUserCount(theRoom.GetUserCount() - 1);
                    theRoom.SetSpectatorCount(theRoom.GetSpectatorCount() + 1);

                    /*
                    * Update another user, who was turned into a player
                    */
                    if (!isItMe)
                    {
                        int userId = DEFAULT_INT;
                        try
                        {
                            userId = int.Parse(xml.Element("body").Element("pid").Attribute("id").Value);
                        }
                        catch { }

                        User user = theRoom.GetUser(userId);

                        if (user != null)
                        {
                            user.SetIsSpectator(true);
                            user.SetPlayerId(playerId);
                        }
                    }
                }

                /*
                * If it's me fire an event
                */
                if (isItMe)
                {
                    sfs.playerId = playerId;

                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("success", sfs.playerId == -1);
                    parameters.Add("newId", sfs.playerId);
                    parameters.Add("room", theRoom);

                    SFSEvent evt = new SFSEvent(SFSEvent.onPlayerSwitchedEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }


        /**
         * <summary>
         * Handle private message
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandlePrivateMessage(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            string message = DEFAULT_STRING;
            try
            {
                message = xml.Element("body").Element("txt").Value;
            }
            catch { }

            if (sfs.GetRoom(roomId) == null)
            {
                sfs.DebugMessage("WARNING! PrivateMessage received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                User sender = sfs.GetRoom(roomId).GetUser(userId);

                if (sender == null)
                {
                    sfs.DebugMessage("WARNING! PrivateMessage received from unknown sender. Command ignored!!");
                    // TODO: We bail out here - what to do instead? Anything?

                }
                else
                {
                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("message", Entities.DecodeEntities(message));
                    parameters.Add("sender", sender);
                    parameters.Add("roomId", roomId);
                    parameters.Add("userId", userId);

                    SFSEvent evt = new SFSEvent(SFSEvent.onPrivateMessageEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }

        /**
         * <summary>
         * Handle admin message
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleAdminMessage(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            string message = DEFAULT_STRING;
            try
            {
                message = xml.Element("body").Element("txt").Value;
            }
            catch { }

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("message", Entities.DecodeEntities(message));

            SFSEvent evt = new SFSEvent(SFSEvent.onAdminMessageEvent, parameters);
            sfs.EnqueueEvent(evt);

        }

        /**
         * <summary>
         * Handle moderator message
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleModMessage(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            string message = DEFAULT_STRING;
            try
            {
                message = xml.Element("body").Element("txt").Value;
            }
            catch { }

            User sender = null;
            Room room = sfs.GetRoom(roomId);

            if (sfs.GetRoom(roomId) == null)
            {
                sfs.DebugMessage("WARNING! ModMessage received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {
                sender = sfs.GetRoom(roomId).GetUser(userId);

                if (sender == null)
                {
                    sfs.DebugMessage("WARNING! ModMessage received from unknown sender. Command ignored!!");
                    // TODO: We bail out here - what to do instead? Anything?

                }
                else
                {
                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("message", Entities.DecodeEntities(message));
                    parameters.Add("sender", sender);

                    SFSEvent evt = new SFSEvent(SFSEvent.onModeratorMessageEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }

        /**
         * <summary>
         * Handle SFS object received
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleSFSObject(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            string xmlStr = DEFAULT_STRING;
            try
            {
                xmlStr = xml.Element("body").Element("dataObj").Value;
            }
            catch { }

            if (sfs.GetRoom(roomId) == null)
            {
                sfs.DebugMessage("WARNING! SFSObject received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {
                User sender = sfs.GetRoom(roomId).GetUser(userId);

                if (sender == null)
                {
                    sfs.DebugMessage("WARNING! SFSObject received from unknown sender. Command ignored!!");
                    // TODO: We bail out here - what to do instead? Anything?

                }
                else
                {
                    SFSObject asObj = SFSObjectSerializer.GetInstance().Deserialize(xmlStr);

                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("obj", asObj);
                    parameters.Add("sender", sender);

                    SFSEvent evt = new SFSEvent(SFSEvent.onObjectReceivedEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }

        /**
         * <summary>
         * Handle update of room variables
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleRoomVarsUpdate(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            Room currRoom = sfs.GetRoom(roomId);

            if (currRoom == null)
            {
                sfs.DebugMessage("WARNING! RoomVarsUpdate received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                Dictionary<string, object> changedVars = new Dictionary<string, object>();

                // Handle Room Variables
                try
                {
                    if (xml.Element("body").Element("vars") != null)
                    {
                        PopulateVariables(currRoom.GetVariables(), xml.Element("body"), changedVars);
                    }
                }
                catch { }

                // Fire event!
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("room", currRoom);
                parameters.Add("changedVars", changedVars);

                SFSEvent evt = new SFSEvent(SFSEvent.onRoomVariablesUpdateEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }

        /**
         * <summary>
         * Handle update of user variables
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        public void HandleUserVarsUpdate(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int userId = DEFAULT_INT;
            try
            {
                userId = int.Parse(xml.Element("body").Element("user").Attribute("id").Value);
            }
            catch { }

            if (sfs.GetRoom(roomId) == null)
            {
                sfs.DebugMessage("WARNING! UserVarsUpdate received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {
                User currUser = sfs.GetRoom(roomId).GetUser(userId);

                if (currUser == null)
                {
                    sfs.DebugMessage("WARNING! UserVarsUpdate received for unknown user. Command ignored!!");
                    // TODO: We bail out here - what to do instead? Anything?

                }
                else
                {
                    Dictionary<string, object> changedVars = new Dictionary<string, object>();

                    try
                    {
                        if (xml.Element("body").Element("vars") != null)
                        {
                            PopulateVariables(currUser.GetVariables(), xml.Element("body"), changedVars);
                        }
                    }
                    catch { }


                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("user", currUser);
                    parameters.Add("changedVars", changedVars);

                    SFSEvent evt = new SFSEvent(SFSEvent.onUserVariablesUpdateEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }

        /**
         * <summary>
         * Handle room added
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleRoomAdded(XElement xml)
        {
            int rId = DEFAULT_INT;
            try
            {
                rId = int.Parse(xml.Element("body").Element("rm").Attribute("id").Value);
            }
            catch { }

            string rName = DEFAULT_STRING;
            try
            {
                rName = xml.Element("body").Element("rm").Element("name").Value;
            }
            catch { }

            int rMax = DEFAULT_INT;
            try
            {
                rMax = int.Parse(xml.Element("body").Element("rm").Attribute("max").Value);
            }
            catch { }

            int rSpec = DEFAULT_INT;
            try
            {
                rSpec = int.Parse(xml.Element("body").Element("rm").Attribute("spec").Value);
            }
            catch { }

            bool isTemp = DEFAULT_BOOL;
            try
            {
                isTemp = bool.Parse(xml.Element("body").Element("rm").Attribute("temp").Value);
            }
            catch { }

            bool isGame = DEFAULT_BOOL;
            try
            {
                isGame = bool.Parse(xml.Element("body").Element("rm").Attribute("game").Value);
            }
            catch { }

            bool isPriv = DEFAULT_BOOL;
            try
            {
                isPriv = bool.Parse(xml.Element("body").Element("rm").Attribute("priv").Value);
            }
            catch { }

            bool isLimbo = DEFAULT_BOOL;
            try
            {
                isLimbo = bool.Parse(xml.Element("body").Element("rm").Attribute("limbo").Value);
            }
            catch { }

            // Create room obj
            Room newRoom = new Room(rId, rName, rMax, rSpec, isTemp, isGame, isPriv, isLimbo);

            Dictionary<int, Room> rList = sfs.GetAllRooms();
            rList[rId] = newRoom;

            // Handle Room Variables
            try
            {
                if (xml.Element("body").Element("rm").Element("vars") != null)
                    PopulateVariables(newRoom.GetVariables(), xml.Element("body").Element("rm"));
            }
            catch { }

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("room", newRoom);

            SFSEvent evt = new SFSEvent(SFSEvent.onRoomAddedEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle room deleted
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleRoomDeleted(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            Dictionary<int, Room> roomList = sfs.GetAllRooms();

            // Pass the last reference to the upper level
            // If there's no other references to this room in the upper level
            // This is the last reference we're keeping

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("room", roomList[roomId]);

            // Remove reference from main room list
            roomList.Remove(roomId);

            SFSEvent evt = new SFSEvent(SFSEvent.onRoomDeletedEvent, parameters);
            sfs.EnqueueEvent(evt);
        }


        /**
         * <summary>
         * Handle random key received
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleRandomKey(XElement xml)
        {
            string key = DEFAULT_STRING;
            try
            {
                key = xml.Element("body").Element("k").Value;
            }
            catch { }

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("key", key);

            SFSEvent evt = new SFSEvent(SFSEvent.onRandomKeyEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle reound trip benchmark
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleRoundTripBench(XElement xml)
        {
            DateTime now = DateTime.Now;
            TimeSpan res = now - sfs.GetBenchStartTime();

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("elapsed", Convert.ToInt32(res.TotalSeconds)); // TODO - check if casting from double to int is OK with original API

            SFSEvent evt = new SFSEvent(SFSEvent.onRoundTripResponseEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle unsuccessful create room
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleCreateRoomError(XElement xml)
        {
            string errMsg = DEFAULT_STRING;
            try
            {
                errMsg = xml.Element("body").Element("Room").Attribute("e").Value;
            }
            catch { }

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("error", errMsg);

            SFSEvent evt = new SFSEvent(SFSEvent.onCreateRoomErrorEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle buddy list received
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleBuddyList(XElement xml)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            sfs.buddyList.Clear();
            sfs.myBuddyVars.Clear();

            // Get my buddy variables
            try
            {
                if (xml.Element("body").Element("mv") != null)
                {
                    foreach (XElement myVar in xml.Element("body").Element("mv").Elements("v"))
                    {
                        sfs.myBuddyVars[myVar.Attribute("n").Value] = myVar.Value;
                    }
                }
            }
            catch { }


            // Get all buddies and variables
            if (xml.Element("body").Element("bList").Element("b") != null)
            {
                foreach (XElement b in xml.Element("body").Element("bList").Elements("b"))
                {
                    int bId = DEFAULT_INT;
                    try
                    {
                        bId = int.Parse(b.Attribute("i").Value);
                    }
                    catch { }

                    string bName = DEFAULT_STRING;
                    try
                    {
                        bName = b.Element("n").Value;
                    }
                    catch { }

                    bool bS = DEFAULT_BOOL;
                    try
                    {
                        bS = bool.Parse(b.Attribute("s").Value);
                    }
                    catch { }

                    bool bX = DEFAULT_BOOL;
                    try
                    {
                        bX = bool.Parse(b.Attribute("x").Value);
                    }
                    catch { }

                    Buddy buddy = new Buddy(bId, bName, bS, bX,
                        new Dictionary<string, object>());

                    // Runs through buddy variables
                    try
                    {
                        if (b.Element("vs") != null)
                        {
                            foreach (XElement bVar in b.Element("body").Elements("v"))
                            {
                                buddy.SetVariable(bVar.Attribute("n").Value, bVar.Value);
                            }
                        }
                    }
                    catch { }

                    sfs.buddyList.Add(buddy);
                }

                // Fire event!
                parameters.Add("list", sfs.buddyList);
                SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
                sfs.EnqueueEvent(evt);
            }

            // Buddy List load error!
            else
            {
                // Fire event!
                string error = DEFAULT_STRING;
                try
                {
                    error = xml.Element("body").Element("err").Value;
                }
                catch { }

                parameters.Add("error", error);
                SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListErrorEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }


        /**
         * <summary>
         * Handle update of buddy list
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleBuddyListUpdate(XElement xml)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            SFSEvent evt = null;

            if (xml.Element("body").Element("b") != null)
            {
                int bId = DEFAULT_INT;
                try
                {
                    bId = int.Parse(xml.Element("body").Element("b").Attribute("i").Value);
                }
                catch { }

                string bName = DEFAULT_STRING;
                try
                {
                    bName = xml.Element("body").Element("b").Element("n").Value;
                }
                catch { }

                bool bS = DEFAULT_BOOL;
                try
                {
                    bS = bool.Parse(xml.Element("body").Element("b").Attribute("s").Value);
                }
                catch { }

                bool bX = DEFAULT_BOOL;
                try
                {
                    bX = bool.Parse(xml.Element("body").Element("b").Attribute("x").Value);
                }
                catch { }

                Buddy buddy = new Buddy(bId, bName, bS, bX);

                // Runs through buddy variables
                XElement bVars = xml.Element("body").Element("b").Element("vs");

                bool found = false;

                foreach (Buddy tempB in sfs.buddyList)
                {
                    if (tempB.GetName() == buddy.GetName())
                    {
                        buddy.SetBlocked(tempB.IsBlocked());
                        buddy.SetVariables(tempB.GetVariables());

                        // add/modify variables
                        if (bVars != null)
                        {
                            foreach (XElement bVar in bVars.Elements("v"))
                            {
                                buddy.SetVariable(bVar.Attribute("n").Value, bVar.Value);
                            }
                        }

                        // swap objects
                        sfs.buddyList.Remove(tempB);
                        sfs.buddyList.Add(buddy);

                        found = true;
                        break;
                    }
                }

                // Fire event!
                if (found)
                {
                    parameters.Add("buddy", buddy);

                    evt = new SFSEvent(SFSEvent.onBuddyListUpdateEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }

            // Buddy List load error!
            else
            {
                // Fire event!
                string error = DEFAULT_STRING;
                try
                {
                    error = xml.Element("body").Element("err").Value;
                }
                catch { }
                parameters.Add("error", error);
                evt = new SFSEvent(SFSEvent.onBuddyListErrorEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }

        /**
         * <summary>
         * Handle permission to add buddy
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleAddBuddyPermission(XElement xml)
        {
            // Fire event!
            string sender = DEFAULT_STRING;
            try
            {
                sender = xml.Element("body").Element("n").Value;
            }
            catch { }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("sender", sender);
            parameters.Add("message", "");

            try
            {
                if (xml.Element("body").Element("txt") != null)
                {
                    string message = DEFAULT_STRING;
                    try
                    {
                        message = xml.Element("body").Element("txt").Value;
                    }
                    catch { }
                    parameters.Add("message", Entities.DecodeEntities(message));
                }
            }
            catch { }

            SFSEvent evt = new SFSEvent(SFSEvent.onBuddyPermissionRequestEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle buddy added
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleBuddyAdded(XElement xml)
        {
            int bId = DEFAULT_INT;
            try
            {
                bId = int.Parse(xml.Element("body").Element("b").Attribute("i").Value);
            }
            catch { }

            string bName = DEFAULT_STRING;
            try
            {
                bName = xml.Element("body").Element("b").Element("n").Value;
            }
            catch { }

            bool bS = DEFAULT_BOOL;
            try
            {
                bS = bool.Parse(xml.Element("body").Element("b").Attribute("s").Value);
            }
            catch { }

            bool bX = DEFAULT_BOOL;
            try
            {
                bX = bool.Parse(xml.Element("body").Element("b").Attribute("x").Value);
            }
            catch { }

            Buddy buddy = new Buddy(bId, bName, bS, bX);

            // Runs through buddy variables
            try
            {
                if (xml.Element("body").Element("b").Element("vs") != null)
                {
                    foreach (XElement bVar in xml.Element("body").Element("b").Element("vs").Elements("v"))
                    {
                        buddy.SetVariable(bVar.Attribute("n").Value, bVar.Value);
                    }
                }
            }
            catch { }

            sfs.buddyList.Add(buddy);

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("list", sfs.buddyList);

            SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle remove buddy
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleRemoveBuddy(XElement xml)
        {
            string buddyName = DEFAULT_STRING;
            try
            {
                buddyName = xml.Element("body").Element("n").Value;
            }
            catch { }


            foreach (Buddy buddy in sfs.buddyList)
            {
                if (buddy.GetName() == buddyName)
                {
                    sfs.buddyList.Remove(buddy);

                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("list", sfs.buddyList);

                    SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
                    sfs.EnqueueEvent(evt);

                    break;
                }
            }

        }

        /**
         * <summary>
         * Handle buddy room
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleBuddyRoom(XElement xml)
        {
            string roomIds = DEFAULT_STRING;
            try
            {
                roomIds = xml.Element("body").Element("br").Attribute("r").Value;
            }
            catch { }

            SyncArrayList ids = new SyncArrayList();
            foreach (string s in roomIds.Split(",".ToCharArray()))
            {
                int id = DEFAULT_INT;
                try
                {
                    id = int.Parse(s);
                }
                catch { }
                ids.Add(id);
            }

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("idList", ids);

            SFSEvent evt = new SFSEvent(SFSEvent.onBuddyRoomEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle leave room
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleLeaveRoom(XElement xml)
        {
            int roomLeft = DEFAULT_INT;
            try
            {
                roomLeft = int.Parse(xml.Element("body").Element("rm").Attribute("id").Value);
            }
            catch { }

            // Fire event!
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("roomId", roomLeft);

            SFSEvent evt = new SFSEvent(SFSEvent.onRoomLeftEvent, parameters);
            sfs.EnqueueEvent(evt);
        }

        /**
         * <summary>
         * Handle spectator switched
         * </summary>
         * 
         * <param name="xml">message object</param>
         */
        private void HandleSpectatorSwitched(XElement xml)
        {
            int roomId = DEFAULT_INT;
            try
            {
                roomId = int.Parse(xml.Element("body").Attribute("r").Value);
            }
            catch { }

            int playerId = DEFAULT_INT;
            try
            {
                playerId = int.Parse(xml.Element("body").Element("pid").Attribute("id").Value);
            }
            catch { }

            // Synch user count, if switch successful
            Room theRoom = sfs.GetRoom(roomId);

            if (theRoom == null)
            {
                sfs.DebugMessage("WARNING! SpectatorSwitched received for unknown room. Command ignored!! Roomlist not up to date?");
                // TODO: We bail out here - what to do instead? Anything?

            }
            else
            {

                if (playerId > 0)
                {
                    theRoom.SetUserCount(theRoom.GetUserCount() + 1);
                    theRoom.SetSpectatorCount(theRoom.GetSpectatorCount() - 1);
                }

                /*
                * Update another user, who was turned into a player
                */
                if (xml.Element("body").Element("pid").Attribute("u") != null)
                {
                    int userId = DEFAULT_INT;
                    try
                    {
                        int.Parse(xml.Element("body").Element("pid").Attribute("u").Value);
                    }
                    catch { }

                    User user = theRoom.GetUser(userId);

                    if (user != null)
                    {
                        user.SetIsSpectator(false);
                        user.SetPlayerId(playerId);
                    }
                }

                /*
                * Update myself
                */
                else
                {
                    sfs.playerId = playerId;

                    // Fire event!
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("success", sfs.playerId > 0);
                    parameters.Add("newId", sfs.playerId);
                    parameters.Add("room", theRoom);

                    SFSEvent evt = new SFSEvent(SFSEvent.onSpectatorSwitchedEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }
        }

        //=======================================================================
        // Other class methods
        //=======================================================================

        /**
         * <summary><see cref="PopulateVariables(Dictionary&lt;string, object&gt;, XElement, Dictionary&lt;string, object&gt;)"/></summary>
         */
        private void PopulateVariables(Dictionary<string, object> variables, XElement xmlData)
        {
            PopulateVariables(variables, xmlData, null);
        }
        /**
         * <summary>
         * Takes an SFS variables XML node and store it in an array<br/>
         * Usage: for parsing room and user variables
         * </summary>
         * 
         * <param name="variables">variable list to populate</param>
         * <param name="xmlData">the XML variables node</param>
         * <param name="changedVars">dictionary of changed variables</param>
         */
        private void PopulateVariables(Dictionary<string, object> variables, XElement xmlData, Dictionary<string, object> changedVars)
        {
            foreach (XElement v in xmlData.Element("vars").Elements("var"))
            {
                string vName = DEFAULT_STRING;
                try
                {
                    vName = v.Attribute("n").Value;
                }
                catch { }

                string vType = DEFAULT_STRING;
                try
                {
                    vType = v.Attribute("t").Value;
                }
                catch { }

                string vValue = DEFAULT_STRING;
                try
                {
                    vValue = v.Value;
                }
                catch { }

                // Add the vName to the list of changed vars
                // The changed List is an array that can contains all the
                // var names changed with numeric indexes but also contains
                // the var names as keys for faster search
                if (changedVars != null)
                {
                    changedVars.Add(vName, true);
                }

                if (vType == "b")
                    variables[vName] = (string)vValue == "1" ? true : false;

                else if (vType == "n")
                    variables[vName] = double.Parse(vValue);

                else if (vType == "s")
                    variables[vName] = (string)vValue;

                else if (vType == "x")
                    variables.Remove(vName);

            }
        }


        /**
         * <summary>
         * Handle disconnects
         * </summary>
         */
        public void DispatchDisconnection()
        {
            SFSEvent evt = new SFSEvent(SFSEvent.onConnectionLostEvent, null);
            sfs.EnqueueEvent(evt);
        }
    }
}