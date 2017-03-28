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

    void ParseHeader()
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
            throw new Exception("ParseHeader called on a line that is not a header");
        }

        var headerTag = "h" + count;
        var headerHtml = Wrap(markdown.Substring(count + 1), headerTag);

        WriteHtml(headerHtml);

        NextLine();
    }

    bool CurrentLineExists => lineIndex < lines.Count();

    string CurrentLine => lines[lineIndex];

    bool CurrentLineIsList => CurrentLineExists && CurrentLine.StartsWith("*");
    bool CurrentLineIsHeader => CurrentLineExists && CurrentLine.StartsWith("#");

    void ParseParagraph()
    {
        WriteHtml($"<p>{ParseText(CurrentLine)}</p>");

        NextLine();
    }

    void ParseLine()
    {
        if (CurrentLineIsList)
            ParseList();
        else if (CurrentLineIsHeader)
            ParseHeader();
        else 
            ParseParagraph();
    }

    void ParseList()
    {
        WriteHtml("<ul>");

        do
        {
            WriteHtml(ListItem());
            NextLine();
        } while (CurrentLineIsList);

        WriteHtml("</ul>");
    }

    void NextLine() => lineIndex++;
    void FirstLine() => lineIndex = 0;

    string ListItem()
    {
        return Wrap(ParseText(CurrentLine.Substring(2)), "li");
    }

    public string Parse(string markdown)
    {
        lines = markdown.Split('\n').ToList();

        html.Clear();

        FirstLine();
        while (CurrentLineExists)
            ParseLine();

        return html.ToString();
    }

}