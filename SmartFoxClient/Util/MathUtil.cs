
namespace SmartFoxClientAPI.Util
{
    /**
     * <summary>Math utilities</summary>
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
    public class MathUtil
    {
        /**
         * <summary>
         * Check if given object is numeric
         * </summary>
         * 
         * <param name="expression">Object to inspect</param>
         * 
         * <returns><c>true</c> if object is numeric</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static bool IsNumeric(object expression)
        {
            if (expression == null || expression is System.DateTime)
                return false;

            if (expression is short || expression is int || expression is long || expression is decimal || expression is float || expression is double || expression is bool)
                return true;


            try
            {
                if (expression is string)
                    double.Parse(expression as string);
                else
                    double.Parse(expression.ToString());
                return true;
            }
            catch { } // just dismiss errors but return false
            return false;
        }
    }

}
