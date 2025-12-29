using Core.Services.Helper.Interface;
using ExCSS;

namespace Core.Services.Helper;

public class CssService : ICssService
{
    public async Task<string> Parse(string css)
    {
        StylesheetParser parser = new();
        Stylesheet? stylesheet = await parser.ParseAsync(css);
        string? cssString = stylesheet.ToCss();
        return cssString ?? "";
    }
}