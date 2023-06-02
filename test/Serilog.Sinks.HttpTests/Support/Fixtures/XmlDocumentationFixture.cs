using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Serilog.Support.Fixtures;

public class XmlDocumentationFixture
{
    private readonly XDocument document;
    private readonly Regex seeClassRegex;
    private readonly Regex seeEnumRegex;
    private readonly Regex seeHrefRegex;
    private readonly Regex seeAlsoRegex;
    private readonly Regex paramRefRegex;

    public XmlDocumentationFixture()
    {
        document = XDocument.Load("Serilog.Sinks.Http.xml");
        seeClassRegex = new Regex(@"<see cref=""T:(?<fullName>[\w.]+)""\s*/>");
        seeEnumRegex = new Regex(@"<see cref=""F:(?<fullName>[\w.]+)""\s*/>");
        seeHrefRegex = new Regex(@"<see href=""(?<url>[\w.:/]+)""\s*>(?<link>[\w\s.]+)</see>");
        seeAlsoRegex = new Regex(@"<seealso cref=""T:(?<fullName>[\w.]+)""\s*/>");
        paramRefRegex = new Regex(@"<paramref name=""(?<parameterName>\w+)""\s*/>");
    }

    public string GetDescription(string extensionName, string parameterName)
    {
        var member = document
            .Descendants("member")
            .Single(descendant => descendant.Attribute("name").Value.StartsWith($"M:Serilog.LoggerSinkConfigurationExtensions.{extensionName}"));

        var parameter = member
            .Descendants("param")
            .Single(descendant => descendant.Attribute("name").Value == parameterName);

        var description = GetValue(parameter)
            .Split("\n")
            .Select(RemoveSeeClassLinks)
            .Select(RemoveSeeEnumLinks)
            .Select(RemoveSeeHrefLinks)
            .Select(RemoveSeeAlsoLinks)
            .Select(RemoveParamRefLinks)
            .Select(RemovePara)
            .Select(RemoveQuotationMarks)
            .Select(row => row.Trim())
            .Where(row => row.Length > 0);

        return string.Join(" ", description);
    }

    private string RemoveSeeClassLinks(string description)
    {
        var matches = seeClassRegex.Matches(description);

        foreach (Match match in matches)
        {
            var type = ProbeType(match.Groups["fullName"].Value);

            description = description.Replace(
                match.Groups[0].Value,
                type.Name);
        }

        return description;
    }

    private string RemoveSeeEnumLinks(string description)
    {
        var matches = seeEnumRegex.Matches(description);

        foreach (Match match in matches)
        {
            var type = ProbeType(match.Groups["fullName"].Value);

            if (type != null)
            {
                description = description.Replace(
                    match.Groups[0].Value,
                    type.Name);
            }
            else
            {
                // If documentation is specifying an enum value instead of the enum itself, we need
                // to strip away the value
                var parts = match.Groups["fullName"].Value.Split('.');
                var fullName = string.Join('.', parts, 0, parts.Length - 1);
                type = ProbeType(fullName);

                description = description.Replace(
                    match.Groups[0].Value,
                    $"{type.Name}.{parts.Last()}");
            }
        }

        return description;
    }

    private string RemoveSeeHrefLinks(string description)
    {
        var matches = seeHrefRegex.Matches(description);

        foreach (Match match in matches)
        {
            description = description.Replace(
                match.Groups[0].Value,
                match.Groups["link"].Value);
        }

        return description;
    }

    private string RemoveSeeAlsoLinks(string description)
    {
        var matches = seeAlsoRegex.Matches(description);

        foreach (Match match in matches)
        {
            var type = ProbeType(match.Groups["fullName"].Value);

            description = description.Replace(
                match.Groups[0].Value,
                type.Name);
        }

        return description;
    }

    private string RemoveParamRefLinks(string description)
    {
        var matches = paramRefRegex.Matches(description);

        foreach (Match match in matches)
        {
            description = description.Replace(
                match.Groups[0].Value,
                match.Groups["parameterName"].Value);
        }

        return description;
    }

    private static string RemovePara(string description)
    {
        return description.Replace("<para />", string.Empty);
    }

    private static string RemoveQuotationMarks(string description)
    {
        return description.Replace("\"", string.Empty);
    }

    private static string GetValue(XNode node)
    {
        using var reader = node.CreateReader();
        reader.MoveToContent();

        return reader.ReadInnerXml();
    }

    private static Type ProbeType(string fullName)
    {
        Type ProbeTypeInAssembly(string assemblyName)
        {
            return Assembly.Load(assemblyName).GetType(fullName);
        }

        return ProbeTypeInAssembly("Serilog.Sinks.Http")
               ?? ProbeTypeInAssembly("Serilog")
               ?? ProbeTypeInAssembly("netstandard");
    }
}