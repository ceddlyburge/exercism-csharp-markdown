using System.Collections.Generic;
using System.Text;

namespace MarkdownToHtml
{
    internal class MarkdownHtmlIoCoordinator
    {
        internal MarkdownHtmlIoCoordinator()
        {
            html = new StringBuilder();
            lines = new List<string>(); // just to make sure its always initialised
        }

        internal void Initialise(IReadOnlyList<string> lines)
        {
            this.lines = lines;
            html.Clear();
        }

        internal int CurrentLineIndex => lineIndex;
        internal void MoveToNextLine() => lineIndex++;
        internal void MoveToFirstLine() => lineIndex = 0;
        internal bool CurrentLineExists => lineIndex < lines.Count;
        internal string CurrentLine => lines[lineIndex];

        internal void WriteHtml(string html) => this.html.Append(html);
        internal void WriteTag(string tag, string innerText) => WriteHtml($"<{tag}>{innerText}</{tag}>");
        internal string Html => html.ToString();

        int lineIndex;
        IReadOnlyList<string> lines;
        readonly StringBuilder html;
    }
}