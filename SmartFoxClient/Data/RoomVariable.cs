
namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>RoomVariable is the class representing a rooms variables.<br/>
     * This class is used internally by the <see cref="SmartFoxClient"/> class; also, RoomVariable objects are returned by various methods and events of the SmartFoxServer API.</summary>
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

    public class RoomVariable
    {
        private string name;
        private object value;
        private bool isPrivate;
        private bool isPersistent;

        /**
         * <summary>
         * <see cref="RoomVariable(string, object, bool, bool)"/>
         * </summary>
         */
        public RoomVariable(string name, object value) : this(name, value, false, false) { }

        /**
         * <summary>
         * <see cref="RoomVariable(string, object, bool, bool)"/>
         * </summary>
         */
        public RoomVariable(string name, object value, bool isPrivate) : this(name, value, isPrivate, false) { }

        /**
         * <summary>
         * Room variable constructor.
         * </summary>
         * 
         * <param name="name">the variable name</param>
         * <param name="value">the variable value</param>
         * <param name="isPrivate"><c>true</c> if the variable is private</param>
         * <param name="isPersistent"><c>true</c> if the variable is persistent</param>
         */
        public RoomVariable(string name, object value, bool isPrivate, bool isPersistent)
        {
            this.name = name;
            this.value = value;
            this.isPrivate = isPrivate;
            this.isPersistent = isPersistent;
        }

        /**
         * <summary>
         * Get the name of the variable.
         * </summary>
         * 
         * <returns>The name of the variable</returns>
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
         * Get the value of the variable.
         * </summary>
         * 
         * <returns>The value of the variable</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public object GetValue()
        {
            return value;
        }

        /**
         * <summary>
         * A boolean flag indicating if the variable is private
         * </summary>
         * 
         * <returns><c>true</c> if the variable is private</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsPrivate()
        {
            return isPrivate;
        }

        /**
         * <summary>
         * A boolean flag indicating if the variable is persistent
         * </summary>
         * 
         * <returns><c>true</c> if the variable is persistent</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsPersistent()
        {
            return isPersistent;
        }
    }
}
