using System.Runtime.InteropServices;

namespace Beskar.Markdown.Parsing.Models;

[StructLayout(LayoutKind.Auto)]
public struct Delimiter
{
   public char Marker;

   public int NodeIndex;
   public int Length;

   public bool CanOpen;
   public bool CanClose;

   public bool Active;
}