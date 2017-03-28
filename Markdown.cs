using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Markdown
{
    private int lineIndex;
    private IReadOnlyList<string> lines;

    private static string Wrap(string text, string tag) => "<" + tag + ">" + text + "</" + tag + ">";

    private static bool IsTag(string text, string tag) => text.StartsWith("<" + tag + ">");

    private static string Parse(string markdown, string delimiter, string tag)
    {
        var pattern = delimiter + "(.+)" + delimiter;
        var replacement = "<" + tag + ">$1</" + tag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }

    private static string Parse__(string markdown) => Parse(markdown, "__", "strong");

    private static string Parse_(string markdown) => Parse(markdown, "_", "em");

    private static string ParseText(string markdown, bool list = false)
    {
        var parsedText = Parse_(Parse__((markdown)));

        if (list)
        {
            return parsedText;
        }
        else
        {
            return Wrap(parsedText, "p");
        }
    }

    private string ParseHeader()
    {
        string markdown = CurrentLine;

        var count = 0;

        for (int i = 0; i < markdown.Length; i++)
        {
            if (markdown[i] == '#')
            {
                count += 1;
            }
            else
            {
                break;
            }
        }

        if (count == 0)
        {
            return null;
        }

        var headerTag = "h" + count;
        var headerHtml = Wrap(markdown.Substring(count + 1), headerTag);

        NextLine();
        return headerHtml;
    }

    bool CurrentLineExists => lineIndex < lines.Count();

    string CurrentLine => lines[lineIndex];

    bool CurrentLineIsList => CurrentLineExists && CurrentLine.StartsWith("*");


    private string ParseLineItem()
    {
        return null;
    }

    private string ParseParagraph()
    {
        string markdown = CurrentLine;

        NextLine();
        return ParseText(markdown);
    }

    private string ParseLine()
    {
        if (CurrentLineIsList)
            return ParseList();

        var result = ParseHeader();

        if (result == null)
        {
            result = ParseLineItem();
        }

        if (result == null)
        {
            result = ParseParagraph();
        }

        if (result == null)
        {
            NextLine();
            throw new ArgumentException("Invalid markdown");
        }

        return result;
    }

    string ParseList()
    {
        string html = "<ul>";

        do
        {
            html += ListItem();
            NextLine();
        } while (CurrentLineIsList);

        html += "</ul>";

        return html;
    }

    void NextLine() => lineIndex++;

    private string ListItem()
    {
        return Wrap(ParseText(CurrentLine.Substring(2), true), "li");
    }

    public string Parse(string markdown)
    {
        lines = markdown.Split('\n').ToList();
        var result = "";

        lineIndex = 0;
        while (lineIndex < lines.Count)
        {
            var lineResult = ParseLine();
            result += lineResult;
        }

        return result;
    }
}