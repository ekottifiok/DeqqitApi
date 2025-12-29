using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Core.Services.Helper.Interface;

public class HtmlService : IHtmlService
{
    public async Task<string?> Parse(string html)
    {
        HtmlParser htmlParser = new();
        IHtmlDocument document = await htmlParser.ParseDocumentAsync(html);
        return document.Body?.ToHtml();
    }
}