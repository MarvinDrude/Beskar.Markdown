using System.Runtime.InteropServices;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Beskar.Markdown.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing;

/// <summary>
/// Main parser struct to run the raw text to AST conversion.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public ref struct MarkdownParser<TData>(
   ReadOnlySpan<char> rawText,
   Span<MarkdownNode> initialNodeBuffer)
   : IDisposable
{
   public ReadOnlySpan<MarkdownNode> WrittenNodes => _writer.WrittenSpan;

   private readonly ReadOnlySpan<char> _sourceText = rawText;
   private ReadOnlySpan<char> _rawText = rawText;
   private BufferWriter<MarkdownNode> _writer = new(initialNodeBuffer);

   /// <summary>
   /// Main parse logic to construct the AST of the Markdown document.
   /// </summary>
   public MarkdownContext<TData> Parse(ParserOptions options, TData? data = default)
   {
      var context = new MarkdownContext<TData>
      {
         Data = data
      };

      var initialOffset = 0;
      
      if (options.ParseFrontMatter)
      {
         initialOffset = FrontMatterUtils.ParseFrontMatter(ref _rawText, context);
      }

      var documentIndex = _writer.WrittenSpan.Length;
      _writer.Add(new MarkdownNode()
      {
         Type = NodeType.Document,
         FirstChildIndex = -1,
         LastChildIndex = -1,
         NextSiblingIndex = -1
      });

      // Secured to be a max length of 32 by setter
      Span<int> openBlocks = stackalloc int[options.MaxBlockDepth];
      openBlocks[0] = documentIndex;
      var openBlockCount = 1;

      var iterator = new LineIterator(_rawText, initialOffset, _sourceText);
      var lastLineWasBlank = false;

      while (iterator.TryMoveNext(context, out var state))
      {
         var currentLineIsBlank = state.IsBlank;
         var matchedLevels = 1; // ignore the document node

         // Phase one: continue open blocks if possible
         for (var i = 1; i < openBlockCount; i++)
         {
            var nodeIdx = openBlocks[i];
            ref var node = ref _writer.GetReference(nodeIdx);

            // check for interrupts
            if (node.Type is NodeType.Paragraph)
            {
               if (!state.IsBlank) matchedLevels++;
               break;
            }

            var parser = options.GetParserForType((int)node.Type);
            if (parser == null || !parser.CanContinue(ref node, ref state, ref _writer))
            {
               break;
            }

            matchedLevels++;
         }

         var originalOpenBlockCount = openBlockCount;
         openBlockCount = matchedLevels;
         var matchedNew = false;

         // Phase two: try to match new blocks
         while (openBlockCount < options.MaxBlockDepth)
         {
            var currentParentIndex = openBlocks[openBlockCount - 1];
            var foundNewNodeIndex = -1;

            // Check if we are currently inside a paragraph that might be interrupted
            var isParagraphOpen = _writer.WrittenSpan[currentParentIndex].Type is NodeType.Paragraph;
            var isLazyParagraph = !isParagraphOpen && openBlockCount == matchedLevels &&
                matchedLevels < originalOpenBlockCount &&
                _writer.WrittenSpan[openBlocks[originalOpenBlockCount - 1]].Type ==
                NodeType.Paragraph;

            var testParentIndex = isParagraphOpen ? openBlocks[openBlockCount - 2] : currentParentIndex;

            if (_writer.WrittenSpan[testParentIndex].Type is NodeType.List)
            {
               var listItemParser = options.GetParserForType((int)NodeType.ListItem);

               if (listItemParser == null
                   || !ShouldTryBlockParser((int)NodeType.ListItem, ref state))
               {
                  if (state.IsBlank || isLazyParagraph)
                  {
                     break;
                  }

                  openBlockCount--;
                  if (openBlockCount <= 0)
                  {
                     openBlockCount = 1;
                     break;
                  }

                  continue;
               }

               var tempState = state;
               foundNewNodeIndex = listItemParser.TryMatch(ref tempState, testParentIndex, ref _writer);

               if (foundNewNodeIndex == -1)
               {
                  if (isLazyParagraph
                      && (state.LeadingSpaces >= 4
                          || !IsPossibleListMarkerStart(state.FirstChar)))
                  {
                     break;
                  }

                  openBlockCount--;
                  if (openBlockCount <= 0)
                  {
                     openBlockCount = 1;
                     break;
                  }

                  continue;
               }

               state = tempState;
            }

            if (isParagraphOpen)
            {
               if (TryMatchSetextUnderline(ref state, currentParentIndex))
               {
                  openBlockCount--; // close the converted heading
                  matchedNew = true;
                  break;
               }

               if (TryMatchTableDelimiter(ref state, currentParentIndex, testParentIndex, ref _writer,
                      out var tableIndex))
               {
                  openBlocks[openBlockCount - 1] = tableIndex;
                  matchedNew = true;
                  break;
               }
            }

            if (foundNewNodeIndex == -1)
            {
               for (var index = 0; index < options.BlockParsers.Length; index++)
               {
                  var parser = options.BlockParsers[index];

                  if (parser.SupportedTypeValue == (int)NodeType.Paragraph)
                  {
                     continue;
                  }

                  if ((isParagraphOpen || isLazyParagraph) &&
                      parser.SupportedTypeValue is (int)NodeType.LinkReferenceDefinition
                         or (int)NodeType.IndentedCodeBlock)
                  {
                     continue;
                  }

                  if (!ShouldTryBlockParser(parser.SupportedTypeValue, ref state))
                  {
                     continue;
                  }

                  var tempState = state;
                  foundNewNodeIndex = parser.TryMatch(ref tempState, testParentIndex, ref _writer);

                  if (foundNewNodeIndex != -1)
                  {
                     state = tempState; // commit state change
                     break;
                  }
               }
            }

            if (foundNewNodeIndex != -1)
            {
               matchedNew = true;

               if (lastLineWasBlank)
               {
                  ref var parentForWrap = ref _writer.GetReference(currentParentIndex);
                  if (parentForWrap.Type == NodeType.ListItem && parentForWrap.FirstChildIndex != -1)
                  {
                     var childIdx = parentForWrap.FirstChildIndex;
                     while (childIdx != -1)
                     {
                        ref var child = ref _writer.GetReference(childIdx);
                        if (child.Type == NodeType.Paragraph)
                           child.ParagraphIsWrapped = 1;
                        childIdx = child.NextSiblingIndex;
                     }
                  }
               }

               lastLineWasBlank = false;

               if (isParagraphOpen)
               {
                  openBlockCount--;
                  currentParentIndex = testParentIndex;
               }

               LinkNodes(currentParentIndex, foundNewNodeIndex);

               var newType = (int)_writer.WrittenSpan[foundNewNodeIndex].Type;
               if (options.IsParserType(newType))
               {
                  openBlocks[openBlockCount++] = foundNewNodeIndex;
                  continue;
               }

               // Its a leaf (Heading, Code), no more nesting possible
            }

            break; // no more block parsers matched
         }

         if (!matchedNew && !state.IsBlank && openBlockCount < originalOpenBlockCount)
         {
            var lastIdx = openBlocks[originalOpenBlockCount - 1];
            if (_writer.WrittenSpan[lastIdx].Type is NodeType.Paragraph)
            {
               openBlockCount = originalOpenBlockCount;
            }
         }

         var isBlankLine = currentLineIsBlank;
         if (!matchedNew || !state.IsBlank)
         {
            var currentParentIndex = openBlocks[openBlockCount - 1];
            ref var parentNode = ref _writer.GetReference(currentParentIndex);

            if (state.IsBlank && parentNode.Type is NodeType.Paragraph)
            {
               // close paragraph on a blank line
               openBlockCount--;
            }
            else
            {
               if (parentNode.Type is NodeType.Paragraph)
               {
                  // Continuation: Extend the TextSpan of the existing paragraph
                  var textIndex = _writer.WrittenSpan.Length;
                  var trailingSpaces = SpanUtils.CountTrailingSpaces(state.RawLine);

                  if (trailingSpaces > 0 && SpanUtils.IsHardBreak(state.RawLine))
                  {
                     // preserve the spaces for the inline parser to handle the hard break
                     trailingSpaces = 0;
                  }

                  _writer.Add(new MarkdownNode()
                  {
                     Type = NodeType.Text,
                     TextSpan = new TextSpan(state.GlobalOffset + state.FirstNonSpaceIndex,
                        state.RawLine.Length - state.FirstNonSpaceIndex - trailingSpaces),
                     FirstChildIndex = -1,
                     LastChildIndex = -1,
                     NextSiblingIndex = -1
                  });

                  LinkNodes(currentParentIndex, textIndex);
                  state.ConsumeRest();
               }
               else
               {
                  // Start a new paragraph
                  var pParser = options.GetParserForType((int)NodeType.Paragraph);
                  if (pParser != null)
                  {
                     var pIndex = pParser.TryMatch(ref state, currentParentIndex, ref _writer);
                     if (pIndex != -1)
                     {
                        ref var pNode = ref _writer.GetReference(pIndex);
                        ref var parentNodeRef = ref _writer.GetReference(currentParentIndex);

                        var isFirstChild = parentNodeRef.FirstChildIndex == -1;
                        var previousSiblingType = isFirstChild
                           ? NodeType.Document
                           : _writer.WrittenSpan[parentNodeRef.LastChildIndex].Type;
                        var isLooseListItem = !isFirstChild && lastLineWasBlank;

                        if (parentNodeRef.Type != NodeType.ListItem
                            || (isFirstChild && lastLineWasBlank)
                            || (!isFirstChild && previousSiblingType is not (NodeType.Paragraph or NodeType.Header))
                            || isLooseListItem)
                        {
                           pNode.ParagraphIsWrapped = 1;
                        }

                        if (isLooseListItem)
                        {
                           var childIdx = parentNodeRef.FirstChildIndex;
                           while (childIdx != -1)
                           {
                              ref var child = ref _writer.GetReference(childIdx);
                              if (child.Type == NodeType.Paragraph)
                                 child.ParagraphIsWrapped = 1;
                              childIdx = child.NextSiblingIndex;
                           }
                        }

                        LinkNodes(currentParentIndex, pIndex);
                        if (openBlockCount < options.MaxBlockDepth)
                        {
                           openBlocks[openBlockCount++] = pIndex;
                        }
                     }
                  }
               }
            }
         }

         lastLineWasBlank = isBlankLine;
      }

      MarkListsLooseFromBlankLinesBetweenItems();
      ApplyLooseListParagraphs();

      // process inlines
      var inlineParser = new InlineParser<TData>(_sourceText);
      inlineParser.Parse(ref _writer, context, options);

      return context;
   }

   private void LinkNodes(int parentIndex, int childIndex)
   {
      ref var parent = ref _writer.GetReference(parentIndex);

      if (parent.FirstChildIndex == -1)
      {
         parent.FirstChildIndex = childIndex;
         parent.LastChildIndex = childIndex;
         return;
      }

      ref var lastChild = ref _writer.GetReference(parent.LastChildIndex);
      lastChild.NextSiblingIndex = childIndex;
      parent.LastChildIndex = childIndex;
   }

   private void ApplyLooseListParagraphs()
   {
      var nodes = _writer.WrittenSpan;
      for (var i = 0; i < nodes.Length; i++)
      {
         if (nodes[i] is not { Type: NodeType.List, IsLoose: 1 })
         {
            continue;
         }

         var itemIndex = nodes[i].FirstChildIndex;
         while (itemIndex != -1)
         {
            var item = nodes[itemIndex];
            if (item.Type == NodeType.ListItem)
            {
               var childIndex = item.FirstChildIndex;
               while (childIndex != -1)
               {
                  ref var child = ref _writer.GetReference(childIndex);
                  if (child.Type == NodeType.Paragraph)
                  {
                     child.ParagraphIsWrapped = 1;
                  }

                  childIndex = child.NextSiblingIndex;
               }
            }

            itemIndex = item.NextSiblingIndex;
         }
      }
   }

   private void MarkListsLooseFromBlankLinesBetweenItems()
   {
      var nodes = _writer.WrittenSpan;
      for (var i = 0; i < nodes.Length; i++)
      {
         if (nodes[i].Type != NodeType.List)
         {
            continue;
         }

         var itemIndex = nodes[i].FirstChildIndex;
         while (itemIndex != -1)
         {
            var item = nodes[itemIndex];
            if (ContainsBlankLineBetweenBlockChildren(item))
            {
               _writer.GetReference(i).IsLoose = 1;
               break;
            }

            var nextItemIndex = item.NextSiblingIndex;
            if (nextItemIndex != -1)
            {
               var itemEnd = GetNodeEnd(itemIndex);
               var nextItemStart = nodes[nextItemIndex].TextSpan.Start;
               if (itemEnd >= 0
                   && nextItemStart > itemEnd
                   && ContainsBlankLine(_sourceText[itemEnd..nextItemStart]))
               {
                  _writer.GetReference(i).IsLoose = 1;
                  break;
               }
            }

            itemIndex = nextItemIndex;
         }
      }
   }

   private bool ContainsBlankLineBetweenBlockChildren(MarkdownNode item)
   {
      var childIndex = item.FirstChildIndex;
      while (childIndex != -1)
      {
         var child = _writer.WrittenSpan[childIndex];
         var nextChildIndex = child.NextSiblingIndex;
         if (nextChildIndex != -1)
         {
            var childEnd = GetNodeEnd(childIndex);
            var nextChildStart = GetNodeStart(nextChildIndex);
            var nextChild = _writer.WrittenSpan[nextChildIndex];

            if (childEnd >= 0
                && nextChildStart > childEnd
                && ContainsBlankLine(
                   _sourceText[childEnd..nextChildStart],
                   child.Type == NodeType.Paragraph && nextChild.Type == NodeType.Paragraph))
            {
               return true;
            }
         }

         childIndex = nextChildIndex;
      }

      return false;
   }

   private int GetNodeEnd(int nodeIndex)
   {
      var node = _writer.WrittenSpan[nodeIndex];
      var end = node.TextSpan.Start >= 0
         ? node.TextSpan.Start + node.TextSpan.Length
         : -1;

      var childIndex = node.FirstChildIndex;
      while (childIndex != -1)
      {
         var childEnd = GetNodeEnd(childIndex);
         if (childEnd > end)
         {
            end = childEnd;
         }

         childIndex = _writer.WrittenSpan[childIndex].NextSiblingIndex;
      }

      return end;
   }

   private int GetNodeStart(int nodeIndex)
   {
      var node = _writer.WrittenSpan[nodeIndex];
      var start = node.TextSpan is { Start: >= 0, Length: > 0 }
         ? node.TextSpan.Start
         : -1;

      var childIndex = node.FirstChildIndex;
      while (childIndex != -1)
      {
         var childStart = GetNodeStart(childIndex);
         if (childStart >= 0 && (start < 0 || childStart < start))
         {
            start = childStart;
         }

         childIndex = _writer.WrittenSpan[childIndex].NextSiblingIndex;
      }

      return start;
   }

   private static bool ContainsBlankLine(ReadOnlySpan<char> text, bool allowBlockQuoteMarkers = false)
   {
      var lineHasContent = true;
      for (var i = 0; i < text.Length; i++)
      {
         var c = text[i];
         if (c is '\r' or '\n')
         {
            if (!lineHasContent)
            {
               return true;
            }

            lineHasContent = false;
            if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
               i++;
            }
         }
         else if (c is not (' ' or '\t')
                  && (!allowBlockQuoteMarkers || c != '>'))
         {
            lineHasContent = true;
         }
      }

      return false;
   }

   private bool TryMatchSetextUnderline(ref LineState<TData> state, int paragraphIndex)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4) return false;

      var marker = state.FirstChar;
      if (marker != '=' && marker != '-') return false;

      var line = state.RawLine;
      var i = state.FirstNonSpaceIndex;

      while (i < line.Length && line[i] == marker)
      {
         i++;
      }

      while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
      {
         i++;
      }

      if (i < line.Length) return false;

      ref var para = ref _writer.GetReference(paragraphIndex);

      if (para.LastChildIndex != -1)
      {
         ref var lastChild = ref _writer.GetReference(para.LastChildIndex);
         if (lastChild.Type == NodeType.Text)
         {
            var span = lastChild.TextSpan;
            var content = _sourceText.Slice(span.Start, span.Length);
            var trimCount = 0;

            // Look back from the end of the span for whitespace
            while (trimCount < content.Length
                   && char.IsWhiteSpace(content[content.Length - 1 - trimCount]))
            {
               trimCount++;
            }

            if (trimCount > 0)
            {
               lastChild.TextSpan = span with { Length = span.Length - trimCount };
            }
         }
      }

      para.Type = NodeType.Header;
      para.HeadingLevel = marker == '=' ? 1 : 2;

      state.ConsumeRest();
      return true;
   }

   private static bool ShouldTryBlockParser(int parserType, ref LineState<TData> state)
   {
      if (parserType is > ParserConstants.MaxInbuiltParseValue or < 0)
      {
         return true;
      }

      if (state.IsBlank)
      {
         return false;
      }

      var firstChar = state.FirstChar;
      return (NodeType)parserType switch
      {
         NodeType.CodeBlock => state.LeadingSpaces < 4 && firstChar is '`' or '~',
         NodeType.IndentedCodeBlock => state.LeadingSpaces >= 4,
         NodeType.Header => state.LeadingSpaces < 4 && firstChar == '#',
         NodeType.ThematicBreak => state.LeadingSpaces < 4 && firstChar is '*' or '-' or '_',
         NodeType.List => state.LeadingSpaces < 4 && IsPossibleListMarkerStart(firstChar),
         NodeType.ListItem => IsPossibleListMarkerStart(firstChar),
         NodeType.BlockQuote => firstChar == '>',
         NodeType.HtmlBlock => state.LeadingSpaces < 4 && firstChar == '<',
         _ => true
      };
   }

   private static bool IsPossibleListMarkerStart(char c)
   {
      return c is '-' or '*' or '+' || char.IsAsciiDigit(c);
   }

   private bool TryMatchTableDelimiter(ref LineState<TData> state, int paragraphIndex, int parentIndex,
      ref BufferWriter<MarkdownNode> writer, out int tableIndex)
   {
      tableIndex = -1;
      if (state.IsBlank || state.LeadingSpaces >= 4) return false;

      if (!TableUtils.IsDelimiterRow(state.RawLine[state.FirstNonSpaceIndex..], out var columnCount)) return false;

      ref var para = ref writer.GetReference(paragraphIndex);
      if (para.LastChildIndex == -1) return false;

      ref var lastChild = ref writer.GetReference(para.LastChildIndex);
      if (lastChild.Type != NodeType.Text) return false;

      var headerLine = _sourceText.Slice(lastChild.TextSpan.Start, lastChild.TextSpan.Length);
      var headerColumnCount = TableUtils.CountHeaderColumns(headerLine);

      if (headerColumnCount != columnCount) return false;

      var alignments = TableUtils.ParseAlignments(state.RawLine[state.FirstNonSpaceIndex..]);

      var prevChildIndex = -1;
      var currentChild = para.FirstChildIndex;

      while (currentChild != -1 && currentChild != para.LastChildIndex)
      {
         prevChildIndex = currentChild;
         currentChild = writer.WrittenSpan[currentChild].NextSiblingIndex;
      }

      if (prevChildIndex != -1)
      {
         writer.GetReference(prevChildIndex).NextSiblingIndex = -1;
         para.LastChildIndex = prevChildIndex;

         tableIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode
         {
            Type = NodeType.Table,
            FirstChildIndex = -1,
            LastChildIndex = -1,
            NextSiblingIndex = -1
         });

         LinkNodes(parentIndex, tableIndex);
      }
      else
      {
         para.Type = NodeType.Table;
         para.FirstChildIndex = -1;
         para.LastChildIndex = -1;

         tableIndex = paragraphIndex;
      }

      var headerNodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode
      {
         Type = NodeType.TableHeader,
         FirstChildIndex = -1,
         LastChildIndex = -1,
         NextSiblingIndex = -1
      });
      LinkNodes(tableIndex, headerNodeIndex);

      var headerRowIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode
      {
         Type = NodeType.TableRow,
         FirstChildIndex = -1,
         LastChildIndex = -1,
         NextSiblingIndex = -1
      });
      LinkNodes(headerNodeIndex, headerRowIndex);

      TableUtils.ParseRow(headerLine, columnCount, alignments, headerRowIndex,
         ref writer, lastChild.TextSpan.Start, isHeader: true);

      var bodyNodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode
      {
         Type = NodeType.TableBody,
         FirstChildIndex = -1,
         LastChildIndex = -1,
         NextSiblingIndex = -1
      });
      LinkNodes(tableIndex, bodyNodeIndex);

      state.ConsumeRest();
      return true;
   }

   public void Dispose()
   {
      _writer.Dispose();
   }
}
