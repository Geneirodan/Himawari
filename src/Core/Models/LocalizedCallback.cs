using System.Text.Json;
using Himawari.Core.Enums;

namespace Himawari.Core.Models;

public record LocalizedCallback(Callback Callback, string Language)
{
    public string Serialize() => JsonSerializer.Serialize(this);
}