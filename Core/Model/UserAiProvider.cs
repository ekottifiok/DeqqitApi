using Core.Model.Helper;
using Microsoft.EntityFrameworkCore;

namespace Core.Model;

[Owned]
public class UserAiProvider
{
    public int Id { get; set; }
    public UserAiProviderType Type { get; set; }
    public string Key { get; set; }
}