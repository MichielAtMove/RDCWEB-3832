using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace GetChannelNames
{
    class Program
    {
        private static void Main(string[] args)
        {
            var channels = GetChannels();

            var s = String.Format("public enum Channel = \n{{  {0}\n}};",
                String.Join(", \n",
                    from channel in channels orderby channel.Key select String.Format("\t{0}", channel.Key)));

            Trace.WriteLine(s);


            /*
            foreach (var channel in from channel in channels
                                    orderby channel.Key 
                                    select channel)
            Trace.WriteLine(String.Format("{0}\tcount: {1}", channel.Key, channel.Value.Count));
             */
        }

        private static AutoDictionary<string, List<string>> GetChannels()
        {
            var document = new XmlDocument();
            document.Load("../../../AddTrigger.xml");
            var n = document.SelectNodes("/model/node[position()=2]/node/node/*");

            var statements = from node in n.Cast<XmlNode>() select node.Attributes["column0"].Value;

            // (21,27) signUpFormConnections.AddTrigger("SIGN_UP_SUCCESS", WidgetEventConnector.Events.Message, "USER_SIGNED_UP");

            var pattern =
                new Regex(
                    "\\([0-9]+,[0-9]+\\) (?:[a-zA-Z_][a-zA-Z0-9_$]*\\s*.\\s*)+AddTrigger\\s*\\(\\s*\"(?<eventname>[^\"]*)\"\\s*,\\s*[^,]+\\s*,\\s*\"(?<channelname>[^\"]*)\"");

            var matches = statements.Select(statement => pattern.Match(statement)).ToArray();

            if (matches.Any(match => !match.Success))
                throw new ApplicationException();

            var dictionary = new AutoDictionary<string, List<string>>();

            foreach (var match in matches)
            {
                var eventname = match.Groups["eventname"].ToString();
                var channelname = match.Groups["channelname"].ToString();

                dictionary[channelname].Add(eventname);
            }
            return dictionary;
        }
    }
}
