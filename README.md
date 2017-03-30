# Exercism C# Markdown problem
 
[Submission on Exercism](http://exercism.io/submissions/6ead0d9fc8954358bb21113768093662)

## Aims of my refactoring

My primary goal was to optimise for human understandability.

The techniques I used to achieve this are:
* Stick to single responsibility principle as much as possible, especially in the case of the unordered list code being spread everywhere
* Keep total variable live and span time as low as possible
* Encapsulate and obey the law of demeter
* Name methods that return values to describe the value they return
* Name methods that return nothing to describe what they do in Verb-Noun format
* Name variables to describe what they are
* Name classes to desribe the single responsibility
* Program defensively
* Use specific exceptions

## Thoughts

Creating the extendable architecture with IMarkdownToHtmlTag just for the UnorderdedList and Header elements is overkill in this situation, but would get increasingly more relevant as the number of elements increases, which I think it would do in a fully featured markdown parser.
