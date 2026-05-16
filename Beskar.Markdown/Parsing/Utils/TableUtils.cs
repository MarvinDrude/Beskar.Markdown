using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Utils;

public static class TableUtils
{
    public static bool IsDelimiterRow(ReadOnlySpan<char> line, out int columnCount)
    {
        columnCount = 0;
        var i = 0;
      
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
      
        if (i >= line.Length) return false;
      
        var hasLeadingPipe = line[i] == '|';
        if (hasLeadingPipe) i++;
      
        var cellCount = 0;
        var hasSeenCell = false;
      
        while (i < line.Length)
        {
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
            if (i >= line.Length) break;
         
            if (line[i] == '|')
            {
                if (!hasSeenCell && cellCount > 0) 
                {
                    return false; 
                }
            
                cellCount++;
                hasSeenCell = false;
                i++;
            
                continue;
            }
         
            if (line[i] == ':')
            {
                i++;
            }
         
            var hyphenCount = 0;
            while (i < line.Length && line[i] == '-')
            {
                hyphenCount++;
                i++;
            }
         
            if (hyphenCount == 0) 
            {
                return false; 
            }
         
            if (i < line.Length && line[i] == ':')
            {
                i++;
            }
         
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
            if (i < line.Length && line[i] != '|') 
            {
                return false; 
            }
         
            hasSeenCell = true;
        }
      
        if (hasSeenCell) cellCount++;
      
        columnCount = cellCount;
        return columnCount > 0;
    }

    public static long ParseAlignments(ReadOnlySpan<char> line)
    {
        var i = 0;
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
        if (i < line.Length && line[i] == '|') i++;
        
        long alignments = 0;
        var cellIndex = 0;
        
        while (i < line.Length && cellIndex < 32)
        {
            var start = i;
            while (i < line.Length && line[i] != '|') i++;
            
            var cellStart = start;
            var cellEnd = i;
            
            while (cellStart < cellEnd && (line[cellStart] == ' ' || line[cellStart] == '\t')) cellStart++;
            while (cellEnd > cellStart && (line[cellEnd - 1] == ' ' || line[cellEnd - 1] == '\t')) cellEnd--;

            if (cellEnd > cellStart)
            {
                var left = line[cellStart] == ':';
                var right = line[cellEnd - 1] == ':';
                
                long val;
                
                if (left && right) val = 2; // center
                else if (right) val = 3; // right
                else if (left) val = 1; // left
                else val = 0; // none
                
                alignments |= (val << (cellIndex * 2));
                cellIndex++;
            }
            
            if (i < line.Length && line[i] == '|') i++;
        }
        return alignments;
    }

    public static int CountHeaderColumns(ReadOnlySpan<char> line)
    {
        var count = 0;
        var i = 0;
        
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
        if (i < line.Length && line[i] == '|') i++;
        
        while (i < line.Length)
        {
            var start = i;
            while (i < line.Length)
            {
                if (line[i] == '\\' 
                    && i + 1 < line.Length 
                    && line[i + 1] == '|')
                {
                    i += 2;
                    continue;
                }
                
                if (line[i] == '|') break;
                i++;
            }
            
            var cellEnd = i;
            var hasContent = false;
            
            for (var j = start; j < cellEnd; j++)
            {
                if (line[j] is not (' ' or '\t'))
                {
                    hasContent = true;
                    break;
                }
            }

            if (i < line.Length || hasContent)
            {
                count++;
            }
            
            if (i < line.Length && line[i] == '|') i++;
            
            if (i >= line.Length) break;
            var hasRemainingContent = false;
            
            for (var j = i; j < line.Length; j++)
            {
                if (line[j] is not (' ' or '\t'))
                {
                    hasRemainingContent = true;
                    break;
                }
            }

            if (!hasRemainingContent) break;
        }
        
        return count;
    }

    public static void ParseRow(ReadOnlySpan<char> line, int columnCount, long alignments, int tableRowIndex, 
        ref BufferWriter<MarkdownNode> writer, int globalOffset, bool isHeader)
    {
        var i = 0;
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
        if (i < line.Length && line[i] == '|') i++;
        
        for (var cellIndex = 0; cellIndex < columnCount; cellIndex++)
        {
            var cellNodeIndex = writer.WrittenSpan.Length;
            var alignment = (byte)((alignments >> (Math.Min(cellIndex, 31) * 2)) & 0x3);
            
            if (i >= line.Length)
            {
                writer.Add(new MarkdownNode()
                {
                    Type = NodeType.TableCell,
                    TextSpan = new TextSpan(globalOffset + line.Length, 0),
                    TableCellAlignment = alignment,
                    IsHeaderCell = (byte)(isHeader ? 1 : 0),
                    FirstChildIndex = -1,
                    LastChildIndex = -1,
                    NextSiblingIndex = -1
                });
            }
            else
            {
                var start = i;
                while (i < line.Length)
                {
                    if (line[i] == '\\' && i + 1 < line.Length && line[i + 1] == '|')
                    {
                        i += 2;
                        continue;
                    }
                    if (line[i] == '|') break;
                    i++;
                }
                
                var cellStart = start;
                var cellEnd = i;
                
                while (cellStart < cellEnd && (line[cellStart] == ' ' || line[cellStart] == '\t')) cellStart++;
                while (cellEnd > cellStart && (line[cellEnd - 1] == ' ' || line[cellEnd - 1] == '\t')) cellEnd--;
                
                writer.Add(new MarkdownNode()
                {
                    Type = NodeType.TableCell,
                    TextSpan = new TextSpan(globalOffset + cellStart, cellEnd - cellStart),
                    TableCellAlignment = alignment,
                    IsHeaderCell = (byte)(isHeader ? 1 : 0),
                    FirstChildIndex = -1,
                    LastChildIndex = -1,
                    NextSiblingIndex = -1
                });
                
                if (i < line.Length && line[i] == '|') i++;
            }
            
            LinkNodes(tableRowIndex, cellNodeIndex, ref writer);
        }
    }

    private static void LinkNodes(int parentIndex, int childIndex, ref BufferWriter<MarkdownNode> writer)
    {
        ref var parent = ref writer.GetReference(parentIndex);
        
        if (parent.FirstChildIndex == -1)
        {
            parent.FirstChildIndex = childIndex;
            parent.LastChildIndex = childIndex;
            return;
        }
        
        ref var lastChild = ref writer.GetReference(parent.LastChildIndex);
        
        lastChild.NextSiblingIndex = childIndex;
        parent.LastChildIndex = childIndex;
    }
}
