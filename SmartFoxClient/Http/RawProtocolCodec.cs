
namespace SmartFoxClientAPI.Http
{
    /**
     * <summary>Raw HTTP codec class</summary>
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
    public class RawProtocolCodec : IHttpProtocolCodec
    {
        private const int SESSION_ID_LEN = 32;

        /**
         * <summary>Encode given message with given session id</summary>
         * 
         * <param name="sessionId">Session id to use for encoding</param>
         * <param name="message">Message to encode</param>
         * 
         * <returns>Encoded message</returns>
         */
        public string Encode(string sessionId, string message)
		{
			return ((sessionId == null ? "" : sessionId) + message);
		}

        /**
         * <summary>Decode given message</summary>
         * 
         * <param name="message">Message to decode</param>
         * 
         * <returns>Decoded message</returns>
         */
        public string Decode(string message)
		{
			string decoded = "";
			
			// Decode the connect response
			if (message.Substring(0, 1) == HttpConnection.HANDSHAKE_TOKEN)
			{
				decoded = message.Substring(1, SESSION_ID_LEN);
			}

            return decoded;
		}
    }
}
