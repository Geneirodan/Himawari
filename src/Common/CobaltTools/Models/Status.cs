using System.Runtime.Serialization;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// CobaltTools API response status: how the media is delivered or that an error occurred.
/// </summary>
public enum Status
{
    /// <summary>Media is streamed through a tunnel.</summary>
    [EnumMember(Value = "tunnel")]
    Tunnel,
    /// <summary>Media is processed locally (e.g. merge, mute, remux).</summary>
    [EnumMember(Value = "local-processing")]
    LocalProcessing,
    /// <summary>Client should follow a redirect URL.</summary>
    [EnumMember(Value = "redirect")]
    Redirect,
    /// <summary>User must pick from multiple options (e.g. format).</summary>
    [EnumMember(Value = "picker")]
    Picker,
    /// <summary>Request failed; see error payload.</summary>
    [EnumMember(Value = "error")]
    Error
}