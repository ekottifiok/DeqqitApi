using System.Text.RegularExpressions;
using Core.Model;
using Core.Services.Helper.Interface;
using Scriban;

namespace Core.Services.Helper;

public partial class TemplateService : ITemplateService
{
    public async Task<string> Parse(string text, Dictionary<string, string> data)
    {
        Template? template = Template.Parse(text);
        string? result = await template.RenderAsync(data);
        return result ?? "";
    }

    public List<string> GetAllFields(IEnumerable<string> templates)
    {
        return templates
            .SelectMany(t => MyRegex().Matches(t))
            .Select(m => m.Groups[1].Value.Trim())
            .Distinct()
            .ToList();
    }

    public static IEnumerable<string> GetField
        (IEnumerable<NoteTypeTemplate> template)
    {
        return template.Select(x => $"{x.Back}{x.Front}");
    }

    [GeneratedRegex(@"\{\{(.*?)\}\}")]
    private partial Regex MyRegex();
}