﻿using System.Text.Json;

namespace Himawari.Alias.Models;

public sealed record AliasCallbackData(AliasCallbackData.CallbackType Callback, string Language)
{
    public enum CallbackType
    {
        Choose,
        Restart,
        SeeWord,
        NextWord
    }

    public string Serialize() => JsonSerializer.Serialize(this);

    public static AliasCallbackData? Deserialize(string value) => JsonSerializer.Deserialize<AliasCallbackData>(value);
}