using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// would like to use contracts and use static analysis to check for problems but I think .net core doesn't support it at the moment
//using System.Diagnostics.Contracts;

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

    protected static string ParseMidlineMarkdown(string markdown) => Markdown_IndicatorsReplacedWithHtmlEmTags(Markdown__IndicatorsReplacedWithHtmlStrongTags((markdown)));

    static string MarkdownIndicatorsReplacedWithHtmlTags(string markdown, string markdownIndicator, string htmlTag)
    {
        var pattern = markdownIndicator + "(.+)" + markdownIndicator;
        var replacement = "<" + htmlTag + ">$1</" + htmlTag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }

    static string Markdown__IndicatorsReplacedWithHtmlStrongTags(string markdown) => MarkdownIndicatorsReplacedWithHtmlTags(markdown, "__", "strong");

    static string Markdown_IndicatorsReplacedWithHtmlEmTags(string markdown) => MarkdownIndicatorsReplacedWithHtmlTags(markdown, "_", "em");
}

internal class MarkdownToHtmlHeaderTag : MarkdownToHtmlTagBase, IMarkdownToHtmlTag
{
    internal MarkdownToHtmlHeaderTag(MarkdownHtmlIoCoordinator ioCoordinator)
        : base(ioCoordinator)
    { }

    public bool CanParseCurrentLine => CurrentLineExists && CurrentLine.StartsWith("#");

    public void WriteHtmlTag()
    {
        if (HeadingLevel == 0)
            throw new Exception("ParseHeader called on a line that is not a header");

        if (HeadingLevel > 6)
            throw new Exception($"Invalid Markdown: h6 is the lowest level heading available, but trying to create h{HeadingLevel}");

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
        WriteTag("p", ParseMidlineMarkdown(CurrentLine));

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

    void ParseListItem() => WriteTag("li", ParseMidlineMarkdown(CurrentLineWithoutMarkdownListIndicator));

    string CurrentLineWithoutMarkdownListIndicator => CurrentLine.Substring(2);
}

public class Markdown
{
    public Markdown()
    {
        // I would probably use dependency injection to set these up in a bigger example
        ioCoordinator = new MarkdownHtmlIoCoordinator();
        header = new MarkdownToHtmlHeaderTag(ioCoordinator);
        unorderedList = new MarkdownToHtmlUnorderedListTag(ioCoordinator);
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
                throw new Exception($"Internal parser error. Line '{CurrentLine}' would have caused an infinite loop.");
        }

        return Html;
    }

    void ParseCurrentLine()
    {
        if (unorderedList.CanParseCurrentLine)
            unorderedList.WriteHtmlTag();
        else if (header.CanParseCurrentLine)
            header.WriteHtmlTag();
        else
            paragraph.WriteParagraphTag();
    }


    string Html => ioCoordinator.Html;

    void Initialise(string markdown) => ioCoordinator.Start(markdown.Split('\n').ToList());

    int CurrentLineIndex => ioCoordinator.CurrentLineIndex;
    void MoveToFirstLine() => ioCoordinator.MoveToFirstLine();
    bool CurrentLineExists => ioCoordinator.CurrentLineExists;
    string CurrentLine => ioCoordinator.CurrentLine;

    readonly MarkdownHtmlIoCoordinator ioCoordinator;
    readonly MarkdownToHtmlHeaderTag header;
    readonly MarkdownToHtmlUnorderedListTag unorderedList;
    readonly MarkdownToHtmlParagraphTag paragraph;
}