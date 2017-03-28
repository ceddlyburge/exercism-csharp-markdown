namespace MarkdownToHtml
{
    internal interface IMarkdownToHtmlTag
    {
        bool CanParseCurrentLine { get; }

        void WriteHtmlTag();
    }
}