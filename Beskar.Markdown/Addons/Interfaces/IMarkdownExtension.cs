using Beskar.Markdown.Rendering.Interfaces;

namespace Beskar.Markdown.Addons.Interfaces;

public interface IMarkdownExtension
{
   public INodeRenderer[] Renderers { get; set; }
}