using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ep.App.Web.Core
{
    /// <summary>
    /// It seems OpenXML SDK uses .NET Uri internaly and it throws exception when url is malformed. 
    /// Only workaround was to find all malformed links and remove them before opening doument with OpenXML. 
    /// https://social.msdn.microsoft.com/Forums/windowsserver/en-US/8147fa29-6e55-4f4e-9d60-c07ea3779042/how-to-validate-a-word-document-using-openxml-sdk-25?forum=oxmlsdk
    /// </summary>
    public class PreOpenXmlProcessor
    {
        private const string SlideXmlRelsRegexString = @"/ppt/(slides|slideLayouts|slideMasters)/_rels/(slide|slideLayout|slideMaster)\d+\.xml.rels";


        public static void Process(Package package)
        {
            ProcessBasedRelationshipsUris(package);
        }

        private static void ProcessBasedRelationshipsUris(Package package)
        {
            bool needToFlushPackage = false;
            foreach (PackagePart part in package.GetParts())
            {
                if (IsRelationshipsFile(part.Uri.OriginalString))
                {
                    XmlDocument document = new XmlDocument();
                    bool needToSaveDocument = FindAndReplaceBadRelationshipUris(document, part);
                    needToFlushPackage = needToFlushPackage || needToSaveDocument;
                    if (needToSaveDocument)
                    {
                        SaveXmlDocument(document, part);
                    }
                }

                if (part.Uri.OriginalString.Contains("sheet"))
                {
                    XmlDocument document = new XmlDocument();
                    bool needToSaveDocument = FindAndReplaceBadRelationshipUris(document, part);
                    needToFlushPackage = needToFlushPackage || needToSaveDocument;
                    if (needToSaveDocument)
                    {
                        SaveXmlDocument(document, part);
                    }
                }
            }

            if (needToFlushPackage) package.Flush();
        }

        private static void SaveXmlDocument(XmlDocument document, PackagePart part)
        {
            using (Stream stream = part.GetStream(FileMode.Create, FileAccess.Write))
            {
                document.Save(stream);
            }
        }

        private static bool FindAndReplaceBadRelationshipUris(XmlDocument document, PackagePart part)
        {
            bool foundBadRelationships = false;

            using (Stream stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite))
            {
                document.Load(stream);
                XmlNodeList nodes = document.GetElementsByTagName("Relationship");

                foreach (XmlNode node in nodes)
                {
                    if (!IsUriValid(node))
                    {
                        ReplaceWithDummyRelationshipUri(node);
                        foundBadRelationships = true;
                    }
                }
            }

            return foundBadRelationships;
        }

        private static bool FindAndReplaceBadHeperlinksUris(XmlDocument document, PackagePart part)
        {
            bool foundBadRelationships = false;

            using (Stream stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite))
            {
                document.Load(stream);
                XmlNodeList nodes = document.GetElementsByTagName("hyperlink");
                List<XmlNode> toRemove = new List<XmlNode>();
                foreach (XmlNode node in nodes)
                {
                    toRemove.Add(node);
                    foundBadRelationships = true;
                }

                foreach (XmlNode node in toRemove)
                    node.ParentNode.RemoveChild(node);
            }

            return foundBadRelationships;
        }

        private static bool IsRelationshipsFile(string filename)
        {
            return Regex.IsMatch(filename, SlideXmlRelsRegexString);
        }

        private static bool IsUriValid(XmlNode node)
        {
            XmlAttribute targetAttr = node.Attributes["Target"];
            Uri uri;
            if (!Uri.TryCreate(targetAttr.Value, UriKind.RelativeOrAbsolute, out uri))
            {
                return false;
            }

            return true;
        }

        private static void ReplaceWithDummyRelationshipUri(XmlNode node)
        {
            XmlAttribute targetAttr = node.Attributes["Target"];
            targetAttr.Value = "about:blank";
        }
    }
}