namespace MarkdownToHtml
{
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
}