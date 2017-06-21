using System;
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

            infiniteRecursionChecker = new InfiniteRecursionChecker();

            paragraph = new MarkdownToHtmlParagraphTag(ioCoordinator);
        }

        public string ParsedHtml(string markdown)
        {
            Initialise(markdown);

            while (CurrentLineExists)
            {
                SaveCurrentLineIndex();
                ParseAtCurrentLine();
                CheckCurrentLineIndexForInfiniteRecursion();
            }

            return Html;
        }

        void CheckCurrentLineIndexForInfiniteRecursion() => infiniteRecursionChecker.CheckCurrentLineIndexForInfiniteRecursion(CurrentLineIndex, CurrentLine);

        void SaveCurrentLineIndex() => infiniteRecursionChecker.SaveCurrentLineIndex(CurrentLineIndex);

        void Initialise(string markdown)
        {
            InitialiseIOCoordinator(markdown);

            MoveToFirstLine();
        }

        void InitialiseIOCoordinator(string markdown)
        {
            ioCoordinator.Initialise(markdown.Split('\n').ToList());
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
        private InfiniteRecursionChecker infiniteRecursionChecker;
    }

    public class InfiniteRecursionChecker
    {
        private int currentLineIndex;

        public void CheckCurrentLineIndexForInfiniteRecursion(int currentLineIndex, string currentLine)
        {
            if (this.currentLineIndex == currentLineIndex)
                throw new MarkdownInternalException($"Internal parser error. Line '{currentLine}' would have caused an infinite loop.");
        }

        public void SaveCurrentLineIndex(int currentLineIndex) => this.currentLineIndex = currentLineIndex;
    }

}