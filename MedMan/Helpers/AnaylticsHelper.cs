using System.Text;
using System.Web.Mvc;

namespace Med.Web.Helpers
{
    public static class AnaylticsHelper
    {
        /// 
        /// Creates Universal Analytics code as string,
        /// with the necessary javascript. Include in head of the page.
        /// 
        ///
        ///Google analytics Tracking-ID as a string
        ///Domainname can be given for extra data verification in Analytics.
        ///MVC html string with corresponsing javascript functions.
        public static MvcHtmlString AnalyticsCode(this HtmlHelper helper, string analyticsCode, string domainName = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<script type='text/javascript'>// <![CDATA[");
            sb.AppendLine("(function (i, s, o, g, r, a, m) {");
            sb.AppendLine(" i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {");
            sb.AppendLine("(i[r].q = i[r].q || []).push(arguments)");
            sb.AppendLine(" }, i[r].l = 1 * new Date(); a = s.createElement(o),");
            sb.AppendLine(" m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)");
            sb.AppendLine("})(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');");
 
            sb.Append("ga('create',");
            sb.Append("'" + analyticsCode + "' , ");
            sb.Append(string.IsNullOrWhiteSpace(domainName) ? "'auto'" : "'" + domainName + "'");
            sb.Append(");");
            sb.AppendLine();
            sb.AppendLine("ga('require', 'displayfeatures');");
            sb.AppendLine("ga('send', 'pageview');");
            sb.AppendLine("// ]]></script>"); 
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// Creates the Classic Google analytics code as a javascript function. Should be included in the head of the page
        /// 
        ///
        ///Google analytics Tracking-ID as a string
        ///Domainname can be given for extra data verification in Analytics.
        /// MVC html string with corresponsing javascript functions.
        public static MvcHtmlString ClassicAnalyticsCode(this HtmlHelper helper, string analyticsCode, string domainName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("var _gaq = _gaq || [];");
            sb.AppendLine("_gaq.push(['_setAccount', '" + analyticsCode + "']);");
            sb.AppendLine("_gaq.push(['_setDomainName', '" + domainName + "']);");
            sb.AppendLine("_gaq.push(['_trackPageview']);");

            sb.AppendLine("(function() {");
            sb.AppendLine("var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;");
            sb.AppendLine("ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';");
            sb.AppendLine("var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);");
            sb.AppendLine(" })();");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}