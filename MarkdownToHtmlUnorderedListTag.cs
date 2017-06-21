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
            WriteListItems();

            WriteHtml("</ul>");
        }

        private void WriteListItems()
        {
            do
            {
                ParseListItem();
                MoveToNextLine();
            }
            while (CanParseCurrentLine);
        }

        void ParseListItem() => WriteTag("li", MarkdownMidlineIndicatorsReplacedWithHtmlTags(CurrentLineWithoutMarkdownListIndicator));

        string CurrentLineWithoutMarkdownListIndicator => CurrentLine.Substring(2);
    }
}