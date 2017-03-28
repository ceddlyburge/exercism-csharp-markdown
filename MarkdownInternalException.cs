using System;

namespace MarkdownToHtml
{
    public class MarkdownInternalException : Exception
    {
        public MarkdownInternalException(string message) : base(message)
        {
        }
    }
}