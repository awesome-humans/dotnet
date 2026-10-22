// awesome-humans/dotnet template | targets: net10.0, csharp-14 | last-reviewed: 2026-10-22 | sources: dotnet-blog, ms-learn
// Belongs to Example.Library. Library surface example: C# 14 extension members (opinions/csharp.md).
namespace Example.Library;

public static class TextExtensions
{
    extension(string text)
    {
        /// <summary>A fact about the instance, so a property.</summary>
        public bool IsBlank => string.IsNullOrWhiteSpace(text);

        /// <summary>A transform that takes an argument, so a method.</summary>
        public string Truncate(int maxLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
            return text.Length <= maxLength ? text : text[..maxLength];
        }
    }
}
