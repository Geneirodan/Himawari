using Himawari.SpellChecking.Extensions;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace Himawari.SpellChecking.Tests.Extensions;

[TestSubject(typeof(ReadOnlyDictionaryExtensions))]
public class ReadOnlyDictionaryExtensionsTests
{
    private readonly Dictionary<char, char> _map = new()
    {
        ['a'] = 'b', ['c'] = 'd', ['e'] = 'f', ['g'] = 'h', ['i'] = 'j',
        ['k'] = 'l', ['m'] = 'n', ['o'] = 'o', ['p'] = 'r', ['s'] = 't',
        ['u'] = 'v', ['v'] = 'w', ['w'] = 'x', ['x'] = 'y', ['y'] = 'z'
    };

    [Theory]
    [MemberData(nameof(Generator))]
    public void Translate_ShouldReturnExpectedResults(string source, string expected) => 
        _map.Translate(source).ShouldBe(expected);

    public static TheoryData<string,string> Generator() => new()
        {
            { "a", "b" },
            { "ce", "df" },
            { "eca", "fdb" }
        };
}