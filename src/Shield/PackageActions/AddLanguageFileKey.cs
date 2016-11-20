using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using Umbraco.Core.Logging;
using spa = umbraco.cms.businesslogic.packager.standardPackageActions;

namespace Shield.PackageActions
{
    class AddLanguageFileKey : IPackageAction
    {
        private const string DirPath = "~Umbraco/Config/Lang/";

        public string Alias()
        {
            return "AddLanguageFileKey";
        }

        public bool Execute(string packageName, XmlNode xmlData)
        {
            //Set result default to false
            bool result = false;

            //Get attribute values of xmlData
            string language, position, area, key, value;
            language = getAttributeDefault(xmlData, "language", "en");
            position = getAttributeDefault(xmlData, "position", null);
            if (!getAttribute(xmlData, "area", out area)) return result;
            if (!getAttribute(xmlData, "key", out key)) return result;
            if (!getAttribute(xmlData, "value", out value)) return result;
            
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.Load(HttpContext.Current.Server.MapPath(DirPath + language + ".xml"));

            //Select root node in the web.config file to insert new node
            //RS: We need to ensure that the area rootnode exists
            XmlNode rootNode = EnsureAreaRootNode(area, document);
            
            if (rootNode == null) return result;
            bool modified = false;
            bool insertNode = true;
            
            if (rootNode.HasChildNodes)
            {
                XmlNode node = rootNode.SelectSingleNode($"//key[@alias = '{key}']");
                if (node != null)
                {
                    insertNode = false;
                }
            }

            if (insertNode)
            {
                XmlNode newNode = document.CreateElement("key");
                newNode.Attributes.Append(xmlHelper.addAttribute(document, "alias", key));
                newNode.InnerText = value;
                
                if (position == null || position == "end")
                {
                    rootNode.AppendChild(newNode);
                    modified = true;
                }
                else if (position == "beginning")
                {
                    rootNode.PrependChild(newNode);
                    modified = true;
                }
            }
            
            if (modified)
            {
                try
                {
                    document.Save(HttpContext.Current.Server.MapPath(DirPath + language + ".xml"));
                    result = true;
                }
                catch (Exception ex)
                {
                    string message = "Error at execute AddLanguageFileKey package action: " + ex.Message;
                    LogHelper.Error<AddLanguageFileKey>(message, ex);
                }
            }
            return result;
        }

        /// <summary>
        /// Return the area root node.
        /// When the area rootnode doesn't exist the area will be created
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        private XmlNode EnsureAreaRootNode(string area, XmlDocument document)
        {
            XmlNode rootNode = document.SelectSingleNode(String.Format("//language/area[@alias = '{0}']", area));
            if (rootNode == null)
            {
                rootNode = CreateRootNode(area, document);
            }
            return rootNode;
        }

        /// <summary>
        /// Creates the area node.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        private XmlNode CreateRootNode(string area, XmlDocument document)
        {
            //Create the rootnode
            XmlNode node = document.CreateElement("area");
            //Append alias
            node.Attributes.Append(xmlHelper.addAttribute(document, "alias", area));

            //append the rootnode to xml
            document.DocumentElement.AppendChild(node);

            return node;
        }


        public XmlNode SampleXml()
        {
            return spa.helper.parseStringToXmlNode(
                "<Action runat=\"install\" undo=\"true/false\" alias=\"AddLanguageFileKey\" "
                    + "language=\"en/da/de/es/fr/it/nl/no/se/sv\" "
                    + "position=\"beginning/end\" "
                    + "area=\"sections\" "
                    + "key=\"shield\" "
                    + "value=\"Shield\" />"
            );

        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            //Set result default to false
            bool result = false;

            //Get attribute values of xmlData
            string language, area, key;
            language = getAttributeDefault(xmlData, "language", "en");
            if (!getAttribute(xmlData, "area", out area)) return result;
            if (!getAttribute(xmlData, "key", out key)) return result;
            
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.Load(HttpContext.Current.Server.MapPath(DirPath + language + ".xml"));
            XmlNode rootNode = document.SelectSingleNode($"//language/area[@alias = '{area}']");
            
            if (rootNode == null) return result;
            bool modified = false;

            //Look for existing nodes with same path of undo attribute
            if (rootNode.HasChildNodes)
            {
                //Look for existing add nodes with attribute path
                foreach (XmlNode existingNode in rootNode.SelectNodes($"//key[@alias = '{key}']"))
                {
                    //Remove existing node from root node
                    rootNode.RemoveChild(existingNode);
                    modified = true;
                }
            }

            if (modified)
            {
                try
                {
                    document.Save(HttpContext.Current.Server.MapPath(DirPath + language + ".xml"));
                    result = true;
                }
                catch (Exception ex)
                {
                    string message = "Error in the undo section of the AddLanguageFileKey package action: " + ex.Message;
                    LogHelper.Error<AddLanguageFileKey>(message, ex);
                }
            }
            return result;
        }

        /// <summary>
        /// Get a named attribute from xmlData root node
        /// </summary>
        /// <param name="xmlData">The data that must be appended to the web.config file</param>
        /// <param name="attribute">The name of the attribute</param>
        /// <param name="value">returns the attribute value from xmlData</param>
        /// <returns>True, when attribute value available</returns>
        private bool getAttribute(XmlNode xmlData, string attribute, out string value)
        {
            bool result = false;
            value = string.Empty;
            XmlAttribute xmlAttribute = xmlData.Attributes[attribute];
            
            if (xmlAttribute != null)
            {
                value = xmlAttribute.Value;
                result = true;
            }
            else
            {
                string message = "Error at AddLanguageFileKey package action: Attribute \"" + attribute + "\" not found.";
                LogHelper.Error<AddLanguageFileKey>(message, null);
            }
            return result;
        }

        /// <summary>
        /// Get an optional named attribute from xmlData root node
        /// when attribute is unavailable, return the default value
        /// </summary>
        /// <param name="xmlData">The data that must be appended to the web.config file</param>
        /// <param name="attribute">The name of the attribute</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>The attribute value or the default value</returns>
        private string getAttributeDefault(XmlNode xmlData, string attribute, string defaultValue)
        {
            string result = defaultValue;
            XmlAttribute xmlAttribute = xmlData.Attributes[attribute];
            
            if (xmlAttribute != null)
            {
                result = xmlAttribute.Value;
            }
            return result;
        }

    }
}
