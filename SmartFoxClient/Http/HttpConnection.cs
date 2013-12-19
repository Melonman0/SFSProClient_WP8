using System;
using System.Collections.Generic;
using System.Net;

namespace SmartFoxClientAPI.Http
{
    /**
     * <summary>Internal class for handling all HTTP based communication</summary>
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
    public class HttpConnection
    {
        /**
         * <summary>Token used in handshaking</summary>
         */
        public const string HANDSHAKE_TOKEN = "#";

        private const string HANDSHAKE = "connect";
        private const string DISCONNECT = "disconnect";
        private const string CONN_LOST = "ERR#01";

        private const string servletUrl = "BlueBox/HttpBox.do";
        private const string paramName = "sfsHttp";

        private string sessionId;
        private bool connected = false;
        private string ipAddr;
        private int port;
        private string webUrl;

        private IHttpProtocolCodec codec;

        private SmartFoxClient sfs;

        /**
         * <summary>Delegate for all HTTP callbacks</summary>
         */
        public delegate void HttpCallbackHandler(HttpEvent evt);

        // Handlers
        private HttpCallbackHandler OnHttpConnectCallback;
        private HttpCallbackHandler OnHttpCloseCallback;
        private HttpCallbackHandler OnHttpErrorCallback;
        private HttpCallbackHandler OnHttpDataCallback;

        /**
         * <summary>
         * HttpConnection constructor.
         * </summary>
         * 
         * <param name="sfs">The smartfox client</param>
         */
        public HttpConnection(SmartFoxClient sfs)
        {
            this.codec = new RawProtocolCodec();
            this.sfs = sfs;
        }

        /**
         * <summary>
         * Get the session id of the current connection.
         * </summary>
         * 
         * <returns>The session id</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public string GetSessionId()
        {
            return this.sessionId;
        }

        /**
         * <summary>
         * A boolean flag indicating if we are connected
         * </summary>
         * 
         * <returns><c>true</c> if the conection is open</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsConnected()
        {
            return this.connected;
        }

        /**
         * <summary><see cref="Connect(string, int)"/></summary>
         */
        public void Connect(string ipAddr)
        {
            Connect(ipAddr, 8080);
        }

        /**
         * <summary>
         * Connect to the given server address and port
         * </summary>
         * 
         * <param name="ipAddr">Address of server</param>
         * <param name="port">Port of server</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void Connect(string ipAddr, int port)
        {
            this.ipAddr = ipAddr;
            this.port = port;
            this.webUrl = "http://" + this.ipAddr + ":" + this.port + "/" + servletUrl;

            Send(HANDSHAKE);

        }

        /**
         * <summary>
         * Close current connection
         * </summary>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void Close()
        {
            Send(DISCONNECT);
        }

        /**
         * <summary>
         * Send message to server
         * </summary>
         * 
         * <param name="message">Message to send</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void Send(string message)
        {
            if (connected || (!connected && message == HANDSHAKE) || (!connected && message == "poll"))
            {
                sfs.DebugMessage("[ BlueBox Send ]: " + message + "\n");

                HttpSendViaWebClient(message);
            }
        }

        private void HttpSendViaWebClient(string message)
        {
            WebClient client = new WebClient();

            //client.Headers["Content-Type"] = "application/x-www-form-urlencoded; charset=utf-8";

            client.UploadStringCompleted += new UploadStringCompletedEventHandler(HttpAsyncDownloadCompletedCallback);
            string data = paramName + "=" + codec.Encode(this.sessionId, message);

            client.UploadStringAsync(new Uri(this.webUrl), "Post", data);
        }

        private void HttpAsyncDownloadCompletedCallback(object sender, UploadStringCompletedEventArgs e)
        {

            if (e.Error == null)
            {

                string dataPayload = System.Text.RegularExpressions.Regex.Replace(e.Result, @"\s+$", "");

                sfs.DebugMessage("[ BlueBox Receive ]: '" + dataPayload + "'\n");

                // Data is read now - lets process it unless the length is 0 (nothing recieved)
                // Handle handshake
                if (dataPayload.Length != 0 && dataPayload.Substring(0, 1) == HANDSHAKE_TOKEN)
                {
                    if (sessionId == null)
                    {
                        sessionId = codec.Decode(dataPayload);
                        connected = true;

                        sfs.DebugMessage("BlueBox Connected with session id : " + sessionId);

                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        parameters.Add("sessionId", this.sessionId);
                        parameters.Add("success", true);

                        if (OnHttpConnectCallback != null)
                            OnHttpConnectCallback(new HttpEvent(HttpEvent.onHttpConnect, parameters));

                    }
                    else
                    {
                        sfs.DebugMessage("**ERROR** SessionId is being rewritten");
                    }
                }
                // Handle data
                else
                {
                    // The server can send multiple messages back to us - separated by \n characters
                    // Split and handle each message
                    string[] messages = System.Text.RegularExpressions.Regex.Split(dataPayload, "\n");

                    for (int messageCount = 0; messageCount < messages.Length; messageCount++)
                    {
                        if (messages[messageCount].Length > 0)
                        {
                            // fire disconnection
                            if (messages[messageCount].IndexOf(CONN_LOST) == 0)
                            {
                                if (OnHttpCloseCallback != null)
                                    OnHttpCloseCallback(new HttpEvent(HttpEvent.onHttpClose, null));
                            }

                            // fire onHttpData
                            else
                            {

                                Dictionary<string, object> parameters = new Dictionary<string, object>();
                                parameters.Add("data", messages[messageCount]);
                                if (OnHttpDataCallback != null)
                                {
                                    OnHttpDataCallback(new HttpEvent(HttpEvent.onHttpData, parameters));
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                // Got an error
                sfs.DebugMessage("WARNING: Error trying to send/read data via HTTP. Exception: " + e.ToString());
                if (OnHttpCloseCallback != null)
                    OnHttpCloseCallback(new HttpEvent(HttpEvent.onHttpClose, null));
            }
        }

        /**
         * <summary>
         * Add callback methods to the given event
         * </summary>
         * 
         * <param name="evt">The event to listen to</param>
         * <param name="method">The callback handler</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void AddEventListener(string evt, HttpCallbackHandler method)
        {
            switch (evt)
            {
                case HttpEvent.onHttpConnect:
                    OnHttpConnectCallback = method;
                    break;
                case HttpEvent.onHttpClose:
                    OnHttpCloseCallback = method;
                    break;
                case HttpEvent.onHttpError:
                    OnHttpErrorCallback = method;
                    break;
                case HttpEvent.onHttpData:
                    OnHttpDataCallback = method;
                    break;
            }
        }
    }
}
