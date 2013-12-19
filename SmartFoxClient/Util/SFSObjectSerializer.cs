using SmartFoxClientAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SmartFoxClientAPI.Util
{
    /** 
     * <summary>SFS Object Serializer and Deserializer Class.</summary>
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
    public class SFSObjectSerializer
    {
        private static SFSObjectSerializer _instance;
        private static Dictionary<char, string> asciiTable_e;	// ascii code table for encoding	

        private SFSObjectSerializer()
        {
            asciiTable_e = new Dictionary<char, string>();

            asciiTable_e.Add('>', "&gt;");
            asciiTable_e.Add('<', "&lt;");
            asciiTable_e.Add('&', "&amp;");
            asciiTable_e.Add('\'', "&apos;");
            asciiTable_e.Add('"', "&quot;");
        }

        /**
         * <summary>
         * Get instance of this serializer
         * </summary>
         * 
         * <returns>Singleton instance of this serializer</returns>
         */
        public static SFSObjectSerializer GetInstance()
        {
            if (_instance == null)
                _instance = new SFSObjectSerializer();

            return _instance;
        }

        /**
         * <summary>
         * Serialize given object
         * </summary>
         * 
         * <param name="ao">Object to serialize</param>
         * 
         * <returns>Serialized object</returns>
         */
        public string Serialize(SFSObject ao)
        {
            StringBuilder xmlData = new StringBuilder();
            Obj2xml(ao, 0, "", xmlData);

            return xmlData.ToString();
        }

        /**
         * <summary>
         * Deserialize given string to object
         * </summary>
         * 
         * <param name="xmlData">String to deserialize</param>
         * 
         * <returns>Deserialized object</returns>
         */
        public SFSObject Deserialize(string xmlData)
        {
            SFSObject ao = new SFSObject();

            try
            {
                XDocument xmlDoc = XDocument.Load(new StringReader(xmlData));

                XElement root = xmlDoc.Root;

                Xml2obj(root, ao, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problems parsing XML: " + e);
            }
            return ao;
        }

        private void Xml2obj(XElement xmlNode, SFSObject ao, int depth)
        {
            foreach (XElement subNode in xmlNode.Descendants())
            {
                switch (subNode.Name.ToString())
                {
                    case "obj":
                        string objName = subNode.Attribute("o").Value;

                        SFSObject subASObj = new SFSObject();
                        ao.Put(objName, subASObj);
                        Xml2obj(subNode, subASObj, depth + 1);

                        break;

                    case "var":
                        string name = subNode.Attribute("n").Value;
                        string type = subNode.Attribute("t").Value;
                        string val;
                        try
                        {
                            val = type != "x" ? subNode.Value : null;
                        }
                        catch
                        {
                            val = "";
                        }

                        object varValue = null;

                        //--- bool ----------------------------------------------------------------
                        if (type == "b")
                            varValue = val == "1";

                        //--- string ------------------------------------------------------------------
                        else if (type == "s")
                            varValue = val;

                        //--- Number ------------------------------------------------------------------
                        else if (type == "n")
                        {
                            // This is a workaround for a mono bug where mono doesnt automatically changes
                            // , and . as separator depending on running on OSX or Windows.
                            varValue = double.Parse(val);
                        }

                        // Add as string key
                        ao.Put(name, varValue);

                        break;

                }
            }
        }

        private void Obj2xml(SFSObject ao, int depth, string nodeName, StringBuilder xmlData)
        {
            if (depth == 0)
                xmlData.Append("<dataObj>");
            else
                xmlData.Append("<obj o='").Append(nodeName).Append("' t='a'>");

            ICollection<object> keys = ao.Keys();

            foreach (object k in keys)
            {
                string key = k.ToString();
                object o = ao.Get(key);

                //--- Handle Nulls -----------------------------------------
                if (o == null)
                    xmlData.Append("<var n='").Append(key).Append("' t='x' />");

                else if (o is SFSObject)
                {
                    // Scan the object recursively			
                    Obj2xml((SFSObject)o, depth + 1, key, xmlData);

                    // When you get back to this level, close the 
                    xmlData.Append("</obj>");
                }

                else if (o is bool)
                {
                    bool b = (bool)o;
                    xmlData.Append("<var n='").Append(key).Append("' t='b'>").Append((b ? "1" : "0")).Append("</var>");

                }

                //--- Handle strings and Numbers ---------------------------------------
                else if (o is float)
                {
                    float val = (float)o;
                    xmlData.Append("<var n='").Append(key).Append("' t='n'>").Append(val.ToString()).Append("</var>");
                }
                else if (o is double)
                {
                    double val = (double)o;
                    xmlData.Append("<var n='").Append(key).Append("' t='n'>").Append(val.ToString()).Append("</var>");
                }
                else if (o is int)
                {
                    xmlData.Append("<var n='").Append(key).Append("' t='n'>").Append(o.ToString()).Append("</var>");
                }



              //--- Handle strings ---------------------------------------
                else if (o is string)
                {
                    xmlData.Append("<var n='").Append(key).Append("' t='s'>").Append(EncodeEntities((string)o)).Append("</var>");
                }
            }

            // If we're back to root node then close it!
            if (depth == 0)
                xmlData.Append("</dataObj>");
        }

        private static string EncodeEntities(string in_str)
        {
            char[] in_chars = in_str.ToCharArray();
            string out_str = "";
            int i = 0;

            while (i < in_chars.Length)
            {
                char c = in_chars[i];

                if (asciiTable_e.ContainsKey(c))
                {
                    out_str += asciiTable_e[c];
                }
                else
                {
                    out_str += c;
                }
                i++;
            }

            return out_str;
        }
    }
}
