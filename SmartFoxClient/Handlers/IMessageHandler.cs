
namespace SmartFoxClientAPI.Handlers
{
    /**
     * <summary>Handlers interface.</summary>
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
    public interface IMessageHandler
    {
        /**
         * <summary>
         * Handle messages
         * </summary>
         * 
         * <param name="msgObj">the message object to handle</param>
         * <param name="type">type of message</param>
         */
        void HandleMessage(object msgObj, string type);
    }
}