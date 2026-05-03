using System.Runtime.InteropServices;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Extensions;

public static class BufferWriterExtensions
{
   extension<T>(ref BufferWriter<T> writer)
   {
      public ref T GetReference(int index)
      {
         return ref MemoryMarshal.GetReference(writer.WrittenSpan.Slice(index, 1));
      }
   }
}