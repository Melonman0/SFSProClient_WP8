using System.Collections.Generic;

namespace SmartFoxClientAPI.Util
{
    /**
     * <summary>Entities class</summary>
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
    public class Entities
    {
        private static Dictionary<char, string> ascTab = new Dictionary<char, string>();
        private static Dictionary<string, string> ascTabRev = new Dictionary<string, string>();
        private static Dictionary<string, int> hexTable = new Dictionary<string, int>();

        Entities()
        {
        }

        internal static void Initialize()
        {
            //--- XML Entities Conversion table ----------------------
            ascTab.Clear();
            ascTab.Add('>', "&gt;");
            ascTab.Add('<', "&lt;");
            ascTab.Add('&', "&amp;");
            ascTab.Add('\'', "&apos;");
            ascTab.Add('"', "&quot;");

            ascTabRev.Clear();
            ascTabRev.Add("&gt;", ">");
            ascTabRev.Add("&lt;", "<");
            ascTabRev.Add("&amp;", "&");
            ascTabRev.Add("&apos;", "'");
            ascTabRev.Add("&quot;", "\"");

            hexTable.Clear();
            hexTable.Add("0", 0);
            hexTable.Add("1", 1);
            hexTable.Add("2", 2);
            hexTable.Add("3", 3);
            hexTable.Add("4", 4);
            hexTable.Add("5", 5);
            hexTable.Add("6", 6);
            hexTable.Add("7", 7);
            hexTable.Add("8", 8);
            hexTable.Add("9", 9);
            hexTable.Add("A", 10);
            hexTable.Add("B", 11);
            hexTable.Add("C", 12);
            hexTable.Add("D", 13);
            hexTable.Add("E", 14);
            hexTable.Add("F", 15);
        }

        /**
         * <summary>
         * Encode given entities
         * </summary>
         * 
         * <param name="st">String to encode</param>
         * 
         * <returns>Encoded entities</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static string EncodeEntities(string st)
        {
            string strbuff = "";

            // char codes < 32 are ignored except for tab,lf,cr
            for (int i = 0; i < st.Length; i++)
            {
                char ch = st.Substring(i, 1).ToCharArray()[0];
                int cod = (int)ch;

                if (cod == 9 || cod == 10 || cod == 13)
                {
                    strbuff += ch;
                }
                else if (cod >= 32 && cod <= 126)
                {
                    if (ascTab.ContainsKey(ch))
                    {
                        strbuff += ascTab[ch];
                    }
                    else
                        strbuff += ch;
                }

                else
                    strbuff += ch;
            }

            return strbuff;
        }



        /**
         * <summary>
         * Decode entity string
         * </summary>
         * 
         * <param name="st">String to decode</param>
         * 
         * <returns>Decoded entities</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static string DecodeEntities(string st)
        {
            string strbuff;
            string ch;
            string ent;
            string chi;
            string item;

            int i = 0;

            strbuff = "";

            while (i < st.Length)
            {
                ch = st.Substring(i, 1);

                if (ch == "&")
                {
                    ent = ch;

                    // read the complete entity
                    do
                    {
                        i++;
                        chi = st.Substring(i, 1);
                        ent += chi;
                    }
                    while (chi != ";" && i < st.Length);

                    item = (string)ascTabRev[ent];

                    if (item != null)
                        strbuff += item;
                    else
                        strbuff += (char)GetCharCode(ent);
                }
                else
                    strbuff += ch;

                i++;
            }

            return strbuff;
        }


        //-----------------------------------------------
        // Transform xml code entity into hex code
        // and return it as a number
        //-----------------------------------------------
        private static int GetCharCode(string ent)
        {
            string hex = ent.Substring(3, ent.Length);
            hex = hex.Substring(0, hex.Length - 1);

            return int.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier);
        }
    }
}