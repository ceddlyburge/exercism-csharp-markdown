namespace MarkdownToHtml
{
    public class InfiniteRecursionChecker
    {
        int currentLineIndex;

        public void SaveCurrentLineIndex(int currentLineIndex) =>
            this.currentLineIndex = currentLineIndex;

        public void CheckCurrentLineIndexForInfiniteRecursion(int currentLineIndex, string currentLine)
        {
            if (this.currentLineIndex == currentLineIndex)
                throw new MarkdownInternalException($"Internal parser error. Line '{currentLine}' would have caused an infinite loop.");
        }
    }

}