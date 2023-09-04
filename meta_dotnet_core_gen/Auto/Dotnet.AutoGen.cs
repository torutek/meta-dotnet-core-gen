// ******************************************************************************************************************************
// Filename:    Dotnet.AutoGen.cs
// Description:
// ******************************************************************************************************************************
// Copyright © Richard Dunkley 2023
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ******************************************************************************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace meta_dotnet_core_gen.Auto
{
	//***************************************************************************************************************************
	/// <summary>In memory representation of .Net download runtime information.</summary>
	//***************************************************************************************************************************
	public partial class Dotnet
	{
		#region Classes

		//***********************************************************************************************************************
		/// <summary>Represents the various runtimes that can be downloaded.</summary>
		//***********************************************************************************************************************
		public partial class Runtime
		{
			#region Classes

			//*******************************************************************************************************************
			/// <summary>In memory representation of a specific build of the runtime.</summary>
			//*******************************************************************************************************************
			public partial class Build
			{
				#region Classes

				//***************************************************************************************************************
				/// <summary>Architecture supported by the version.</summary>
				//***************************************************************************************************************
				public partial class Arch
				{
					#region Enumerations

					//***********************************************************************************************************
					/// <summary>Enumerates the possible values of Target.</summary>
					//***********************************************************************************************************
					public enum TargetEnum
					{
						#region Names

						//*******************************************************************************************************
						/// <summary>Represents the 'arm' string.</summary>
						//*******************************************************************************************************
						Arm,

						//*******************************************************************************************************
						/// <summary>Represents the 'arm64' string.</summary>
						//*******************************************************************************************************
						Arm64,

						//*******************************************************************************************************
						/// <summary>Represents the 'x64' string.</summary>
						//*******************************************************************************************************
						X64,

						#endregion Names
					}

					#endregion Enumerations

					#region Properties

					//***********************************************************************************************************
					/// <summary>Direct download link to the binaries.</summary>
					//***********************************************************************************************************
					public string Link { get; set; }

					//***********************************************************************************************************
					/// <summary>Gets or sets the MD5 value of the binaries. Can be null.</summary>
					//***********************************************************************************************************
					public string Md5 { get; set; }

					//***********************************************************************************************************
					/// <summary>
					///   Gets the index of this object in relation to the other child element of this object's parent.
					/// </summary>
					///
					/// <remarks>
					///   If the value is -1, then this object was not created from an XML node and the property has not been
					///   set.
					/// </remarks>
					//***********************************************************************************************************
					public int Ordinal { get; set; }

					//***********************************************************************************************************
					/// <summary>Gets or sets the SHA256 checksum of the download binaries. Can be null.</summary>
					//***********************************************************************************************************
					public string Sha256 { get; set; }

					//***********************************************************************************************************
					/// <summary>Gets or sets the SHA512 hash of the binaries. Can be null.</summary>
					//***********************************************************************************************************
					public string Sha512 { get; set; }

					//***********************************************************************************************************
					/// <summary>Gets or sets the target cpu of the architecture.</summary>
					//***********************************************************************************************************
					public TargetEnum Target { get; set; }

					#endregion Properties

					#region Methods

					//***********************************************************************************************************
					/// <overloads><summary>Instantiates a new <see cref="Arch"/> object.</summary></overloads>
					///
					/// <summary>Instantiates a new <see cref="Arch"/> object using the provided information.</summary>
					///
					/// <param name="link">'link' String attribute contained in the XML element.</param>
					/// <param name="md5">'md5' String attribute contained in the XML element. Can be null.</param>
					/// <param name="sha256">'sha256' String attribute contained in the XML element. Can be null.</param>
					/// <param name="sha512">'sha512' String attribute contained in the XML element. Can be null.</param>
					/// <param name="target">'target' Custom Enumeration attribute contained in the XML element.</param>
					///
					/// <exception cref="ArgumentException">
					///   <paramref name="link"/>, <paramref name="md5"/>, <paramref name="sha256"/>, or <paramref
					///   name="sha512"/> is an empty array.
					/// </exception>
					/// <exception cref="ArgumentNullException"><paramref name="link"/> is a null reference.</exception>
					//***********************************************************************************************************
					public Arch(string link, string md5, string sha256, string sha512, TargetEnum target)
					{
						if(link == null)
							throw new ArgumentNullException("link");
						if(link.Length == 0)
							throw new ArgumentException("link is empty");
						if(md5 != null && md5.Length == 0)
							throw new ArgumentException("md5 is empty");
						if(sha256 != null && sha256.Length == 0)
							throw new ArgumentException("sha256 is empty");
						if(sha512 != null && sha512.Length == 0)
							throw new ArgumentException("sha512 is empty");
						Link = link;
						Md5 = md5;
						Sha256 = sha256;
						Sha512 = sha512;
						Target = target;
						Ordinal = -1;
					}

					//***********************************************************************************************************
					/// <summary>Instantiates a new <see cref="Arch"/> object from an <see cref="XmlNode"/> object.</summary>
					///
					/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
					/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
					///
					/// <exception cref="ArgumentException">
					///   <paramref name="node"/> does not correspond to a arch node or is not an 'Element' type node or
					///   <paramref name="ordinal"/> is negative.
					/// </exception>
					/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
					/// <exception cref="InvalidDataException">
					///   An error occurred while reading the data into the node, or one of it's child nodes.
					/// </exception>
					//***********************************************************************************************************
					public Arch(XmlNode node, int ordinal)
					{
						if(node == null)
							throw new ArgumentNullException("node");
						if(ordinal < 0)
							throw new ArgumentException("the ordinal specified is negative.");
						if(node.NodeType != XmlNodeType.Element)
							throw new ArgumentException("node is not of type 'Element'.");

						ParseXmlNode(node, ordinal);
					}

					//***********************************************************************************************************
					/// <summary>
					///   Creates an XML element for this object using the provided <see cref="XmlDocument"/> object.
					/// </summary>
					///
					/// <param name="doc"><see cref="XmlDocument"/> object to generate the element from.</param>
					///
					/// <returns><see cref="XmlElement"/> object containing this classes data.</returns>
					///
					/// <exception cref="ArgumentNullException"><paramref name="doc"/> is a null reference.</exception>
					//***********************************************************************************************************
					public XmlElement CreateElement(XmlDocument doc)
					{
						if(doc == null)
							throw new ArgumentNullException("doc");
						XmlElement returnElement = doc.CreateElement("arch");

						string valueString;

						// link
						valueString = GetLinkString();
						returnElement.SetAttribute("link", valueString);

						// md5
						valueString = GetMd5String();
						if(valueString != null)
							returnElement.SetAttribute("md5", valueString);

						// sha256
						valueString = GetSha256String();
						if(valueString != null)
							returnElement.SetAttribute("sha256", valueString);

						// sha512
						valueString = GetSha512String();
						if(valueString != null)
							returnElement.SetAttribute("sha512", valueString);

						// target
						valueString = GetTargetString();
						returnElement.SetAttribute("target", valueString);
						return returnElement;
					}

					//***********************************************************************************************************
					/// <summary>Gets a string representation of Link.</summary>
					///
					/// <returns>String representing the value.</returns>
					//***********************************************************************************************************
					public string GetLinkString()
					{
						return Link;
					}

					//***********************************************************************************************************
					/// <summary>Gets a string representation of Md5.</summary>
					///
					/// <returns>String representing the value. Can be null.</returns>
					//***********************************************************************************************************
					public string GetMd5String()
					{
						return Md5;
					}

					//***********************************************************************************************************
					/// <summary>Gets a string representation of Sha256.</summary>
					///
					/// <returns>String representing the value. Can be null.</returns>
					//***********************************************************************************************************
					public string GetSha256String()
					{
						return Sha256;
					}

					//***********************************************************************************************************
					/// <summary>Gets a string representation of Sha512.</summary>
					///
					/// <returns>String representing the value. Can be null.</returns>
					//***********************************************************************************************************
					public string GetSha512String()
					{
						return Sha512;
					}

					//***********************************************************************************************************
					/// <summary>Gets a string representation of Target.</summary>
					///
					/// <returns>String representing the value.</returns>
					//***********************************************************************************************************
					public string GetTargetString()
					{

						switch(Target)
						{
							case TargetEnum.Arm:
								return "arm";
							case TargetEnum.Arm64:
								return "arm64";
							case TargetEnum.X64:
								return "x64";
							default:
								throw new NotImplementedException("The enumerated type was not recognized as a supported type.");
						}
					}

					//***********************************************************************************************************
					/// <summary>Parses an XML node and populates the data into this object.</summary>
					///
					/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
					/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
					///
					/// <exception cref="ArgumentException">
					///   <paramref name="node"/> does not correspond to a arch node.
					/// </exception>
					/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
					/// <exception cref="InvalidDataException">
					///   An error occurred while reading the data into the node, or one of it's child nodes.
					/// </exception>
					//***********************************************************************************************************
					public void ParseXmlNode(XmlNode node, int ordinal)
					{
						if(node == null)
							throw new ArgumentNullException("node");
						if(string.Compare(node.Name, "arch", false) != 0)
							throw new ArgumentException("node does not correspond to a arch node.");

						XmlAttribute attrib;

						// link
						attrib = node.Attributes["link"];
						if(attrib == null)
							throw new InvalidDataException("An XML string Attribute (link) is not optional, but was not found in"
								+ " the XML element (arch).");
						SetLinkFromString(attrib.Value);

						// md5
						attrib = node.Attributes["md5"];
						if(attrib == null)
							Md5 = null;
						else
							SetMd5FromString(attrib.Value);

						// sha256
						attrib = node.Attributes["sha256"];
						if(attrib == null)
							Sha256 = null;
						else
							SetSha256FromString(attrib.Value);

						// sha512
						attrib = node.Attributes["sha512"];
						if(attrib == null)
							Sha512 = null;
						else
							SetSha512FromString(attrib.Value);

						// target
						attrib = node.Attributes["target"];
						if(attrib == null)
							throw new InvalidDataException("An XML string Attribute (target) is not optional, but was not found"
								+ " in the XML element (arch).");
						SetTargetFromString(attrib.Value);
						Ordinal = ordinal;
					}

					//***********************************************************************************************************
					/// <summary>Parses a string value and stores the data in Link.</summary>
					///
					/// <param name="value">String representation of the value.</param>
					///
					/// <exception cref="InvalidDataException">
					///   <list type="bullet">
					///     <listheader>One of the following:</listheader>
					///     <item>The string value is a null reference or an empty string.</item>
					///     <item>The string value could not be parsed.</item>
					///   </list>
					/// </exception>
					//***********************************************************************************************************
					public void SetLinkFromString(string value)
					{
						if(value == null)
							throw new InvalidDataException("The string value for 'link' is a null reference.");
						if(value.Length == 0)
							throw new InvalidDataException("The string value for 'link' is an empty string.");
						Link = value;
					}

					//***********************************************************************************************************
					/// <summary>Parses a string value and stores the data in Md5.</summary>
					///
					/// <param name="value">String representation of the value.</param>
					///
					/// <exception cref="InvalidDataException">
					///   <list type="bullet">
					///     <listheader>One of the following:</listheader>
					///     <item>The string value is an empty string.</item>
					///     <item>The string value could not be parsed.</item>
					///   </list>
					/// </exception>
					//***********************************************************************************************************
					public void SetMd5FromString(string value)
					{
						if(value == null)
						{
							Md5 = null;
							return;
						}
						if(value.Length == 0)
							throw new InvalidDataException("The string value for 'md5' is an empty string.");
						if(value.Length < 32)
							throw new InvalidDataException(string.Format("The 'md5' attribute provided ({0}) does not meet the"
								+ " minimum length requirement (32).", value));
						if(value.Length > 32)
							throw new InvalidDataException(string.Format("The 'md5' attribute provided ({0}) exceeds the maximum"
								+ " length requirement (32).", value));
						Md5 = value;
					}

					//***********************************************************************************************************
					/// <summary>Parses a string value and stores the data in Sha256.</summary>
					///
					/// <param name="value">String representation of the value.</param>
					///
					/// <exception cref="InvalidDataException">
					///   <list type="bullet">
					///     <listheader>One of the following:</listheader>
					///     <item>The string value is an empty string.</item>
					///     <item>The string value could not be parsed.</item>
					///   </list>
					/// </exception>
					//***********************************************************************************************************
					public void SetSha256FromString(string value)
					{
						if(value == null)
						{
							Sha256 = null;
							return;
						}
						if(value.Length == 0)
							throw new InvalidDataException("The string value for 'sha256' is an empty string.");
						if(value.Length < 64)
							throw new InvalidDataException(string.Format("The 'sha256' attribute provided ({0}) does not meet"
								+ " the minimum length requirement (64).", value));
						if(value.Length > 64)
							throw new InvalidDataException(string.Format("The 'sha256' attribute provided ({0}) exceeds the"
								+ " maximum length requirement (64).", value));
						Sha256 = value;
					}

					//***********************************************************************************************************
					/// <summary>Parses a string value and stores the data in Sha512.</summary>
					///
					/// <param name="value">String representation of the value.</param>
					///
					/// <exception cref="InvalidDataException">
					///   <list type="bullet">
					///     <listheader>One of the following:</listheader>
					///     <item>The string value is an empty string.</item>
					///     <item>The string value could not be parsed.</item>
					///   </list>
					/// </exception>
					//***********************************************************************************************************
					public void SetSha512FromString(string value)
					{
						if(value == null)
						{
							Sha512 = null;
							return;
						}
						if(value.Length == 0)
							throw new InvalidDataException("The string value for 'sha512' is an empty string.");
						if(value.Length < 128)
							throw new InvalidDataException(string.Format("The 'sha512' attribute provided ({0}) does not meet"
								+ " the minimum length requirement (128).", value));
						if(value.Length > 128)
							throw new InvalidDataException(string.Format("The 'sha512' attribute provided ({0}) exceeds the"
								+ " maximum length requirement (128).", value));
						Sha512 = value;
					}

					//***********************************************************************************************************
					/// <summary>Parses a string value and stores the data in Target.</summary>
					///
					/// <param name="value">String representation of the value.</param>
					///
					/// <exception cref="InvalidDataException">
					///   <list type="bullet">
					///     <listheader>One of the following:</listheader>
					///     <item>The string value is a null reference or an empty string.</item>
					///     <item>The string value could not be parsed.</item>
					///   </list>
					/// </exception>
					//***********************************************************************************************************
					public void SetTargetFromString(string value)
					{
						if(value == null)
							throw new InvalidDataException("The string value for 'target' is a null reference.");
						if(value.Length == 0)
							throw new InvalidDataException("The string value for 'target' is an empty string.");
						if(string.Compare(value, "arm", false) == 0)
						{
							Target = TargetEnum.Arm;
							return;
						}
						if(string.Compare(value, "arm64", false) == 0)
						{
							Target = TargetEnum.Arm64;
							return;
						}
						if(string.Compare(value, "x64", false) == 0)
						{
							Target = TargetEnum.X64;
							return;
						}
						throw new InvalidDataException(string.Format("The enum value specified ({0}) is not a recognized"
							+ " enumerated type for target.", value));
					}

					#endregion Methods
				}

				#endregion Classes

				#region Properties

				//***************************************************************************************************************
				/// <summary>Gets or sets the child XML elements.</summary>
				//***************************************************************************************************************
				public Arch[] ChildArchs { get; private set; }

				//***************************************************************************************************************
				/// <summary>
				///   Gets the index of this object in relation to the other child element of this object's parent.
				/// </summary>
				///
				/// <remarks>
				///   If the value is -1, then this object was not created from an XML node and the property has not been set.
				/// </remarks>
				//***************************************************************************************************************
				public int Ordinal { get; set; }

				//***************************************************************************************************************
				/// <summary>Gets or sets the version of the build (build component)</summary>
				//***************************************************************************************************************
				public int Version { get; set; }

				#endregion Properties

				#region Methods

				//***************************************************************************************************************
				/// <overloads><summary>Instantiates a new <see cref="Build"/> object.</summary></overloads>
				///
				/// <summary>Instantiates a new <see cref="Build"/> object using the provided information.</summary>
				///
				/// <param name="version">'version' 32-bit signed integer attribute contained in the XML element.</param>
				/// <param name="childArchs">Array of arch elements which are child elements of this node. Can be empty.</param>
				///
				/// <exception cref="ArgumentNullException"><paramref name="childArchs"/> is a null reference.</exception>
				//***************************************************************************************************************
				public Build(int version, Arch[] childArchs)
				{
					if(childArchs == null)
						throw new ArgumentNullException("childArchs");
					Version = version;
					ChildArchs = childArchs;
					Ordinal = -1;

					// Compute the maximum index used on any child items.
					int maxIndex = 0;
					foreach(Arch item in ChildArchs)
					{
						if(item.Ordinal >= maxIndex)
							maxIndex = item.Ordinal + 1; // Set to first index after this index.
					}

					// Assign ordinal for any child items that don't have it set (-1).
					foreach(Arch item in ChildArchs)
					{
						if(item.Ordinal == -1)
							item.Ordinal = maxIndex++;
					}
				}

				//***************************************************************************************************************
				/// <summary>Instantiates a new <see cref="Build"/> empty object.</summary>
				///
				/// <param name="version">'version' 32-bit signed integer attribute contained in the XML element.</param>
				//***************************************************************************************************************
				public Build(int version)
				{
					Version = version;
					ChildArchs = new Arch[0];
					Ordinal = -1;
				}

				//***************************************************************************************************************
				/// <summary>Instantiates a new <see cref="Build"/> object from an <see cref="XmlNode"/> object.</summary>
				///
				/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
				/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
				///
				/// <exception cref="ArgumentException">
				///   <paramref name="node"/> does not correspond to a build node or is not an 'Element' type node or <paramref
				///   name="ordinal"/> is negative.
				/// </exception>
				/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
				/// <exception cref="InvalidDataException">
				///   An error occurred while reading the data into the node, or one of it's child nodes.
				/// </exception>
				//***************************************************************************************************************
				public Build(XmlNode node, int ordinal)
				{
					if(node == null)
						throw new ArgumentNullException("node");
					if(ordinal < 0)
						throw new ArgumentException("the ordinal specified is negative.");
					if(node.NodeType != XmlNodeType.Element)
						throw new ArgumentException("node is not of type 'Element'.");

					ParseXmlNode(node, ordinal);
				}

				//***************************************************************************************************************
				/// <summary>Adds a <see cref="Arch"/> to <see cref="ChildArchs"/>.</summary>
				///
				/// <param name="item"><see cref="Arch"/> to be added. If null, then no changes will occur. Can be null.</param>
				//***************************************************************************************************************
				public void AddArch(Arch item)
				{
					if (item == null) return;

					// Compute the maximum index used on any child items.
					int maxIndex = 0;
					foreach(Arch child in ChildArchs)
					{
						if (child.Ordinal >= maxIndex)
							maxIndex = child.Ordinal + 1; // Set to first index after this index.
					}

					var list = new List<Arch>(ChildArchs);
					list.Add(item);
					item.Ordinal = maxIndex;
					ChildArchs = list.ToArray();
				}

				//***************************************************************************************************************
				/// <summary>
				///   Creates an XML element for this object using the provided <see cref="XmlDocument"/> object.
				/// </summary>
				///
				/// <param name="doc"><see cref="XmlDocument"/> object to generate the element from.</param>
				///
				/// <returns><see cref="XmlElement"/> object containing this classes data.</returns>
				///
				/// <exception cref="ArgumentNullException"><paramref name="doc"/> is a null reference.</exception>
				//***************************************************************************************************************
				public XmlElement CreateElement(XmlDocument doc)
				{
					if(doc == null)
						throw new ArgumentNullException("doc");
					XmlElement returnElement = doc.CreateElement("build");

					string valueString;

					// version
					valueString = GetVersionString();
					returnElement.SetAttribute("version", valueString);
					// Build up dictionary of indexes and corresponding items.
					Dictionary<int, object> lookup = new Dictionary<int, object>();

					foreach(Arch child in ChildArchs)
					{
						if(lookup.ContainsKey(child.Ordinal))
							throw new InvalidOperationException("An attempt was made to generate the XML element with two child"
								+ " elements with the same ordinal.Ordinals must be unique across all child objects.");
						lookup.Add(child.Ordinal, child);
					}

					// Sort the keys.
					List<int> keys = lookup.Keys.ToList();
					keys.Sort();

					foreach (int key in keys)
					{
						if(lookup[key] is Arch)
							returnElement.AppendChild(((Arch)lookup[key]).CreateElement(doc));
					}
					return returnElement;
				}

				//***************************************************************************************************************
				/// <summary>Gets a string representation of Version.</summary>
				///
				/// <returns>String representing the value.</returns>
				//***************************************************************************************************************
				public string GetVersionString()
				{

					return Version.ToString();
				}

				//***************************************************************************************************************
				/// <summary>Parses an XML node and populates the data into this object.</summary>
				///
				/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
				/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
				///
				/// <exception cref="ArgumentException"><paramref name="node"/> does not correspond to a build node.</exception>
				/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
				/// <exception cref="InvalidDataException">
				///   An error occurred while reading the data into the node, or one of it's child nodes.
				/// </exception>
				//***************************************************************************************************************
				public void ParseXmlNode(XmlNode node, int ordinal)
				{
					if(node == null)
						throw new ArgumentNullException("node");
					if(string.Compare(node.Name, "build", false) != 0)
						throw new ArgumentException("node does not correspond to a build node.");

					XmlAttribute attrib;

					// version
					attrib = node.Attributes["version"];
					if(attrib == null)
						throw new InvalidDataException("An XML string Attribute (version) is not optional, but was not found in"
							+ " the XML element (build).");
					SetVersionFromString(attrib.Value);

					// Read the child objects.
					List<Arch> childArchsList = new List<Arch>();
					int index = 0;
					foreach(XmlNode child in node.ChildNodes)
					{
						if(child.NodeType == XmlNodeType.Element && child.Name == "arch")
							childArchsList.Add(new Arch(child, index++));
					}
					ChildArchs = childArchsList.ToArray();

					Ordinal = ordinal;
				}

				//***************************************************************************************************************
				/// <summary>Removes a <see cref="Arch"/> from <see cref="ChildArchs"/>.</summary>
				///
				/// <param name="item"><see cref="Arch"/> to be removed. Can be null.</param>
				//***************************************************************************************************************
				public void RemoveArch(Arch item)
				{
					if (item == null) return;

					var list = new List<Arch>(ChildArchs);
					list.Remove(item);
					ChildArchs = list.ToArray();
				}

				//***************************************************************************************************************
				/// <summary>Parses a string value and stores the data in Version.</summary>
				///
				/// <param name="value">String representation of the value.</param>
				///
				/// <exception cref="InvalidDataException">
				///   <list type="bullet">
				///     <listheader>One of the following:</listheader>
				///     <item>The string value is a null reference or an empty string.</item>
				///     <item>The string value could not be parsed.</item>
				///   </list>
				/// </exception>
				//***************************************************************************************************************
				public void SetVersionFromString(string value)
				{
					if(value == null)
						throw new InvalidDataException("The string value for 'version' is a null reference.");
					if(value.Length == 0)
						throw new InvalidDataException("The string value for 'version' is an empty string.");
					int returnValue = 0;
					bool parsed = false;
					try
					{

						// Attempt to parse the number as just an integer.
						returnValue = int.Parse(value, NumberStyles.Integer | NumberStyles.AllowThousands);
						parsed = true;
					}
					catch(FormatException e)
					{
						throw new InvalidDataException(string.Format("The int value specified ({0}) is not in a valid int string"
							+ " format: {1}.", value, e.Message), e);
					}
					catch(OverflowException e)
					{
						throw new InvalidDataException(string.Format("The int value specified ({0}) was larger or smaller than a"
							+ " int value (Min: {1}, Max: {2}).", value, int.MinValue.ToString(), int.MaxValue.ToString()), e);
					}

					if(!parsed)
						throw new InvalidDataException(string.Format("The int value specified ({0}) is not in a valid int string"
							+ " format.", value));

					// Verify that the int value is not lower than the minimum size.
					if(returnValue < 0)
						throw new InvalidDataException(string.Format("The int value specified ({0}) was less than the minimum"
							+ " value allowed for this type (0).", value));

					Version = returnValue;
				}

				#endregion Methods
			}

			#endregion Classes

			#region Enumerations

			//*******************************************************************************************************************
			/// <summary>Enumerates the possible values of Name.</summary>
			//*******************************************************************************************************************
			public enum NameEnum
			{
				#region Names

				//***************************************************************************************************************
				/// <summary>Represents the 'aspnet' string.</summary>
				//***************************************************************************************************************
				AspNet,

				//***************************************************************************************************************
				/// <summary>Represents the 'dotnet' string.</summary>
				//***************************************************************************************************************
				DotNet,

				#endregion Names
			}

			#endregion Enumerations

			#region Properties

			//*******************************************************************************************************************
			/// <summary>Gets or sets the child XML elements.</summary>
			//*******************************************************************************************************************
			public Build[] ChildBuilds { get; private set; }

			//*******************************************************************************************************************
			/// <summary>Gets or sets the name of the runtime.</summary>
			//*******************************************************************************************************************
			public NameEnum Name { get; set; }

			//*******************************************************************************************************************
			/// <summary>Gets the index of this object in relation to the other child element of this object's parent.</summary>
			///
			/// <remarks>
			///   If the value is -1, then this object was not created from an XML node and the property has not been set.
			/// </remarks>
			//*******************************************************************************************************************
			public int Ordinal { get; set; }

			//*******************************************************************************************************************
			/// <summary>Gets or sets the major and minor version number of the runtime.</summary>
			//*******************************************************************************************************************
			public Version Version { get; set; }

			#endregion Properties

			#region Methods

			//*******************************************************************************************************************
			/// <overloads><summary>Instantiates a new <see cref="Runtime"/> object.</summary></overloads>
			///
			/// <summary>Instantiates a new <see cref="Runtime"/> object using the provided information.</summary>
			///
			/// <param name="name">'name' Custom Enumeration attribute contained in the XML element.</param>
			/// <param name="version">'version' Version attribute contained in the XML element.</param>
			/// <param name="childBuilds">Array of build elements which are child elements of this node. Can be empty.</param>
			///
			/// <exception cref="ArgumentNullException">
			///   <paramref name="version"/>, or <paramref name="childBuilds"/> is a null reference.
			/// </exception>
			//*******************************************************************************************************************
			public Runtime(NameEnum name, Version version, Build[] childBuilds)
			{
				if(version == null)
					throw new ArgumentNullException("version");
				if(childBuilds == null)
					throw new ArgumentNullException("childBuilds");
				Name = name;
				Version = version;
				ChildBuilds = childBuilds;
				Ordinal = -1;

				// Compute the maximum index used on any child items.
				int maxIndex = 0;
				foreach(Build item in ChildBuilds)
				{
					if(item.Ordinal >= maxIndex)
						maxIndex = item.Ordinal + 1; // Set to first index after this index.
				}

				// Assign ordinal for any child items that don't have it set (-1).
				foreach(Build item in ChildBuilds)
				{
					if(item.Ordinal == -1)
						item.Ordinal = maxIndex++;
				}
			}

			//*******************************************************************************************************************
			/// <summary>Instantiates a new <see cref="Runtime"/> empty object.</summary>
			///
			/// <param name="name">'name' Custom Enumeration attribute contained in the XML element.</param>
			/// <param name="version">'version' Version attribute contained in the XML element.</param>
			///
			/// <exception cref="ArgumentNullException"><paramref name="version"/> is a null reference.</exception>
			//*******************************************************************************************************************
			public Runtime(NameEnum name, Version version)
			{
				if(version == null)
					throw new ArgumentNullException("version");
				Name = name;
				Version = version;
				ChildBuilds = new Build[0];
				Ordinal = -1;
			}

			//*******************************************************************************************************************
			/// <summary>Instantiates a new <see cref="Runtime"/> object from an <see cref="XmlNode"/> object.</summary>
			///
			/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
			/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
			///
			/// <exception cref="ArgumentException">
			///   <paramref name="node"/> does not correspond to a runtime node or is not an 'Element' type node or <paramref
			///   name="ordinal"/> is negative.
			/// </exception>
			/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
			/// <exception cref="InvalidDataException">
			///   An error occurred while reading the data into the node, or one of it's child nodes.
			/// </exception>
			//*******************************************************************************************************************
			public Runtime(XmlNode node, int ordinal)
			{
				if(node == null)
					throw new ArgumentNullException("node");
				if(ordinal < 0)
					throw new ArgumentException("the ordinal specified is negative.");
				if(node.NodeType != XmlNodeType.Element)
					throw new ArgumentException("node is not of type 'Element'.");

				ParseXmlNode(node, ordinal);
			}

			//*******************************************************************************************************************
			/// <summary>Adds a <see cref="Build"/> to <see cref="ChildBuilds"/>.</summary>
			///
			/// <param name="item"><see cref="Build"/> to be added. If null, then no changes will occur. Can be null.</param>
			//*******************************************************************************************************************
			public void AddBuild(Build item)
			{
				if (item == null) return;

				// Compute the maximum index used on any child items.
				int maxIndex = 0;
				foreach(Build child in ChildBuilds)
				{
					if (child.Ordinal >= maxIndex)
						maxIndex = child.Ordinal + 1; // Set to first index after this index.
				}

				var list = new List<Build>(ChildBuilds);
				list.Add(item);
				item.Ordinal = maxIndex;
				ChildBuilds = list.ToArray();
			}

			//*******************************************************************************************************************
			/// <summary>Creates an XML element for this object using the provided <see cref="XmlDocument"/> object.</summary>
			///
			/// <param name="doc"><see cref="XmlDocument"/> object to generate the element from.</param>
			///
			/// <returns><see cref="XmlElement"/> object containing this classes data.</returns>
			///
			/// <exception cref="ArgumentNullException"><paramref name="doc"/> is a null reference.</exception>
			//*******************************************************************************************************************
			public XmlElement CreateElement(XmlDocument doc)
			{
				if(doc == null)
					throw new ArgumentNullException("doc");
				XmlElement returnElement = doc.CreateElement("runtime");

				string valueString;

				// name
				valueString = GetNameString();
				returnElement.SetAttribute("name", valueString);

				// version
				valueString = GetVersionString();
				returnElement.SetAttribute("version", valueString);
				// Build up dictionary of indexes and corresponding items.
				Dictionary<int, object> lookup = new Dictionary<int, object>();

				foreach(Build child in ChildBuilds)
				{
					if(lookup.ContainsKey(child.Ordinal))
						throw new InvalidOperationException("An attempt was made to generate the XML element with two child"
							+ " elements with the same ordinal.Ordinals must be unique across all child objects.");
					lookup.Add(child.Ordinal, child);
				}

				// Sort the keys.
				List<int> keys = lookup.Keys.ToList();
				keys.Sort();

				foreach (int key in keys)
				{
					if(lookup[key] is Build)
						returnElement.AppendChild(((Build)lookup[key]).CreateElement(doc));
				}
				return returnElement;
			}

			//*******************************************************************************************************************
			/// <summary>Gets a string representation of Name.</summary>
			///
			/// <returns>String representing the value.</returns>
			//*******************************************************************************************************************
			public string GetNameString()
			{

				switch(Name)
				{
					case NameEnum.AspNet:
						return "aspnet";
					case NameEnum.DotNet:
						return "dotnet";
					default:
						throw new NotImplementedException("The enumerated type was not recognized as a supported type.");
				}
			}

			//*******************************************************************************************************************
			/// <summary>Gets a string representation of Version.</summary>
			///
			/// <returns>String representing the value.</returns>
			//*******************************************************************************************************************
			public string GetVersionString()
			{
				return Version.ToString(2);
			}

			//*******************************************************************************************************************
			/// <summary>Parses an XML node and populates the data into this object.</summary>
			///
			/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
			/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
			///
			/// <exception cref="ArgumentException"><paramref name="node"/> does not correspond to a runtime node.</exception>
			/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
			/// <exception cref="InvalidDataException">
			///   An error occurred while reading the data into the node, or one of it's child nodes.
			/// </exception>
			//*******************************************************************************************************************
			public void ParseXmlNode(XmlNode node, int ordinal)
			{
				if(node == null)
					throw new ArgumentNullException("node");
				if(string.Compare(node.Name, "runtime", false) != 0)
					throw new ArgumentException("node does not correspond to a runtime node.");

				XmlAttribute attrib;

				// name
				attrib = node.Attributes["name"];
				if(attrib == null)
					throw new InvalidDataException("An XML string Attribute (name) is not optional, but was not found in the XML"
						+ " element (runtime).");
				SetNameFromString(attrib.Value);

				// version
				attrib = node.Attributes["version"];
				if(attrib == null)
					throw new InvalidDataException("An XML string Attribute (version) is not optional, but was not found in the"
						+ " XML element (runtime).");
				SetVersionFromString(attrib.Value);

				// Read the child objects.
				List<Build> childBuildsList = new List<Build>();
				int index = 0;
				foreach(XmlNode child in node.ChildNodes)
				{
					if(child.NodeType == XmlNodeType.Element && child.Name == "build")
						childBuildsList.Add(new Build(child, index++));
				}
				ChildBuilds = childBuildsList.ToArray();

				Ordinal = ordinal;
			}

			//*******************************************************************************************************************
			/// <summary>Removes a <see cref="Build"/> from <see cref="ChildBuilds"/>.</summary>
			///
			/// <param name="item"><see cref="Build"/> to be removed. Can be null.</param>
			//*******************************************************************************************************************
			public void RemoveBuild(Build item)
			{
				if (item == null) return;

				var list = new List<Build>(ChildBuilds);
				list.Remove(item);
				ChildBuilds = list.ToArray();
			}

			//*******************************************************************************************************************
			/// <summary>Parses a string value and stores the data in Name.</summary>
			///
			/// <param name="value">String representation of the value.</param>
			///
			/// <exception cref="InvalidDataException">
			///   <list type="bullet">
			///     <listheader>One of the following:</listheader>
			///     <item>The string value is a null reference or an empty string.</item>
			///     <item>The string value could not be parsed.</item>
			///   </list>
			/// </exception>
			//*******************************************************************************************************************
			public void SetNameFromString(string value)
			{
				if(value == null)
					throw new InvalidDataException("The string value for 'name' is a null reference.");
				if(value.Length == 0)
					throw new InvalidDataException("The string value for 'name' is an empty string.");
				if(string.Compare(value, "aspnet", false) == 0)
				{
					Name = NameEnum.AspNet;
					return;
				}
				if(string.Compare(value, "dotnet", false) == 0)
				{
					Name = NameEnum.DotNet;
					return;
				}
				throw new InvalidDataException(string.Format("The enum value specified ({0}) is not a recognized enumerated type"
					+ " for name.", value));
			}

			//*******************************************************************************************************************
			/// <summary>Parses a string value and stores the data in Version.</summary>
			///
			/// <param name="value">String representation of the value.</param>
			///
			/// <exception cref="InvalidDataException">
			///   <list type="bullet">
			///     <listheader>One of the following:</listheader>
			///     <item>The string value is a null reference or an empty string.</item>
			///     <item>The string value could not be parsed.</item>
			///   </list>
			/// </exception>
			//*******************************************************************************************************************
			public void SetVersionFromString(string value)
			{
				if(value == null)
					throw new InvalidDataException("The string value for 'version' is a null reference.");
				if(value.Length == 0)
					throw new InvalidDataException("The string value for 'version' is an empty string.");
				string[] splits = value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				if(splits.Length < 2)
					throw new InvalidDataException(string.Format("The Version value ({0}) for 'version' does not contain at"
						+ " least two components separated by a period (Ex: <major>.<minor>)", value));
				if(splits.Length > 2)
					throw new InvalidDataException(string.Format("The Version value ({0}) for 'version' has more than 2"
						+ " components separated by a period. This version is limited to two at most (Ex: <major>.<minor>)",
						value));

				try
				{
					int major = int.Parse(splits[0]);
					int minor = int.Parse(splits[1]);
					Version = new Version(major, minor);
					return;
				}
				catch(Exception e)
				{
					throw new InvalidDataException(string.Format("The Version value ({0}) for 'version' is not valid. See Inner"
						+ " Exception.", value), e);
				}
			}

			#endregion Methods
		}

		#endregion Classes

		#region Fields

		//***********************************************************************************************************************
		/// <summary>Default encoding of the XML file generated from this object.</summary>
		//***********************************************************************************************************************
		private const string mDefaultXMLEncoding = "UTF-8";

		//***********************************************************************************************************************
		/// <summary>Default version of the XML file generated from this object.</summary>
		//***********************************************************************************************************************
		private const string mDefaultXMLVersion = "1.0";

		#endregion Fields

		#region Properties

		//***********************************************************************************************************************
		/// <summary>Gets or sets the child XML elements.</summary>
		//***********************************************************************************************************************
		public Runtime[] ChildRuntimes { get; private set; }

		//***********************************************************************************************************************
		/// <summary>Gets the index of this object in relation to the other child element of this object's parent.</summary>
		///
		/// <remarks>
		///   If the value is -1, then this object was not created from an XML node and the property has not been set.
		/// </remarks>
		//***********************************************************************************************************************
		public int Ordinal { get; set; }

		//***********************************************************************************************************************
		/// <summary>Encoding of the XML file this root node will be contained in.</summary>
		///
		/// <remarks>Defaults to 'UTF-8'</remarks>
		//***********************************************************************************************************************
		public string XmlFileEncoding { get; set; }

		//***********************************************************************************************************************
		/// <summary>Version of the XML file this root node will be contained in.</summary>
		///
		/// <remarks>Defaults to '1.0'</remarks>
		//***********************************************************************************************************************
		public string XmlFileVersion { get; set; }

		#endregion Properties

		#region Methods

		//***********************************************************************************************************************
		/// <overloads><summary>Instantiates a new <see cref="Dotnet"/> object.</summary></overloads>
		///
		/// <summary>Instantiates a new <see cref="Dotnet"/> object using the provided information.</summary>
		///
		/// <param name="childRuntimes">Array of runtime elements which are child elements of this node. Can be empty.</param>
		///
		/// <exception cref="ArgumentNullException"><paramref name="childRuntimes"/> is a null reference.</exception>
		//***********************************************************************************************************************
		public Dotnet(Runtime[] childRuntimes)
		{
			if(childRuntimes == null)
				throw new ArgumentNullException("childRuntimes");
			ChildRuntimes = childRuntimes;
			Ordinal = -1;

			// Compute the maximum index used on any child items.
			int maxIndex = 0;
			foreach(Runtime item in ChildRuntimes)
			{
				if(item.Ordinal >= maxIndex)
					maxIndex = item.Ordinal + 1; // Set to first index after this index.
			}

			// Assign ordinal for any child items that don't have it set (-1).
			foreach(Runtime item in ChildRuntimes)
			{
				if(item.Ordinal == -1)
					item.Ordinal = maxIndex++;
			}
			XmlFileVersion = mDefaultXMLVersion;
			XmlFileEncoding = mDefaultXMLEncoding;
		}

		//***********************************************************************************************************************
		/// <summary>Instantiates a new <see cref="Dotnet"/> empty object.</summary>
		//***********************************************************************************************************************
		public Dotnet()
		{
			ChildRuntimes = new Runtime[0];
			Ordinal = -1;
			XmlFileVersion = mDefaultXMLVersion;
			XmlFileEncoding = mDefaultXMLEncoding;
		}

		//***********************************************************************************************************************
		/// <summary>Instantiates a new <see cref="Dotnet"/> object from an <see cref="XmlNode"/> object.</summary>
		///
		/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
		/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
		///
		/// <exception cref="ArgumentException">
		///   <paramref name="node"/> does not correspond to a dotnet node or is not an 'Element' type node or <paramref
		///   name="ordinal"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">
		///   An error occurred while reading the data into the node, or one of it's child nodes.
		/// </exception>
		//***********************************************************************************************************************
		public Dotnet(XmlNode node, int ordinal)
		{
			if(node == null)
				throw new ArgumentNullException("node");
			if(ordinal < 0)
				throw new ArgumentException("the ordinal specified is negative.");
			if(node.NodeType != XmlNodeType.Element)
				throw new ArgumentException("node is not of type 'Element'.");

			ParseXmlNode(node, ordinal);
			XmlFileVersion = mDefaultXMLVersion;
			XmlFileEncoding = mDefaultXMLEncoding;
		}

		//***********************************************************************************************************************
		/// <summary>Instantiates a new <see cref="Dotnet"/> object from an XML file.</summary>
		///
		/// <param name="filePath">Path to the XML file containing the data to be imported.</param>
		///
		/// <exception cref="ArgumentException">
		///   <list type="bullet">
		///     <listheader>One of the following:</listheader>
		///     <item><paramref name="filePath"/> is invalid or an error occurred while accessing it.</item>
		///     <item><paramref name="filePath"/> is an empty array.</item>
		///   </list>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">An error occurred while parsing the XML data.</exception>
		//***********************************************************************************************************************
		public Dotnet(string filePath)
		{
			if(filePath == null)
				throw new ArgumentNullException("filePath");
			if(filePath.Length == 0)
				throw new ArgumentException("filePath is empty");

			ImportFromXML(filePath);
		}

		//***********************************************************************************************************************
		/// <summary>Instantiates a new <see cref="Dotnet"/> object from an XML file.</summary>
		///
		/// <param name="stream">Stream containing the XML file data.</param>
		///
		/// <exception cref="ArgumentException"><paramref name="stream"/> did not contain valid XML.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="stream"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">An error occurred while parsing the XML data.</exception>
		//***********************************************************************************************************************
		public Dotnet(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");

			ImportFromXML(stream);
		}

		//***********************************************************************************************************************
		/// <summary>Instantiates a new <see cref="Dotnet"/> object from an XML file.</summary>
		///
		/// <param name="reader">TextReader object containing the XML file data.</param>
		///
		/// <exception cref="ArgumentException">
		///   A parsing error occurred while attempting to load the XML from <paramref name="reader"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">An error occurred while parsing the XML data.</exception>
		//***********************************************************************************************************************
		public Dotnet(TextReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");

			ImportFromXML(reader);
		}

		//***********************************************************************************************************************
		/// <summary>Instantiates a new <see cref="Dotnet"/> object from an XML file.</summary>
		///
		/// <param name="reader">XmlReader object containing the XML file data.</param>
		///
		/// <exception cref="ArgumentException">
		///   A parsing error occurred while attempting to load the XML from <paramref name="reader"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">An error occurred while parsing the XML data.</exception>
		//***********************************************************************************************************************
		public Dotnet(XmlReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");

			ImportFromXML(reader);
		}

		//***********************************************************************************************************************
		/// <summary>Adds a <see cref="Runtime"/> to <see cref="ChildRuntimes"/>.</summary>
		///
		/// <param name="item"><see cref="Runtime"/> to be added. If null, then no changes will occur. Can be null.</param>
		//***********************************************************************************************************************
		public void AddRuntime(Runtime item)
		{
			if (item == null) return;

			// Compute the maximum index used on any child items.
			int maxIndex = 0;
			foreach(Runtime child in ChildRuntimes)
			{
				if (child.Ordinal >= maxIndex)
					maxIndex = child.Ordinal + 1; // Set to first index after this index.
			}

			var list = new List<Runtime>(ChildRuntimes);
			list.Add(item);
			item.Ordinal = maxIndex;
			ChildRuntimes = list.ToArray();
		}

		//***********************************************************************************************************************
		/// <summary>Creates an XML element for this object using the provided <see cref="XmlDocument"/> object.</summary>
		///
		/// <param name="doc"><see cref="XmlDocument"/> object to generate the element from.</param>
		///
		/// <returns><see cref="XmlElement"/> object containing this classes data.</returns>
		///
		/// <exception cref="ArgumentNullException"><paramref name="doc"/> is a null reference.</exception>
		//***********************************************************************************************************************
		public XmlElement CreateElement(XmlDocument doc)
		{
			if(doc == null)
				throw new ArgumentNullException("doc");
			XmlElement returnElement = doc.CreateElement("dotnet");
			// Build up dictionary of indexes and corresponding items.
			Dictionary<int, object> lookup = new Dictionary<int, object>();

			foreach(Runtime child in ChildRuntimes)
			{
				if(lookup.ContainsKey(child.Ordinal))
					throw new InvalidOperationException("An attempt was made to generate the XML element with two child elements"
						+ " with the same ordinal.Ordinals must be unique across all child objects.");
				lookup.Add(child.Ordinal, child);
			}

			// Sort the keys.
			List<int> keys = lookup.Keys.ToList();
			keys.Sort();

			foreach (int key in keys)
			{
				if(lookup[key] is Runtime)
					returnElement.AppendChild(((Runtime)lookup[key]).CreateElement(doc));
			}
			return returnElement;
		}

		//***********************************************************************************************************************
		/// <summary>Exports data to an XML file.</summary>
		///
		/// <param name="stream">Stream to write the XML to.</param>
		///
		/// <exception cref="ArgumentNullException"><paramref name="stream"/> is a null reference.</exception>
		//***********************************************************************************************************************
		public void ExportToXML(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");
			XmlDocument doc = new XmlDocument();
			XmlDeclaration dec = doc.CreateXmlDeclaration(XmlFileVersion, XmlFileEncoding, null);
			doc.InsertBefore(dec, doc.DocumentElement);

			XmlElement root = CreateElement(doc);
			doc.AppendChild(root);
			doc.Save(stream);
		}

		//***********************************************************************************************************************
		/// <summary>Exports data to an XML file.</summary>
		///
		/// <param name="filePath">Path to the XML file to be written to. If file exists all contents will be destroyed.</param>
		///
		/// <exception cref="ArgumentException">
		///   <list type="bullet">
		///     <listheader>One of the following:</listheader>
		///     <item><paramref name="filePath"/> is invalid or an error occurred while accessing it.</item>
		///     <item><paramref name="filePath"/> is an empty array.</item>
		///   </list>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
		//***********************************************************************************************************************
		public void ExportToXML(string filePath)
		{
			if(filePath == null)
				throw new ArgumentNullException("filePath");
			if(filePath.Length == 0)
				throw new ArgumentException("filePath is empty");
			XmlDocument doc = new XmlDocument();
			XmlDeclaration dec = doc.CreateXmlDeclaration(XmlFileVersion, XmlFileEncoding, null);
			doc.InsertBefore(dec, doc.DocumentElement);

			XmlElement root = CreateElement(doc);
			doc.AppendChild(root);
			doc.Save(filePath);
		}

		//***********************************************************************************************************************
		/// <summary>Exports data to an XML file.</summary>
		///
		/// <param name="writer">TextWriter object to write the XML to.</param>
		///
		/// <exception cref="ArgumentNullException"><paramref name="writer"/> is a null reference.</exception>
		//***********************************************************************************************************************
		public void ExportToXML(TextWriter writer)
		{
			if(writer == null)
				throw new ArgumentNullException("writer");
			XmlDocument doc = new XmlDocument();
			XmlDeclaration dec = doc.CreateXmlDeclaration(XmlFileVersion, XmlFileEncoding, null);
			doc.InsertBefore(dec, doc.DocumentElement);

			XmlElement root = CreateElement(doc);
			doc.AppendChild(root);
			doc.Save(writer);
		}

		//***********************************************************************************************************************
		/// <summary>Exports data to an XML file.</summary>
		///
		/// <param name="writer">XmlWriter object to write the XML to.</param>
		///
		/// <exception cref="ArgumentNullException"><paramref name="writer"/> is a null reference.</exception>
		//***********************************************************************************************************************
		public void ExportToXML(XmlWriter writer)
		{
			if(writer == null)
				throw new ArgumentNullException("writer");
			XmlDocument doc = new XmlDocument();
			XmlDeclaration dec = doc.CreateXmlDeclaration(XmlFileVersion, XmlFileEncoding, null);
			doc.InsertBefore(dec, doc.DocumentElement);

			XmlElement root = CreateElement(doc);
			doc.AppendChild(root);
			doc.Save(writer);
		}

		//***********************************************************************************************************************
		/// <summary>Imports data from an XML stream.</summary>
		///
		/// <param name="stream">Stream containing the XML file data.</param>
		///
		/// <exception cref="ArgumentException"><paramref name="stream"/> did not contain valid XML.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="stream"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">
		///   The XML was valid, but an error occurred while extracting the data from it.
		/// </exception>
		//***********************************************************************************************************************
		public void ImportFromXML(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(stream);
			}
			catch(XmlException e)
			{
				throw new ArgumentException(string.Format("Unable to parse the XML from the stream. Error message: {0}.",
					e.Message), nameof(stream), e);
			}

			// Pull the version and encoding
			XmlDeclaration dec = doc.FirstChild as XmlDeclaration;
			if(dec != null)
			{
				XmlFileVersion = dec.Version;
				XmlFileEncoding = dec.Encoding;
			}
			else
			{
				XmlFileVersion = mDefaultXMLVersion;
				XmlFileEncoding = mDefaultXMLEncoding;
			}

			XmlElement root = doc.DocumentElement;
			if(root.NodeType != XmlNodeType.Element)
				throw new InvalidDataException("The root node is not an element node.");
			if(string.Compare(root.Name, "dotnet", false) != 0)
				throw new InvalidDataException(string.Format("The root element name is not the one expected (Actual: '{0}',"
					+ " Expected: 'dotnet').", root.Name));

			ParseXmlNode(root, 0);
		}

		//***********************************************************************************************************************
		/// <summary>Imports data from an XML file.</summary>
		///
		/// <param name="filePath">Path to the XML file containing the data to be imported.</param>
		///
		/// <exception cref="ArgumentException">
		///   <list type="bullet">
		///     <listheader>One of the following:</listheader>
		///     <item><paramref name="filePath"/> is invalid or an error occurred while accessing it.</item>
		///     <item><paramref name="filePath"/> is an empty array.</item>
		///   </list>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">
		///   The XML was valid, but an error occurred while extracting the data from it.
		/// </exception>
		//***********************************************************************************************************************
		public void ImportFromXML(string filePath)
		{
			if(filePath == null)
				throw new ArgumentNullException("filePath");
			if(filePath.Length == 0)
				throw new ArgumentException("filePath is empty");

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(filePath);
			}
			catch(PathTooLongException e)
			{
				throw new ArgumentException(string.Format("The file path specified ({0}) is not valid ({1}).", filePath,
					e.Message), nameof(filePath), e);
			}
			catch(DirectoryNotFoundException e)
			{
				throw new ArgumentException(string.Format("The file path specified ({0}) is not valid ({1}).", filePath,
					e.Message), nameof(filePath), e);
			}
			catch(NotSupportedException e)
			{
				throw new ArgumentException(string.Format("The file path specified ({0}) is not valid ({1}).", filePath,
					e.Message), nameof(filePath), e);
			}
			catch(FileNotFoundException e)
			{
				throw new ArgumentException(string.Format("The file could not be located at the path specified ({0}).",
					filePath), nameof(filePath), e);
			}
			catch(IOException e)
			{
				throw new ArgumentException(string.Format("An I/O error occurred ({0}) while opening the file specified ({1}).",
					e.Message, filePath), nameof(filePath), e);
			}
			catch(UnauthorizedAccessException e)
			{
				throw new ArgumentException(string.Format("Unable to access the file path specified ({0}).", filePath),
					nameof(filePath), e);
			}
			catch(SecurityException e)
			{
				throw new ArgumentException(string.Format("The caller doesn't have the required permissions to access the file"
					+ " path specified ({0}).", filePath), nameof(filePath), e);
			}
			catch(XmlException e)
			{
				throw new ArgumentException(string.Format("Unable to parse the XML from the file specified ({0}). Error message:"
					+ " {1}.", filePath, e.Message), nameof(filePath), e);
			}

			// Pull the version and encoding
			XmlDeclaration dec = doc.FirstChild as XmlDeclaration;
			if(dec != null)
			{
				XmlFileVersion = dec.Version;
				XmlFileEncoding = dec.Encoding;
			}
			else
			{
				XmlFileVersion = mDefaultXMLVersion;
				XmlFileEncoding = mDefaultXMLEncoding;
			}

			XmlElement root = doc.DocumentElement;
			if(root.NodeType != XmlNodeType.Element)
				throw new InvalidDataException("The root node is not an element node.");
			if(string.Compare(root.Name, "dotnet", false) != 0)
				throw new InvalidDataException(string.Format("The root element name is not the one expected (Actual: '{0}',"
					+ " Expected: 'dotnet').", root.Name));

			ParseXmlNode(root, 0);
		}

		//***********************************************************************************************************************
		/// <summary>Imports data from an XML text reader.</summary>
		///
		/// <param name="reader">TextReader object containing the XML file data.</param>
		///
		/// <exception cref="ArgumentException">
		///   A parsing error occurred while attempting to load the XML from <paramref name="reader"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">
		///   The XML was valid, but an error occurred while extracting the data from it.
		/// </exception>
		//***********************************************************************************************************************
		public void ImportFromXML(TextReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(reader);
			}
			catch(XmlException e)
			{
				throw new ArgumentException(string.Format("Unable to parse the XML from the reader. Error message: {0}.",
					e.Message), nameof(reader), e);
			}

			// Pull the version and encoding
			XmlDeclaration dec = doc.FirstChild as XmlDeclaration;
			if(dec != null)
			{
				XmlFileVersion = dec.Version;
				XmlFileEncoding = dec.Encoding;
			}
			else
			{
				XmlFileVersion = mDefaultXMLVersion;
				XmlFileEncoding = mDefaultXMLEncoding;
			}

			XmlElement root = doc.DocumentElement;
			if(root.NodeType != XmlNodeType.Element)
				throw new InvalidDataException("The root node is not an element node.");
			if(string.Compare(root.Name, "dotnet", false) != 0)
				throw new InvalidDataException(string.Format("The root element name is not the one expected (Actual: '{0}',"
					+ " Expected: 'dotnet').", root.Name));

			ParseXmlNode(root, 0);
		}

		//***********************************************************************************************************************
		/// <summary>Imports data from an XML reader.</summary>
		///
		/// <param name="reader">XmlReader object containing the XML file data.</param>
		///
		/// <exception cref="ArgumentException">
		///   A parsing error occurred while attempting to load the XML from <paramref name="reader"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">
		///   The XML was valid, but an error occurred while extracting the data from it.
		/// </exception>
		//***********************************************************************************************************************
		public void ImportFromXML(XmlReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(reader);
			}
			catch(XmlException e)
			{
				throw new ArgumentException(string.Format("Unable to parse the XML from the reader. Error message: {0}.",
					e.Message), nameof(reader), e);
			}

			// Pull the version and encoding
			XmlDeclaration dec = doc.FirstChild as XmlDeclaration;
			if(dec != null)
			{
				XmlFileVersion = dec.Version;
				XmlFileEncoding = dec.Encoding;
			}
			else
			{
				XmlFileVersion = mDefaultXMLVersion;
				XmlFileEncoding = mDefaultXMLEncoding;
			}

			XmlElement root = doc.DocumentElement;
			if(root.NodeType != XmlNodeType.Element)
				throw new InvalidDataException("The root node is not an element node.");
			if(string.Compare(root.Name, "dotnet", false) != 0)
				throw new InvalidDataException(string.Format("The root element name is not the one expected (Actual: '{0}',"
					+ " Expected: 'dotnet').", root.Name));

			ParseXmlNode(root, 0);
		}

		//***********************************************************************************************************************
		/// <summary>Parses an XML node and populates the data into this object.</summary>
		///
		/// <param name="node"><see cref="XmlNode"/> containing the data to extract.</param>
		/// <param name="ordinal">Index of the <see cref="XmlNode"/> in it's parent elements.</param>
		///
		/// <exception cref="ArgumentException"><paramref name="node"/> does not correspond to a dotnet node.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is a null reference.</exception>
		/// <exception cref="InvalidDataException">
		///   An error occurred while reading the data into the node, or one of it's child nodes.
		/// </exception>
		//***********************************************************************************************************************
		public void ParseXmlNode(XmlNode node, int ordinal)
		{
			if(node == null)
				throw new ArgumentNullException("node");
			if(string.Compare(node.Name, "dotnet", false) != 0)
				throw new ArgumentException("node does not correspond to a dotnet node.");

			// Read the child objects.
			List<Runtime> childRuntimesList = new List<Runtime>();
			int index = 0;
			foreach(XmlNode child in node.ChildNodes)
			{
				if(child.NodeType == XmlNodeType.Element && child.Name == "runtime")
					childRuntimesList.Add(new Runtime(child, index++));
			}
			ChildRuntimes = childRuntimesList.ToArray();

			Ordinal = ordinal;
		}

		//***********************************************************************************************************************
		/// <summary>Removes a <see cref="Runtime"/> from <see cref="ChildRuntimes"/>.</summary>
		///
		/// <param name="item"><see cref="Runtime"/> to be removed. Can be null.</param>
		//***********************************************************************************************************************
		public void RemoveRuntime(Runtime item)
		{
			if (item == null) return;

			var list = new List<Runtime>(ChildRuntimes);
			list.Remove(item);
			ChildRuntimes = list.ToArray();
		}

		#endregion Methods
	}
}
