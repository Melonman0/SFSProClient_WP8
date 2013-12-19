using LitJson;
using SmartFoxClientAPI.Data;
using SmartFoxClientAPI.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SmartFoxClientAPI.Handlers
{
    /**
     * <summary>Extension handler class.</summary>
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
    public class ExtHandler : IMessageHandler
    {
        SmartFoxClient sfs;

        /**
         * <summary>
         * ExtHandler constructor.
         * </summary>
         * 
         * <param name="sfs">the smart fox client</param>
         */
        public ExtHandler(SmartFoxClient sfs)
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
            Dictionary<string, object> parameters;
            SFSEvent evt;

            if (type == SmartFoxClient.XTMSG_TYPE_XML)
            {

                XDocument xmlDoc = (XDocument)msgObj;
                XElement xml = xmlDoc.Element("msg");
                XElement xmlBody = xml.Element("body");
                string action = xmlBody.Attribute("action").Value;

                if (action == "xtRes")
                {
                    string xmlStr = xmlBody.Value;
                    SFSObject asObj = SFSObjectSerializer.GetInstance().Deserialize(xmlStr);

                    // Fire event!
                    parameters = new Dictionary<string, object>();
                    parameters.Add("dataObj", asObj);
                    parameters.Add("type", type);

                    evt = new SFSEvent(SFSEvent.onExtensionResponseEvent, parameters);
                    sfs.EnqueueEvent(evt);
                }
            }

            else if (type == SmartFoxClient.XTMSG_TYPE_JSON)
            {
                // Fire event!
                parameters = new Dictionary<string, object>();
                parameters.Add("dataObj", ((JsonData)msgObj)["o"]);
                parameters.Add("type", type);

                evt = new SFSEvent(SFSEvent.onExtensionResponseEvent, parameters);
                sfs.EnqueueEvent(evt);
            }

            else if (type == SmartFoxClient.XTMSG_TYPE_STR)
            {
                // Fire event!
                parameters = new Dictionary<string, object>();
                parameters.Add("dataObj", msgObj);
                parameters.Add("type", type);

                evt = new SFSEvent(SFSEvent.onExtensionResponseEvent, parameters);
                sfs.EnqueueEvent(evt);
            }
        }
    }
}