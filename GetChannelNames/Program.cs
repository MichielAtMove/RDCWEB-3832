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
            var channels1 = GetChannelsFromAddTrigger();
            
            var channels2 = GetChannelsFromAddListener();

            var allnames = (from channel in channels1 select channel.Key).Union(from channel in channels2 select channel.Key).Distinct();

            var s = FormatAsEnum(allnames);

            Trace.WriteLine(s);


        }

        private static string FormatAsEnum(IEnumerable<string> channels)
        {
            return String.Format("public enum Channel = \n{{  {0}\n}};",
                String.Join(", \n",
                    from channel in channels orderby channel select String.Format("\t{0}", channel)));
        }

        private static AutoDictionary<string, List<string>> GetChannelsFromAddTrigger()
        {
            var pattern =
                new Regex(
                    "\\([0-9]+,[0-9]+\\) (?:[a-zA-Z_][a-zA-Z0-9_$]*\\s*.\\s*)+AddTrigger\\s*\\(\\s*\"(?<eventname>[^\"]*)\"\\s*,\\s*[^,]+\\s*,\\s*\"(?<channelname>[^\"]*)\"");

            var matches = Matches(pattern, "../../../AddTrigger.xml", "/model/node[position()=2]/node/node/*");

            var dictionary = new AutoDictionary<string, List<string>>();

            foreach (var match in matches)
            {
                var eventname = match.Groups["eventname"].ToString();
                var channelname = match.Groups["channelname"].ToString();

                dictionary[channelname].Add(eventname);
            }
            return dictionary;
        }

        private static AutoDictionary<string, List<string>> GetChannelsFromAddListener()
        {
            var pattern =
                new Regex(
                    "\\([0-9]+,[0-9]+\\) (?:[a-zA-Z_][a-zA-Z0-9_$]*\\s*.\\s*)+AddListener\\s*\\(\\s*\"(?<channelname>[^\"]*)\"\\s*,\\s*\"(?<method>[^\"]*)\"");

            Trace.WriteLine(pattern.ToString());

            var matches = Matches(pattern, "../../../AddListener.xml", "/model/node/node/node/node/*");

            var dictionary = new AutoDictionary<string, List<string>>();

            foreach (var match in matches)
            {
                var methodname = match.Groups["methodname"].ToString();
                var channelname = match.Groups["channelname"].ToString();

                dictionary[channelname].Add(methodname);
            }
            return dictionary;
        }

        private static IEnumerable<Match> Matches(Regex pattern, string filename, string modelNodeNodeNodeNode)
        {
            var statementsFromResharperOutputDocument = GetStatementsFromResharperOutputDocument(filename, modelNodeNodeNodeNode);

            var matches =
                statementsFromResharperOutputDocument
                    .Select(statement => pattern.Match(statement))
                    .ToArray();
            
            return matches.Where(match => match.Success);
        }

        private static IEnumerable<string> GetStatementsFromResharperOutputDocument(string filename, string modelNodeNodeNodeNode)
        {
            var document = new XmlDocument();
            document.Load(filename);
            var n = document.SelectNodes(modelNodeNodeNodeNode);

            if (n == null)
                return null;

            return from node in n.Cast<XmlNode>() where node.Attributes != null select node.Attributes["column0"].Value;
        }
    }
}
