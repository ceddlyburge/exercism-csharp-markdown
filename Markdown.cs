using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class Markdown
{
    int lineIndex;
    IReadOnlyList<string> lines;
    int CurrentLineIndex => lineIndex;
    void NextLine() => lineIndex++;
    void FirstLine() => lineIndex = 0;
    bool CurrentLineExists => lineIndex < lines.Count();
    string CurrentLine => lines[lineIndex];

    readonly StringBuilder html;
    void WriteHtml(string html) => this.html.Append(html);
    void WriteTag(string headerTag, string text) => WriteHtml($"<{headerTag}>{text}</{headerTag}>");

    public Markdown()
    {
        html = new StringBuilder();
    }

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

    bool CurrentLineIsHeader => CurrentLineExists && CurrentLine.StartsWith("#");

    void ParseHeader()
    {
        int headingLevel = CurrentLine.TakeWhile(c => c == '#').Count();

        if (headingLevel == 0)
            throw new Exception("ParseHeader called on a line that is not a header");

        WriteTag("h" + headingLevel, CurrentLine.Substring(headingLevel + 1));

        NextLine();
    }

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

    bool CurrentLineIsList => CurrentLineExists && CurrentLine.StartsWith("*");
    
    void ParseList()
    {
        WriteHtml("<ul>");

        do
        {
            ParseListItem();
            NextLine();
        }
        while (CurrentLineIsList);

        WriteHtml("</ul>");
    }

    void ParseListItem() => WriteHtml($"<li>{ParseText(CurrentLineWithoutMarkdownListIndicator())}</li>");

    string CurrentLineWithoutMarkdownListIndicator() => CurrentLine.Substring(2);

    public string Parse(string markdown)
    {
        lines = markdown.Split('\n').ToList();

        html.Clear();

        FirstLine();
        while (CurrentLineExists)
        {
            int currentLine = CurrentLineIndex;
            ParseLine();

            if (currentLine == CurrentLineIndex)
                throw new Exception($"Internal parser error. Line '{CurrentLine}' would have caused infinite loop.");
        }

        return html.ToString();
    }

    
}