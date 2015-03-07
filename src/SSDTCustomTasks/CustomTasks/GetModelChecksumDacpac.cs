using System;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CustomTasks
{
    public class GetModelChecksumDacpac : Task
    {
        [Required]
        public string DacpacFile
        { get; set; }

        [Output]
        public string ModelChecksum
        { get; set; }


        public override bool Execute()
        {
            Log.LogMessage("Getting checksum for model.xml Dacpac: {0}", DacpacFile);

            ModelChecksum = string.Format("-1");

            try
            {
                Log.LogMessage("Dacpac file: {0}", DacpacFile);

                Package dacpac = Package.Open(DacpacFile, FileMode.Open, FileAccess.Read);

                try
                {
                    Uri originUri = PackUriHelper.CreatePartUri(new Uri(Constants.OriginXmlUri, UriKind.Relative));
                    PackagePart dacOriginPart = dacpac.GetPart(originUri);

                    // Load the xml file into an XmlDocument for ease of use.
                    XmlDocument dacOriginXml = new XmlDocument();
                    dacOriginXml.Load(XmlReader.Create(dacOriginPart.GetStream()));

                    XmlNamespaceManager xmlns = new XmlNamespaceManager(dacOriginXml.NameTable);
                    xmlns.AddNamespace("dac", Constants.DacOriginXmlns);

                    // Assumption the Checksums Element always exists when the dacpac is built.
                    XmlNode _modelchecksum = dacOriginXml.SelectSingleNode("/dac:DacOrigin/dac:Checksums/dac:Checksum[@Uri='/model.xml']", xmlns);

                    if (_modelchecksum == null ||
                        string.IsNullOrEmpty(_modelchecksum.InnerText.ToString()))
                    {
                        ModelChecksum = string.Format("-1");
                    }
                    else
                    {
                        ModelChecksum = string.Format(_modelchecksum.InnerText.ToString());
                    }
                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    dacpac.Close();
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return true;
        }
    }
}
