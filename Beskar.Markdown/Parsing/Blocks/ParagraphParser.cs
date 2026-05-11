using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class ParagraphParser : IBlockParser
{
   public int Priority => 10;
   public int SupportedTypeValue => (int)NodeType.Paragraph;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank)
      {
         return -1;
      }
      
      var parent = writer.WrittenSpan[parentIndex];

      if (parent is { Type: NodeType.ListItem, FirstChildIndex: -1 })
      {
         // ugly nesting for now, maybe extract method later and early exit
         var line = state.RawLine[state.FirstNonSpaceIndex..];
         
         if (line.Length >= 3 && line[0] == '[' && line[2] == ']')
         {
            var markerChar = line[1];
            
            if (markerChar is ' ' or 'x' or 'X')
            {
               var isTask = line.Length == 3 || line[3] is ' ' or '\t';
               
               if (isTask)
               {
                  ref var parentNode = ref writer.GetReference(parentIndex);
                  parentNode.TaskListStatus = (byte)(markerChar == ' ' ? 1 : 2);
                  state.Slice(state.FirstNonSpaceIndex + (line.Length > 3 ? 4 : 3));
               }
            }
         }
      }
      
      var paraIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Paragraph,
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         LastChildIndex = -1,
         IsInsideListItem = (byte)(parent.Type is NodeType.ListItem ? 1 : 0),
      });

      if (!state.IsBlank)
      {
         var textIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.Text,
            TextSpan = new TextSpan(state.GlobalOffset + state.FirstNonSpaceIndex, state.RawLine.Length - state.FirstNonSpaceIndex),
            FirstChildIndex = -1,
            NextSiblingIndex = -1,
            LastChildIndex = -1,
         });

         ref var para = ref writer.GetReference(paraIndex);
         para.FirstChildIndex = textIndex;
         para.LastChildIndex = textIndex; 
      }

      state.ConsumeRest();
      return paraIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      return false;
   }
}
