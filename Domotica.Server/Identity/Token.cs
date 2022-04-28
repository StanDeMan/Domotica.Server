using shortid;
using shortid.Configuration;

namespace Domotica.Server.Identity;

public class Token
{
    public int Seed { get; } = 19641108;    // take birthday as default seed (yyyymmdd)

    public Token(int? seed = null)
    {
        ShortId.SetSeed(seed ?? Seed);
    }

    public string Generate(int length = 12)
    {
        if (length is < 8 or > 12)
        {
            throw new ArgumentException(
                "Must be in the range: length > 7 and length < 13",
                nameof(length));
        }

        return ShortId.Generate(new GenerationOptions(
            useNumbers: true, 
            useSpecialCharacters: 
            false, length));
    }
}