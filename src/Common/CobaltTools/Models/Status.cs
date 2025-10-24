using System.Runtime.Serialization;

namespace Himawari.CobaltTools.Models;

public enum Status
{
    [EnumMember(Value = "tunnel")]
    Tunnel,
    [EnumMember(Value = "local-processing")]
    LocalProcessing,
    [EnumMember(Value = "redirect")] 
    Redirect,
    [EnumMember(Value = "picker")] 
    Picker,
    [EnumMember(Value = "error")]
    Error
}