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

    private static string ParseText(string markdown, bool list)
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

    private string ParseHeader(bool list, out bool inListAfter)
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
            inListAfter = list;
            return null;
        }

        var headerTag = "h" + count;
        var headerHtml = Wrap(markdown.Substring(count + 1), headerTag);

        if (list)
        {
            inListAfter = false;
            lineIndex++;
            return "</ul>" + headerHtml;
        }
        else
        {
            inListAfter = false;
            lineIndex++;
            return headerHtml;
        }
    }

    bool CurrentLineExists => lineIndex < lines.Count();

    string CurrentLine => lines[lineIndex];

    bool CurrentLineIsList => CurrentLineExists && CurrentLine.StartsWith("*");


    private string ParseLineItem(bool list, out bool inListAfter)
    {
        string markdown = CurrentLine;

        if (markdown.StartsWith("*"))
        {
            var innerHtml = Wrap(ParseText(markdown.Substring(2), true), "li");

            if (list)
            {
                inListAfter = true;
                lineIndex++;
                return innerHtml;
            }
            else
            {
                inListAfter = true;
                lineIndex++;
                return "<ul>" + innerHtml;
            }
        }

        inListAfter = list;
        return null;
    }

    private string ParseParagraph(bool list, out bool inListAfter)
    {
        string markdown = CurrentLine;

        if (!list)
        {
            inListAfter = false;
            lineIndex++;
            return ParseText(markdown, list);
        }
        else
        {
            inListAfter = false;
            lineIndex++;
            return "</ul>" + ParseText(markdown, list);
        }
    }

    private string ParseLine(bool list)
    {
        if (CurrentLineIsList)
            return ParseList();

        bool inListAfter;
        var result = ParseHeader(list, out inListAfter);

        if (result == null)
        {
            result = ParseLineItem(list, out inListAfter);
        }

        if (result == null)
        {
            result = ParseParagraph(list, out inListAfter);
        }

        if (result == null)
        {
            lineIndex++;
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
        var list = false;

        lineIndex = 0;
        while (lineIndex < lines.Count)
        {
            var lineResult = ParseLine(list);
            result += lineResult;
        }

        if (list)
        {
            return result + "</ul>";
        }
        else
        {
            return result;
        }
    }
}