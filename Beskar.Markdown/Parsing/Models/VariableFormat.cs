namespace Beskar.Markdown.Parsing.Models;

/// <summary>
/// Specifies the format for rendering a template variable.
/// </summary>
public enum VariableFormat : byte
{
   /// <summary>
   /// Standard plain text format (HTML-encoded on render).
   /// </summary>
   Text = 0,

   /// <summary>
   /// Markdown format (parsed and rendered to HTML).
   /// </summary>
   Markdown = 1,

   /// <summary>
   /// Raw HTML format (rendered 1:1 without encoding).
   /// </summary>
   Html = 2
}
