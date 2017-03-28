using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// would like to use contracts and use static analysis to check for problems but I think .net core doesn't support it at the moment
//using System.Diagnostics.Contracts;

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

    internal void Start(IReadOnlyList<string> lines)
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

        MoveToFirstLine();

        while (CurrentLineExists)
        {
            int currentLineIndex = CurrentLineIndex;
            ParseCurrentLine();

            if (currentLineIndex == CurrentLineIndex)
                throw new MarkdownInternalException($"Internal parser error. Line '{CurrentLine}' would have caused an infinite loop.");
        }

        return Html;
    }

    void ParseCurrentLine()
    {
        markdownToHtmls.ToList().ForEach(m => ApplyMarkdownToHtml(m));

        // the paragraph parser is special, in that it applies if nothing else is able to work
        if (markdownToHtmls.Any(m => m.CanParseCurrentLine) == false && CurrentLineExists)
            paragraph.WriteParagraphTag();
    }

    void ApplyMarkdownToHtml(IMarkdownToHtmlTag markdownToHtml)
    {
        if (markdownToHtml.CanParseCurrentLine)
            markdownToHtml.WriteHtmlTag();
    }

    void Initialise(string markdown) => ioCoordinator.Start(markdown.Split('\n').ToList());

    void MoveToFirstLine() => ioCoordinator.MoveToFirstLine();
    int CurrentLineIndex => ioCoordinator.CurrentLineIndex;
    bool CurrentLineExists => ioCoordinator.CurrentLineExists;
    string CurrentLine => ioCoordinator.CurrentLine;

    string Html => ioCoordinator.Html;

    readonly MarkdownHtmlIoCoordinator ioCoordinator;
    readonly IReadOnlyList<IMarkdownToHtmlTag> markdownToHtmls;
    readonly MarkdownToHtmlParagraphTag paragraph;
}