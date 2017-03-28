namespace MarkdownToHtml
{
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
}