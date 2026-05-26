using System.Runtime.InteropServices;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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