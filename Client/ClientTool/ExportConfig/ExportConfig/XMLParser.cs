using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;

public class XMLParser
{
    static public void parseXMLFile(XMLHandler handler, MemoryStream stream)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(stream);
        parserNode(doc, handler);
    }

    static public void parseXMLFile(XMLHandler handler, string text)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);
        parserNode(doc, handler);
    }

    private static void parserNode(XmlNode node, XMLHandler handler)
    {
        XMLAttributes xmlAttributes = new XMLAttributes();
        XmlAttributeCollection srcXmlAtt = node.Attributes;
        if (srcXmlAtt != null)
        {
            for (int i = 0; i < srcXmlAtt.Count; ++i)
                xmlAttributes.add(srcXmlAtt[i].Name, srcXmlAtt[i].Value);
        }

        handler.elementStart(node.Name, xmlAttributes);

        XmlNodeList childNodeList = node.ChildNodes;
        for (int i = 0; i < childNodeList.Count; ++i)
            parserNode(childNodeList[i], handler);

        handler.elementEnd(node.Name);
    }
};
