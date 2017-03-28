// there are a variety of interfaces and classes in this file, which should all be in their own files, but I don't think exercism supports this.
namespace MarkdownToHtml
{
    internal interface IMarkdownToHtmlTag
    {
        bool CanParseCurrentLine { get; }

        void WriteHtmlTag();
    }
}