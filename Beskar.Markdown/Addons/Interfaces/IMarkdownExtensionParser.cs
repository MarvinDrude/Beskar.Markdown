namespace Beskar.Markdown.Addons.Interfaces;

public interface IMarkdownExtensionParser
{
   public int Priority { get; }
   public int SupportedTypeValue { get; }
}