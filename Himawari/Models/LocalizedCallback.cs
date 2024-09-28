using System.Text.Json;
using Himawari.Enums;

namespace Himawari.Models;

public record LocalizedCallback(Callback Callback, string Language)
{
    public string Serialize() => JsonSerializer.Serialize(this);
}