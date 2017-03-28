﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// would like to use contracts and use static analysis to check for problems but I think .net core doesn't support it at the moment
//using System.Diagnostics.Contracts;

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

internal class MarkdownToHtmlTagBase
{
    readonly MarkdownInputOutputCoordinator inputOutputCoordinator;

    protected int CurrentLineIndex => inputOutputCoordinator.CurrentLineIndex;
    protected void NextLine() => inputOutputCoordinator.NextLine();
    protected void FirstLine() => inputOutputCoordinator.FirstLine();
    protected bool CurrentLineExists => inputOutputCoordinator.CurrentLineExists;
    protected string CurrentLine => inputOutputCoordinator.CurrentLine;

    protected void WriteHtml(string html) => inputOutputCoordinator.WriteHtml(html);
    protected void WriteTag(string tag, string innerText) => inputOutputCoordinator.WriteTag(tag, innerText);

    public MarkdownToHtmlTagBase(MarkdownInputOutputCoordinator inputOutputCoordinator)
    {
        // see comment at top of file.Contract.Requires(inputOutputCoordinator != null);
        this.inputOutputCoordinator = inputOutputCoordinator ?? throw new ArgumentNullException(nameof(inputOutputCoordinator));
    }

    protected static string ParseMidlineMarkdown(string markdown) => ParseMidlineEmMarkdown(ParseMidlineStrongMarkdown((markdown)));

    static string ParseMidlineMarkdown(string markdown, string delimiter, string tag)
    {
        var pattern = delimiter + "(.+)" + delimiter;
        var replacement = "<" + tag + ">$1</" + tag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }

    static string ParseMidlineStrongMarkdown(string markdown) => ParseMidlineMarkdown(markdown, "__", "strong");

    static string ParseMidlineEmMarkdown(string markdown) => ParseMidlineMarkdown(markdown, "_", "em");
}

internal class MarkdownToHtmlHeaderTag : MarkdownToHtmlTagBase
{
    public MarkdownToHtmlHeaderTag(MarkdownInputOutputCoordinator inputOutputCoordinator)
        : base(inputOutputCoordinator)
    { }

    internal bool CanParseCurrentLine => CurrentLineExists && CurrentLine.StartsWith("#");

    internal void WriteHtmlTag()
    {
        int headingLevel = CurrentLine.TakeWhile(c => c == '#').Count();

        if (headingLevel == 0)
            throw new Exception("ParseHeader called on a line that is not a header");

        WriteTag($"h{headingLevel}", CurrentLine.Substring(headingLevel + 1));

        NextLine();
    }
}

internal class MarkdownToHtmlParagraphTag : MarkdownToHtmlTagBase
{
    public MarkdownToHtmlParagraphTag(MarkdownInputOutputCoordinator inputOutputCoordinator)
        : base(inputOutputCoordinator)
    { }

    internal void WriteParagraphTag()
    {
        WriteTag("p", ParseMidlineMarkdown(CurrentLine));

        NextLine();
    }
}

internal class MarkdownToHtmlUnorderedListTag : MarkdownToHtmlTagBase
{
    public MarkdownToHtmlUnorderedListTag(MarkdownInputOutputCoordinator inputOutputCoordinator)
        : base(inputOutputCoordinator)
    { }

    internal bool CanParseCurrentLine => CurrentLineExists && CurrentLine.StartsWith("*");

    internal void WriteHtmlTag()
    {
        WriteHtml("<ul>");

        do
        {
            ParseListItem();
            NextLine();
        }
        while (CanParseCurrentLine);

        WriteHtml("</ul>");
    }

    void ParseListItem() => WriteTag("li", ParseMidlineMarkdown(CurrentLineWithoutMarkdownListIndicator()));

    string CurrentLineWithoutMarkdownListIndicator() => CurrentLine.Substring(2);
}

public class Markdown
{
    readonly MarkdownInputOutputCoordinator inputOutputCoordinator;
    readonly MarkdownToHtmlHeaderTag header;
    readonly MarkdownToHtmlUnorderedListTag unorderedList;
    readonly MarkdownToHtmlParagraphTag paragraph;

    int CurrentLineIndex => inputOutputCoordinator.CurrentLineIndex;
    void NextLine() => inputOutputCoordinator.NextLine();
    void FirstLine() => inputOutputCoordinator.FirstLine();
    bool CurrentLineExists => inputOutputCoordinator.CurrentLineExists;
    string CurrentLine => inputOutputCoordinator.CurrentLine;

    void WriteHtml(string html) => inputOutputCoordinator.WriteHtml(html);
    void WriteTag(string tag, string innerText) => inputOutputCoordinator.WriteTag(tag, innerText);

    public Markdown()
    {
        // I would probably use dependency injection to set these up in a bigger example
        inputOutputCoordinator = new MarkdownInputOutputCoordinator();
        header = new MarkdownToHtmlHeaderTag(inputOutputCoordinator);
        unorderedList = new MarkdownToHtmlUnorderedListTag(inputOutputCoordinator);
        paragraph = new MarkdownToHtmlParagraphTag(inputOutputCoordinator);
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

    void ParseCurrentLine()
    {
        if (unorderedList.CanParseCurrentLine)
            unorderedList.WriteHtmlTag();
        else if (header.CanParseCurrentLine)
            header.WriteHtmlTag();
        else
            paragraph.WriteParagraphTag();
    }


    public string Parse(string markdown)
    {
        Start(markdown);

        FirstLine();
        while (CurrentLineExists)
        {
            int currentLine = CurrentLineIndex;
            ParseCurrentLine();

            if (currentLine == CurrentLineIndex)
                throw new Exception($"Internal parser error. Line '{CurrentLine}' would have caused infinite loop.");
        }

        return Html;
    }

    string Html => inputOutputCoordinator.Html;

    void Start(string markdown) => inputOutputCoordinator.Start(markdown.Split('\n').ToList());
}