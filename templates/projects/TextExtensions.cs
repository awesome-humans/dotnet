// awesome-humans/dotnet template | targets: net10.0, csharp-14 | last-reviewed: 2026-10-22 | sources: dotnet-blog, ms-learn
// Belongs to Example.Library. Library surface example: C# 14 extension members (opinions/csharp.md).
namespace Example.Library;

public static class TextExtensions
{
    extension(string text)
    {
        /// <summary>States a fact about the instance, which makes it a property.</summary>
        public bool IsBlank => string.IsNullOrWhiteSpace(text);

        /// <summary>Transforms the text using an argument, which makes it a method.</summary>
        public string Truncate(int maxLength)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
            return text.Length <= maxLength ? text : text[..maxLength];
        }
    }
}
