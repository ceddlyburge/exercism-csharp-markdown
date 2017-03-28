using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// would like to use contracts and use static analysis to check for problems but I think .net core doesn't support it at the moment
//using System.Diagnostics.Contracts;


// there are a variety of interfaces and classes in this file, which should all be in their own files, but I don't think exercism supports this.
public class MarkdownInternalException : Exception
{
    public MarkdownInternalException(string message) : base(message)
    {
    }
}

internal interface IMarkdownToHtmlTag
{
    bool CanParseCurrentLine { get; }

    void WriteHtmlTag();
}

internal class MarkdownHtmlIoCoordinator
{
    int lineIndex;
    IReadOnlyList<string> lines;

    internal int CurrentLineIndex => lineIndex;
    internal void MoveToNextLine() => lineIndex++;
    internal void MoveToFirstLine() => lineIndex = 0;
    internal bool CurrentLineExists => lineIndex < lines.Count();
    internal string CurrentLine => lines[lineIndex];

    readonly StringBuilder html;
    internal void WriteHtml(string html) => this.html.Append(html);
    internal void WriteTag(string tag, string innerText) => WriteHtml($"<{tag}>{innerText}</{tag}>");
    internal string Html => html.ToString();

    internal MarkdownHtmlIoCoordinator()
    {
        html = new StringBuilder();
        lines = new List<string>();
    }

    internal void Initialise(IReadOnlyList<string> lines)
    {
        this.lines = lines;
        html.Clear();
    }
}

internal class MarkdownToHtmlTagBase
{
    readonly MarkdownHtmlIoCoordinator ioCoordinator;

    protected void MoveToNextLine() => ioCoordinator.MoveToNextLine();
    protected bool CurrentLineExists => ioCoordinator.CurrentLineExists;
    protected string CurrentLine => ioCoordinator.CurrentLine;

    protected void WriteHtml(string html) => ioCoordinator.WriteHtml(html);
    protected void WriteTag(string tag, string innerText) => ioCoordinator.WriteTag(tag, innerText);

    public MarkdownToHtmlTagBase(MarkdownHtmlIoCoordinator inputOutputCoordinator)
    {
        // see comment at top of file: Contract.Requires(inputOutputCoordinator != null);
        this.ioCoordinator = inputOutputCoordinator ?? throw new ArgumentNullException(nameof(inputOutputCoordinator));
    }

    protected static string MarkdownMidlineIndicatorsReplacedWithHtmlTags(string markdown) => Markdown_IndicatorsReplacedWithHtmlEmTags(Markdown__IndicatorsReplacedWithHtmlStrongTags((markdown)));

    static string Markdown__IndicatorsReplacedWithHtmlStrongTags(string markdown) => SingleMarkdownIndicatorsReplacedWithHtmlTags(markdown, "__", "strong");

    static string Markdown_IndicatorsReplacedWithHtmlEmTags(string markdown) => SingleMarkdownIndicatorsReplacedWithHtmlTags(markdown, "_", "em");

    // this name isn't the most amazing, but couldn't think of anything better
    static string SingleMarkdownIndicatorsReplacedWithHtmlTags(string markdown, string markdownIndicator, string htmlTag)
    {
        var pattern = markdownIndicator + "(.+)" + markdownIndicator;
        var replacement = "<" + htmlTag + ">$1</" + htmlTag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }
}

internal class MarkdownToHtmlHeaderTag : MarkdownToHtmlTagBase, IMarkdownToHtmlTag
{
    internal MarkdownToHtmlHeaderTag(MarkdownHtmlIoCoordinator ioCoordinator)
        : base(ioCoordinator)
    { }

    public bool CanParseCurrentLine => CurrentLineExists && HeadingLevel > 0 && HeadingLevel <= 6;

    public void WriteHtmlTag()
    {
        WriteTag($"h{HeadingLevel}", CurrentLine.Substring(HeadingLevel + 1));

        MoveToNextLine();
    }

    int HeadingLevel => CurrentLine.TakeWhile(c => c == '#').Count();
}

internal class MarkdownToHtmlParagraphTag : MarkdownToHtmlTagBase
{
    internal MarkdownToHtmlParagraphTag(MarkdownHtmlIoCoordinator ioCoordinator)
        : base(ioCoordinator)
    { }

    internal void WriteParagraphTag()
    {
        WriteTag("p", MarkdownMidlineIndicatorsReplacedWithHtmlTags(CurrentLine));

        MoveToNextLine();
    }
}

internal class MarkdownToHtmlUnorderedListTag : MarkdownToHtmlTagBase, IMarkdownToHtmlTag
{
    internal MarkdownToHtmlUnorderedListTag(MarkdownHtmlIoCoordinator ioCoordinator)
        : base(ioCoordinator)
    { }

    public bool CanParseCurrentLine => CurrentLineExists && CurrentLine.StartsWith("*");

    public void WriteHtmlTag()
    {
        WriteHtml("<ul>");

        do
        {
            ParseListItem();
            MoveToNextLine();
        }
        while (CanParseCurrentLine);

        WriteHtml("</ul>");
    }

    void ParseListItem() => WriteTag("li", MarkdownMidlineIndicatorsReplacedWithHtmlTags(CurrentLineWithoutMarkdownListIndicator));

    string CurrentLineWithoutMarkdownListIndicator => CurrentLine.Substring(2);
}

public class Markdown
{
    public Markdown()
    {
        // Take these via constructor injection later
        ioCoordinator = new MarkdownHtmlIoCoordinator();

        markdownToHtmls = new List<IMarkdownToHtmlTag>
        {
            new MarkdownToHtmlHeaderTag(ioCoordinator)
            , new MarkdownToHtmlUnorderedListTag(ioCoordinator)
        };

        paragraph = new MarkdownToHtmlParagraphTag(ioCoordinator);
    }

    public string ParsedHtml(string markdown)
    {
        Initialise(markdown);

        while (CurrentLineExists)
        {
            int currentLineIndex = CurrentLineIndex;

            ParseAtCurrentLine();

            if (currentLineIndex == CurrentLineIndex)
                throw new MarkdownInternalException($"Internal parser error. Line '{CurrentLine}' would have caused an infinite loop.");
        }

        return Html;
    }

    void Initialise(string markdown)
    {
        ioCoordinator.Initialise(markdown.Split('\n').ToList());

        MoveToFirstLine();
    }

    void ParseAtCurrentLine()
    {
        // the paragraph parser is special, in that it applies if nothing else is able to work
        if (CurrentLineExists && markdownToHtmls.Any(m => m.CanParseCurrentLine) == false)
            paragraph.WriteParagraphTag();

        // this could apply more than one markdownToHtml, as all the parsers are called and they update CurrentLine as they go.
        markdownToHtmls.ToList().ForEach(m => ApplyMarkdownToHtml(m));
    }

    void ApplyMarkdownToHtml(IMarkdownToHtmlTag markdownToHtml)
    {
        if (markdownToHtml.CanParseCurrentLine)
            markdownToHtml.WriteHtmlTag();
    }

    void MoveToFirstLine() => ioCoordinator.MoveToFirstLine();
    int CurrentLineIndex => ioCoordinator.CurrentLineIndex;
    bool CurrentLineExists => ioCoordinator.CurrentLineExists;
    string CurrentLine => ioCoordinator.CurrentLine;

    string Html => ioCoordinator.Html;

    readonly MarkdownHtmlIoCoordinator ioCoordinator;
    readonly IReadOnlyList<IMarkdownToHtmlTag> markdownToHtmls;
    readonly MarkdownToHtmlParagraphTag paragraph;
}