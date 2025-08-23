using System.Runtime.Serialization;

namespace Himawari.VideoParser.CobaltTools;

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