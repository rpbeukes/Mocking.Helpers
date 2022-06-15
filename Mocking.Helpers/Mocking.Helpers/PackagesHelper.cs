using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Mocking.Helpers
{
    public static class PackagesHelper
    {
        public static IEnumerable<string> GetProjectNugetPackages(Project project)
        {
            var nugetPackages = new List<string>();
            var csproj = new XmlDocument();
            csproj.Load(project.FilePath);
            var nodes = csproj.SelectNodes("//PackageReference[@Include and @Version]");
            foreach (XmlNode packageReference in nodes)
            {
                var packageName = packageReference.Attributes["Include"].Value;
                nugetPackages.Add(packageName);
            }

            return nugetPackages;
        }
    }
}
