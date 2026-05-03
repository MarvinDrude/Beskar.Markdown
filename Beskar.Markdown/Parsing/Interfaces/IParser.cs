namespace Beskar.Markdown.Parsing.Interfaces;

public interface IParser
{
   /// <summary>
   /// The priority of the parser. Higher priority parsers are tried first.
   /// </summary>
   public int Priority { get; }
   
   /// <summary>
   /// For built-in types it's 'NodeType'.Value, for custom types it's ur custom value
   /// above 10_000 should be safe.
   /// </summary>
   public int SupportedTypeValue { get; }
}