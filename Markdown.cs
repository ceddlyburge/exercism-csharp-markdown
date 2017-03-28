using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class Markdown
{
    int lineIndex;
    IReadOnlyList<string> lines;
    readonly StringBuilder html;
    string notNull = "notnull";

    public Markdown()
    {
        html = new StringBuilder();
    }

    void WriteHtml(string html) => this.html.Append(html); 

    static string Wrap(string text, string tag) => "<" + tag + ">" + text + "</" + tag + ">";

    static bool IsTag(string text, string tag) => text.StartsWith("<" + tag + ">");

    static string Parse(string markdown, string delimiter, string tag)
    {
        var pattern = delimiter + "(.+)" + delimiter;
        var replacement = "<" + tag + ">$1</" + tag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }

    static string Parse__(string markdown) => Parse(markdown, "__", "strong");

    static string Parse_(string markdown) => Parse(markdown, "_", "em");

    static string ParseText(string markdown)
    {
        var parsedText = Parse_(Parse__((markdown)));

        return parsedText;
    }

    string ParseHeader()
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
        WriteHtml(headerHtml);
        return notNull;
    }

    bool CurrentLineExists => lineIndex < lines.Count();

    string CurrentLine => lines[lineIndex];

    bool CurrentLineIsList => CurrentLineExists && CurrentLine.StartsWith("*");

    string ParseParagraph()
    {
        WriteHtml($"<p>{ParseText(CurrentLine)}</p>");

        NextLine();

        return notNull;
    }

    void ParseLine()
    {
        if (CurrentLineIsList)
        {
            WriteHtml(ParseList());
            return;
        }

        var result = ParseHeader();

        if (result == null)
        {
            result = ParseParagraph();
        }

        if (result == null)
        {
            NextLine();
            throw new ArgumentException("Invalid markdown");
        }
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

    string ListItem()
    {
        return Wrap(ParseText(CurrentLine.Substring(2)), "li");
    }

    public string Parse(string markdown)
    {
        lines = markdown.Split('\n').ToList();

        html.Clear();

        lineIndex = 0;
        while (lineIndex < lines.Count)
        {
            ParseLine();
        }

        return html.ToString();
    }
}