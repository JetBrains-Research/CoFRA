using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Cofra.Core.Util
{
    public static class Logging
    {
        public static void Log(string info)
        {
            Console.WriteLine(info);
        }
        
        public static void LogXML(string xml)
        {
            #if DEBUG

            var result = "";
            var document = new XmlDocument();
            using (var mStream = new MemoryStream())
            using (var writer = new XmlTextWriter(mStream, Encoding.Unicode))
            {
                try
                {
                    document.LoadXml(xml);
                    writer.Formatting = Formatting.Indented;
                    document.WriteContentTo(writer);
                    writer.Flush();
                    mStream.Flush();
                    mStream.Position = 0;
                    var formattedXml = new StreamReader(mStream).ReadToEnd();
                    result = formattedXml;
                }
                catch (XmlException)
                {
                    Console.WriteLine("Exception while xml formatting");
                }
            }

            Console.WriteLine(DeleteSystemInfoFromXml(result));

            #endif
        }

        private static string DeleteSystemInfoFromXml(string xml)
        {
            return xml.Replace("xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"", "")
                .Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "")
                .Replace("xmlns:a=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"", "")
                .Replace("xmlns:c=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"", "")
                .Replace("xmlns:b=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"", "")
                
                .Replace("xmlns:a=\"http://schemas.datacontract.org/2004/07/Cofra.AbstractIL.Common.Types\"", "")
                .Replace("xmlns:b=\"http://schemas.datacontract.org/2004/07/Cofra.AbstractIL.Common.Types\"", "")
                .Replace("xmlns:c=\"http://schemas.datacontract.org/2004/07/Cofra.AbstractIL.Common.Types\"", "")
                
                .Replace("xmlns=\"http://schemas.datacontract.org/2004/07/Cofra.Contracts.Messages.Requests\"", "")
                .Replace("xmlns=\"http://schemas.datacontract.org/2004/07/Cofra.Contracts.Messages.Responses\"", "")
                .Replace("xmlns:a=\"http://schemas.datacontract.org/2004/07/Cofra.AbstractIL.Common.Statements\"", "")
                .Replace("xmlns:b=\"http://schemas.datacontract.org/2004/07/Cofra.AbstractIL.Common.Statements\"", "")
                .Replace("xmlns:c=\"http://schemas.datacontract.org/2004/07/Cofra.AbstractIL.Common.Statements\"", "")
                .Replace("  ", " ");
        }
    }
}
