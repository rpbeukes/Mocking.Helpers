using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Mocking.Helpers
{
    public static class PackagesHelper
    {
        public static IEnumerable<string> GetProjectNugetPackages(Project project)
        {
            var csproj = new XmlDocument();
            csproj.Load(project.FilePath);
           
            var nugetPackages = csproj.SelectNodes("//PackageReference[@Include and @Version]")
                                      .OfType<XmlNode>()
                                      .Select(x => x.Attributes["Include"].Value)
                                      .ToList();

            return nugetPackages;
        }
    }
}
