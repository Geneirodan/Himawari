using System.Text.Json.Serialization;

namespace Himawari.CobaltTools.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaType { Photo , Video , Gif, Audio }