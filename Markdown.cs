using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

internal class MarkdownInputOutputCoordinator
{
    int lineIndex;
    IReadOnlyList<string> lines;

    internal int CurrentLineIndex => lineIndex;
    internal void NextLine() => lineIndex++;
    internal void FirstLine() => lineIndex = 0;
    internal bool CurrentLineExists => lineIndex < lines.Count();
    internal string CurrentLine => lines[lineIndex];

    readonly StringBuilder html;
    internal void WriteHtml(string html) => this.html.Append(html);
    internal void WriteTag(string tag, string innerText) => WriteHtml($"<{tag}>{innerText}</{tag}>");
    internal string Html => html.ToString();

    internal MarkdownInputOutputCoordinator()
    {
        html = new StringBuilder();
        lines = new List<string>();
    }

    internal void Start(IReadOnlyList<string> lines)
    {
        this.lines = lines;
        html.Clear();
    }
}

public class Markdown
{
    readonly MarkdownInputOutputCoordinator inputOutputCoordinator;

    int CurrentLineIndex => inputOutputCoordinator.CurrentLineIndex;
    void NextLine() => inputOutputCoordinator.NextLine();
    void FirstLine() => inputOutputCoordinator.FirstLine();
    bool CurrentLineExists => inputOutputCoordinator.CurrentLineExists;
    string CurrentLine => inputOutputCoordinator.CurrentLine;

    void WriteHtml(string html) => inputOutputCoordinator.WriteHtml(html);
    void WriteTag(string tag, string innerText) => inputOutputCoordinator.WriteTag(tag, innerText);

    public Markdown()
    {
        inputOutputCoordinator = new MarkdownInputOutputCoordinator();
    }

    static string ParseMidlineMarkdown(string markdown, string delimiter, string tag)
    {
        var pattern = delimiter + "(.+)" + delimiter;
        var replacement = "<" + tag + ">$1</" + tag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }

    static string ParseMidlineStrongMarkdown(string markdown) => ParseMidlineMarkdown(markdown, "__", "strong");

    static string ParseMidlineEmMarkdown(string markdown) => ParseMidlineMarkdown(markdown, "_", "em");

    static string ParseMidlineMarkdown(string markdown) => ParseMidlineEmMarkdown(ParseMidlineStrongMarkdown((markdown)));

    bool CurrentLineIsHeader => CurrentLineExists && CurrentLine.StartsWith("#");

    void ParseHeader()
    {
        int headingLevel = CurrentLine.TakeWhile(c => c == '#').Count();

        if (headingLevel == 0)
            throw new Exception("ParseHeader called on a line that is not a header");

        WriteTag($"h{headingLevel}", CurrentLine.Substring(headingLevel + 1));

        NextLine();
    }

    void ParseParagraph()
    {
        WriteTag("p", ParseMidlineMarkdown(CurrentLine));

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

    void ParseListItem() => WriteTag("li", ParseMidlineMarkdown(CurrentLineWithoutMarkdownListIndicator()));

    string CurrentLineWithoutMarkdownListIndicator() => CurrentLine.Substring(2);

    public string Parse(string markdown)
    {
        inputOutputCoordinator.Start(markdown.Split('\n').ToList());
 
        FirstLine();
        while (CurrentLineExists)
        {
            int currentLine = CurrentLineIndex;
            ParseLine();

            if (currentLine == CurrentLineIndex)
                throw new Exception($"Internal parser error. Line '{CurrentLine}' would have caused infinite loop.");
        }

        return inputOutputCoordinator.Html;
    }
    
}