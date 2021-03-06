﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Onion.SolutionTransform.Parser;

namespace Onion.SolutionTransform.Project
{
    public class ProjectDocument
    {
        private TransformableProject _project;
        private readonly string _path;
        private readonly XDocument _doc;

        public static readonly XNamespace XmlNs = "http://schemas.microsoft.com/developer/msbuild/2003";

        public ProjectDocument(TransformableProject proj, IParserInfo info)
        {
            _project = proj;
            _path = GetDocumentPath(proj, info);
            _doc = XDocument.Load(_path);
        }

        public void Write()
        {
            File.WriteAllText(_path, ToString());
        }

        public override string ToString()
        {
            return _doc.ToString();
        }

        public XElement Project
        {
            get { return _doc.Element(XmlNs + "Project"); }
        }

        public string AssemblyName
        {
            get { return Project.Element(XmlNs + "PropertyGroup").Element(XmlNs + "AssemblyName").Value; }
            set { Project.Element(XmlNs + "PropertyGroup").Element(XmlNs + "AssemblyName").SetValue(value); }
        }

        public string RootNamespace
        {
            get { return Project.Element(XmlNs + "PropertyGroup").Element(XmlNs + "RootNamespace").Value; }
            set { Project.Element(XmlNs + "PropertyGroup").Element(XmlNs + "RootNamespace").SetValue(value); }
        }

        public IEnumerable<ProjectReference> ProjectReferences
        {
            get
            {
                var group = Project.Elements(XmlNs + "ItemGroup").Where(e => e.Elements(XmlNs + "ProjectReference").Any());
                var xElements = @group as XElement[] ?? @group.ToArray();
                if (! xElements.Any()) yield break;
                var refs = xElements.Elements(XmlNs + "ProjectReference");
                foreach (var xElement in refs)
                {
                   yield return new ProjectReference(xElement);
                }
            }
        }
 
        public static ProjectDocument Load(TransformableProject proj, IParserInfo info)
        {
            var path = GetDocumentPath(proj, info);
            return !File.Exists(path) ? null : new ProjectDocument(proj, info);
        }

        private static string GetDocumentPath(TransformableProject proj, IParserInfo info)
        {
            return info.BasePath + Path.DirectorySeparatorChar + proj.Path;
        }
    }
}
