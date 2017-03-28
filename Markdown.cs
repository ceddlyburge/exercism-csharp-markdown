using System.Collections.Generic;
using System.Linq;

namespace MarkdownToHtml
{
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
}