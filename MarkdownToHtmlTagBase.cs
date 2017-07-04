using System.Text.RegularExpressions;
using System.Diagnostics.Contracts;

namespace MarkdownToHtml
{
    internal class MarkdownToHtmlTagBase
    {
        readonly MarkdownHtmlIoCoordinator ioCoordinator;

        public MarkdownToHtmlTagBase(MarkdownHtmlIoCoordinator inputOutputCoordinator)
        {
            Contract.Requires(inputOutputCoordinator != null);

            this.ioCoordinator = inputOutputCoordinator;
        }

        protected void MoveToNextLine() => 
            ioCoordinator.MoveToNextLine();

        protected bool CurrentLineExists => 
            ioCoordinator.CurrentLineExists;

        protected string CurrentLine => 
            ioCoordinator.CurrentLine;

        protected void WriteHtml(string html) => 
            ioCoordinator.WriteHtml(html);

        protected void WriteTag(string tag, string innerText) => 
            ioCoordinator.WriteTag(tag, innerText);

        protected static string MarkdownMidlineIndicatorsReplacedWithHtmlTags(string markdown) => 
            Markdown_IndicatorsReplacedWithHtmlEmTags(Markdown__IndicatorsReplacedWithHtmlStrongTags((markdown)));

        static string Markdown__IndicatorsReplacedWithHtmlStrongTags(string markdown) => 
            SingleMarkdownIndicatorsReplacedWithHtmlTags(markdown, "__", "strong");

        static string Markdown_IndicatorsReplacedWithHtmlEmTags(string markdown) =>
            SingleMarkdownIndicatorsReplacedWithHtmlTags(markdown, "_", "em");

        // this name isn't the most amazing, but couldn't think of anything better
        static string SingleMarkdownIndicatorsReplacedWithHtmlTags(string markdown, string markdownIndicator, string htmlTag)
        {
            var pattern = $"{markdownIndicator}(.+){markdownIndicator}";
            var replacement = $"<{htmlTag}>$1</{htmlTag}>";
            return Regex.Replace(markdown, pattern, replacement);
        }

    }
}