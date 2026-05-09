using System.Diagnostics;
using System.Text;
using Beskar.Markdown;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

#if DEBUG

var md = File.ReadAllText("../../Release/net10.0/crash_repro.md");
using var parser = new MarkdownParser(md, stackalloc MarkdownNode[32]);
parser.Parse(ParserOptions.Default);

var debugString = parser.WrittenNodes.ToArray().CreateDebugString(md);

var renderer = new MarkdownRenderer(md);
var html = renderer.Render(parser.WrittenNodes, RenderOptions.HtmlDefault);

Console.WriteLine(html);

#else

Console.WriteLine("--- Beskar.Markdown Simple Fuzzer ---");
Console.WriteLine("Starting chaos input test. Press Ctrl+C to stop.");

var sw = Stopwatch.StartNew();
long totalIterations = 0;
long totalBytes = 0;

var interestingChars = "#*_[]()<>! \t\n\r-+.:/\\`~|0123456789abcdefABCDEF".ToCharArray();

while (true)
{
    totalIterations++;
    
    var maxLength = totalIterations % 100 == 0 ? 100000 : 1000;
    var length = Random.Shared.Next(0, maxLength);
    var sb = new StringBuilder(length);
    
    var strategy = Random.Shared.Next(0, 4);
    
    switch (strategy)
    {
        case 0:
            for (var i = 0; i < length; i++)
            {
                var r = Random.Shared.Next(0, 100);
                if (r < 80) sb.Append(interestingChars[Random.Shared.Next(interestingChars.Length)]);
                else if (r < 95) sb.Append((char)Random.Shared.Next(32, 127));
                else sb.Append((char)Random.Shared.Next(0, 0xFFFF));
            }
            break;
            
        case 1:
            var repeatChar = interestingChars[Random.Shared.Next(interestingChars.Length)];
            sb.Append(new string(repeatChar, length));
            break;
            
        case 2:
            var lines = Random.Shared.Next(1, 20);
            for (int l = 0; l < lines; l++)
            {
                var lineLen = Random.Shared.Next(0, 100);
                for (var i = 0; i < lineLen; i++)
                {
                    sb.Append(interestingChars[Random.Shared.Next(interestingChars.Length)]);
                }
                sb.Append('\n');
            }
            break;

        case 3:
            char[] open = { '[', '(', '<', '*' };
            char[] close = { ']', ')', '>', '*' };
            var pairIdx = Random.Shared.Next(open.Length);
            for (var i = 0; i < length / 2; i++) sb.Append(open[pairIdx]);
            for (var i = 0; i < length / 2; i++) sb.Append(close[pairIdx]);
            break;
    }

    var input = sb.ToString();
    totalBytes += input.Length * sizeof(char);

    try
    {
        _ = BeMarkdown.ToHtml(input);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[!!!] CRASH DETECTED at iteration {totalIterations}");
        Console.WriteLine($"Strategy used: {strategy}");
        Console.WriteLine($"Input Length: {input.Length}");
        Console.WriteLine("--- INPUT START ---");
        // Print first 500 chars if too long
        Console.WriteLine(input.Length > 500 ? input[..500] + "..." : input);
        Console.WriteLine("--- INPUT END ---");
        Console.WriteLine("Exception:");
        Console.WriteLine(ex);
        
        // Write to file for reproduction
        try 
        {
            File.WriteAllText("crash_repro.md", input);
            Console.WriteLine("\nCrash input saved to 'crash_repro.md'");
        }
        catch (Exception writeEx)
        {
            Console.WriteLine($"Failed to save repro file: {writeEx.Message}");
        }
        return;
    }

    if (totalIterations % 500 == 0)
    {
        var elapsed = sw.Elapsed.TotalSeconds;
        var ips = totalIterations / elapsed;
        Console.Write($"\rIterations: {totalIterations:N0} | IPS: {ips:F0} | Data: {totalBytes / 1024.0 / 1024.0:F2} MB    ");
    }
}

#endif
