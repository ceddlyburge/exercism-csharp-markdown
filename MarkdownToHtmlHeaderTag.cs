using System.Linq;

namespace MarkdownToHtml
{
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
}