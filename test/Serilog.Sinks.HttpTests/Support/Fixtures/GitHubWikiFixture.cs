using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Serilog.Support.Fixtures;

public class GitHubWikiFixture
{
    private const string WikiUrl = "https://raw.githubusercontent.com/wiki/FantasticFiasco/serilog-sinks-http/{0}";
                                                              
    private static readonly Regex LinkRegex = new Regex(@"\[(?<link>[\w\s.]+)\]\((?<url>[\w.:/]+)\)");

    private string pageContent;
        
    public async Task LoadAsync(string wikiPage)
    {
        using var httpClient = new HttpClient();
        pageContent = await httpClient.GetStringAsync(string.Format(WikiUrl, wikiPage));
    }

    public string GetDescription(string argumentName)
    {
        var arguments = GetArguments(pageContent);
        var argument = FindArgument(arguments, argumentName);
        var text = GetText(argument);

        text = RemoveArgumentName(text, argumentName);
        text = RemoveCodeIndicator(text);
        text = RemoveLinks(text);

        return text;
    }

    private static ListBlock GetArguments(string pageContent)
    {
        return Markdown.Parse(pageContent).LastChild as ListBlock ??
               throw new Exception("GitHub wiki page does not list arguments at the bottom of the page.");
    }

    private static ListItemBlock FindArgument(ListBlock arguments, string argumentName)
    {
        foreach (var block in arguments)
        {
            if (block is not ListItemBlock listItem)
            {
                continue;
            }

            if (listItem.Count == 0 || listItem.First() is not ParagraphBlock listItemParagraph)
            {
                continue;
            }

            if (listItemParagraph.Inline.Count() < 1 || listItemParagraph.Inline.ElementAt(0) is not CodeInline argumentNameInline)
            {
                continue;
            }

            if (argumentNameInline.Content != argumentName)
            {
                continue;
            }

            return listItem;
        }

        throw new Exception($"GitHub wiki does not contain argument with name \"{argumentName}\"");
    }

    private static string GetText(ListItemBlock listItemBlock)
    {
        var text = new StringBuilder();

        foreach (var block in listItemBlock.Cast<ParagraphBlock>())
        {
            foreach (var part in block.Inline)
            {
                if (part is CodeInline codeInline)
                {
                    text.Append(codeInline.Content);
                }
                else if (part is LiteralInline literalInline)
                {
                    text.Append(literalInline.Content);
                }
                else if (part is LinkInline linkInline)
                {
                    if (linkInline.Count() < 1 || linkInline.ElementAt(0) is not LiteralInline linkText)
                    {
                        throw new Exception("GitHub wiki has unexpected link format.");
                    }

                    text.Append(linkText.Content);
                }
                else
                {
                    throw new Exception($"GitHub wiki argument description contains an inline type we haven't implemented support for: \"{part.GetType()}\"");
                }
            }

            text.Append(" ");
        }

        return text.ToString().Trim();
    }

    private static string RemoveArgumentName(string text, string argumentName)
    {
        var textToRemove = $"{argumentName} - ";

        if (!text.StartsWith(textToRemove))
        {
            throw new Exception("GitHub wiki does not describe argument according to expected format.");
        }

        return text.Substring(textToRemove.Length);
    }

    private static string RemoveCodeIndicator(string description)
    {
        return description.Replace("`", string.Empty);
    }

    private static string RemoveLinks(string description)
    {
        var matches = LinkRegex.Matches(description);

        foreach (Match match in matches)
        {
            description = description.Replace(
                match.Groups[0].Value,
                match.Groups["link"].Value);
        }

        return description;
    }
}
