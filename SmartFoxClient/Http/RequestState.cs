using System.Net;

namespace SmartFoxClientAPI.Http
{
    /**
     * <summary>Class that stores the state of the request.</summary>
     * 
     * <remarks>
     * <para><b>Version:</b><br/>
     * 1.0.0</para>
     * 
     * <para><b>Author:</b><br/>
     * Thomas Hentschel Lund<br/>
     * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
     * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
     * (c) 2008 gotoAndPlay()<br/>
     *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
     * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
     * </para>
     * </remarks>
     */
    public class RequestState
    {
        private WebRequest request;

        /**
         * <summary>
         * RequestState constructor.
         * </summary>
         */
        public RequestState()
        {
            request = null;
        }

        /**
         * <summary>
         * Gets current request
         * </summary>
         * 
         * <returns>The current request</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public WebRequest GetRequest()
        {
            return request;
        }

        /**
         * <summary>
         * Set new request
         * </summary>
         * 
         * <param name="request">The new request</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void SetRequest(WebRequest request)
        {
            this.request = request;
        }
    }
}
