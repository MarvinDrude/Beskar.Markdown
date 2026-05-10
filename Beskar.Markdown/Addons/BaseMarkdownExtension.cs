using Beskar.Markdown.Addons.Interfaces;
using Beskar.Markdown.Rendering.Interfaces;

namespace Beskar.Markdown.Addons;

public abstract class BaseMarkdownExtension : IMarkdownExtension
{
   public INodeRenderer[] Renderers { get; set; } = [];
}