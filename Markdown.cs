using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkdownToHtml
{
    public class Markdown
    {
        readonly MarkdownHtmlIoCoordinator ioCoordinator;
        readonly IReadOnlyList<IMarkdownToHtmlTag> markdownToHtmls;
        readonly MarkdownToHtmlParagraphTag paragraph;
        readonly InfiniteRecursionChecker infiniteRecursionChecker;

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

            Parse();

            return Html;
        }

        void Initialise(string markdown)
        {
            InitialiseIOCoordinator(markdown);

            MoveToFirstLine();
        }

        void Parse()
        {
            while (CurrentLineExists)
                ParseAtCurrentLine();
        }

        void ParseAtCurrentLine()
        {
            SaveCurrentLineIndex();

            ApplyParsersAtCurrentLine();

            CheckCurrentLineIndexForInfiniteRecursion();
        }

        bool CurrentLineExists => 
            ioCoordinator.CurrentLineExists;

        void SaveCurrentLineIndex() =>
            infiniteRecursionChecker.SaveCurrentLineIndex(CurrentLineIndex);

        void ApplyParsersAtCurrentLine()
        {
            // the paragraph parser is special, in that it applies if nothing else is able to work
            if (CurrentLineExists && markdownToHtmls.Any(m => m.CanParseCurrentLine) == false)
                paragraph.WriteParagraphTag();

            // this could apply more than one markdownToHtml, as all the parsers are called and they update CurrentLine as they go.
            markdownToHtmls.ToList().ForEach(m => ApplyMarkdownToHtml(m));
        }

        void CheckCurrentLineIndexForInfiniteRecursion() =>
            infiniteRecursionChecker.CheckCurrentLineIndexForInfiniteRecursion(CurrentLineIndex, CurrentLine);

        void InitialiseIOCoordinator(string markdown)
        {
            ioCoordinator.Initialise(markdown.Split('\n').ToList());
        }

        void MoveToFirstLine() => 
            ioCoordinator.MoveToFirstLine();

        void ApplyMarkdownToHtml(IMarkdownToHtmlTag markdownToHtml)
        {
            if (markdownToHtml.CanParseCurrentLine)
                markdownToHtml.WriteHtmlTag();
        }

        int CurrentLineIndex => 
            ioCoordinator.CurrentLineIndex;

        string CurrentLine => 
            ioCoordinator.CurrentLine;

        string Html => 
            ioCoordinator.Html;
    }
}