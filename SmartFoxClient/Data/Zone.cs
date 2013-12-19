using SmartFoxClientAPI.Util;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>The Zone class stores the properties of the current server zone.<br/>
     * This class is used internally by the <see cref="SmartFoxClient"/> class.</summary>
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
    public class Zone
    {
        private SyncArrayList roomList;
        private string name;

        /**
         * <summary>
         * Zone constructor.
         * </summary>
         * 
         * <param name="name">the zone name</param>
         * @exclude
         */
        public Zone(string name)
        {
            this.name = name;
            this.roomList = new SyncArrayList();
        }

        /**
         * <summary>
         * Get room by given id
         * </summary>
         * 
         * <param name="id">id of the room</param>
         * 
         * <returns>Room for given id</returns>
         * 
         * @exclude
         */
        public Room GetRoom(int id)
        {
            return (Room)roomList.ObjectAt(id);
        }

        /**
         * <summary>
         * Get room by given name
         * </summary>
         * 
         * <param name="name">name of the room</param>
         * 
         * <returns>Room for given name</returns>
         * 
         * @exclude
         */
        public Room GetRoomByName(string name)
        {
            Room room = null;
            bool found = false;

            foreach (Room r in roomList)
            {
                if (r.GetName() == name)
                {
                    room = r;
                    found = true;
                    break;
                }
            }

            if (found)
                return room;
            else
                return null;
        }
    }
}