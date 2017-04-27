using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace NUnitTestProjectSplitter.Entities {

	public sealed partial class NUnitTestProject {

		public void Save( string filePath ) {
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;

			using( XmlWriter xml = XmlWriter.Create( filePath, settings ) ) {

				xml.WriteStartElement( "NUnitProject" );

				Dictionary<string, IEnumerable<string>> configs = Assemblies
					// decompose
					.SelectMany( assmblyItem => assmblyItem.Value.Select( configName => new KeyValuePair<string, string>( configName, assmblyItem.Key ) ) )
					// group by config name
					.GroupBy( x => x.Key )
					.ToDictionary( x => x.Key, x => x.Select( assemblyItem => assemblyItem.Value ) );
					
				if( !String.IsNullOrEmpty( ActiveConfig ) ) {
					if( configs.ContainsKey( ActiveConfig ) ) {

						xml.WriteStartElement( "Settings" );
						xml.WriteAttributeString( "activeconfig", ActiveConfig );
						xml.WriteEndElement();
					}
				}

				foreach( var config in configs ) {

					xml.WriteStartElement( "Config" );
					xml.WriteAttributeString( "name", config.Key );
					xml.WriteAttributeString( "binpathtype", "Auto" );
					{
						var assemblies = config.Value.ToList();
						assemblies.Sort( StringComparer.OrdinalIgnoreCase );

						foreach( string assembly in assemblies ) {

							xml.WriteStartElement( "assembly" );
							xml.WriteAttributeString( "path", assembly );
							xml.WriteEndElement();
						}
					}
					xml.WriteEndElement();
				}

				xml.WriteEndElement();
			}

		}

		public void Load( string filePath ) {

			using( var stream = new FileStream( filePath, FileMode.Open ) ) {
				using( var reader = XmlReader.Create( stream ) ) {
					XDocument doc = XDocument.Load( reader );

					XElement activeConfigElement = doc.Root.Element( XName.Get( "Settings" ) );
					ActiveConfig = activeConfigElement?.Attribute( XName.Get( "activeconfig" ) )?.Value;

					foreach( var configSectionElement in doc.Root.Elements( XName.Get( "Config" ) ) ) {
						string configName = configSectionElement.Attribute( XName.Get( "name" ) )?.Value;
						string binpathtype = configSectionElement.Attribute( XName.Get( "binpathtype" ) )?.Value;

						foreach( var assemblyElement in configSectionElement.Elements( XName.Get( "assembly" ) ) ) {
							string path = assemblyElement.Attribute( XName.Get( "path" ) )?.Value;
							Add( configName, path );
						}
					}
				}
			}
		}

		public static NUnitTestProject LoadFromFile( string filePath ) {
			var proj = new NUnitTestProject();
			proj.Load( filePath );
			return proj;
		}

	}
}