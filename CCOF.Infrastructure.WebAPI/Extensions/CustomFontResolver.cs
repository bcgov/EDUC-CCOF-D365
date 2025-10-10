using PdfSharp.Fonts;
using System.Collections.Generic;
using System.IO;
namespace CCOF.Infrastructure.WebAPI.Extensions;
public class CustomFontResolver : IFontResolver
{
    private readonly Dictionary<string, byte[]> _fontData = new();

    public CustomFontResolver()
    {
        // Load font data once
        _fontData["OpenSans#Regular"] = File.ReadAllBytes("Fonts/OpenSans-Regular.ttf");
        _fontData["OpenSans#Bold"] = File.ReadAllBytes("Fonts/OpenSans-Bold.ttf");
        _fontData["Calibri#Regular"] = File.ReadAllBytes("Fonts/Calibri-Regular.ttf");
        _fontData["Calibri#Bold"] = File.ReadAllBytes("Fonts/Calibri-Bold.ttf");
    }

    public byte[] GetFont(string faceName)
    {
        if (_fontData.TryGetValue(faceName, out var data))
            return data;

        throw new InvalidOperationException($"Font '{faceName}' not found.");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Equals("OpenSans", StringComparison.OrdinalIgnoreCase))
        {
            if (isBold)
                return new FontResolverInfo("OpenSans#Bold");
            else
                return new FontResolverInfo("OpenSans#Regular");
        }
        if (familyName.Equals("Calibri", StringComparison.OrdinalIgnoreCase))
        {
            if (isBold)
                return new FontResolverInfo("Calibri#Bold");
            else
                return new FontResolverInfo("Calibri#Regular");
        }
        // Fallback to default
        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }
}
