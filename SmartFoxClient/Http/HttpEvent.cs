using System.Collections.Generic;

namespace SmartFoxClientAPI.Http
{
    /**
     * <summary>HttpEvent class</summary>
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
    public class HttpEvent
    {
        /**
         * <summary>Event fired when data is received</summary>
         */
        public const string onHttpData = "OnHttpData";

        /**
         * <summary>Event fired when error is received on connection</summary>
         */
        public const string onHttpError = "OnHttpError";

        /**
        * <summary>Event fired on connection</summary>
        */
        public const string onHttpConnect = "OnHttpConnect";

        /**
         * <summary>Event fired when connection is closed</summary>
         */
        public const string onHttpClose = "OnHttpClose";

        private string type;
        private Dictionary<string, object> parameters;

        /**
         * <summary>HttpEvent contructor.</summary>
         * 
         * <param name="type">the event's type (see the constants in this class).</param>
         * <param name="parameters">the parameters object for the event.</param>
         * @exclude
         */
        public HttpEvent(string type, Dictionary<string, object> parameters)
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
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
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
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public object GetParameter(string key)
        {
            return parameters[key];
        }
    }
}
