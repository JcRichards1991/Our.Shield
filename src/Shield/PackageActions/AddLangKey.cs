using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using umbraco.interfaces;

namespace Shield.PackageActions
{
    class AddLangKey : IPackageAction
    {
        public string Alias()
        {
            throw new NotImplementedException();
        }

        public bool Execute(string packageName, XmlNode xmlData)
        {
            throw new NotImplementedException();
        }

        public XmlNode SampleXml()
        {
            throw new NotImplementedException();
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            throw new NotImplementedException();
        }
    }
}
