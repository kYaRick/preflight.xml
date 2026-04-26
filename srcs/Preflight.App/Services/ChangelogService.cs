using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Markdig;

namespace Preflight.App.Services;

/// <summary>
/// Loads CHANGELOG.md (or CHANGELOG.uk.md) from embedded resources and
/// renders it to HTML via Markdig. Strips internal-only blocks before
/// rendering so the in-app modal shows only end-user-facing entries
/// while the source file (visible on GitHub, used for release notes)
/// retains full developer context.
/// </summary>
/// <remarks>
/// <para>
/// <b>Localization.</b> The class reads the current culture's two-letter
/// language tag and looks for <c>CHANGELOG.{lang}.md</c> first, then
/// falls back to <c>CHANGELOG.md</c>. Adding a new translation = adding
/// a new <c>CHANGELOG.uk.md</c>-style file alongside the EN canonical
/// and embedding it in the .csproj.
/// </para>
/// <para>
/// <b>Internal-only blocks.</b> Wrap any text between
/// <c>&lt;!-- internal:start --&gt;</c> and <c>&lt;!-- internal:end --&gt;</c>
/// HTML comments and it will be removed before Markdig parses the file.
/// Use this for chore/refactor/build/CI entries that matter to maintainers
/// but not to the user reading "What's new". The markers are still visible
/// on GitHub (rendered as comments - invisible there too) and in PR diffs,
/// preserving full audit trail.
/// </para>
/// <para>
/// <b>Caching.</b> The rendered HTML for a given culture is memoised on
/// first read. Switching language between modal opens triggers a fresh
/// load+parse for the new culture but keeps the EN cache for next switch
/// back.
/// </para>
/// </remarks>
public sealed class ChangelogService
{
    private const string FallbackResource = "CHANGELOG.md";

    /// <summary>
    /// Matches <c>&lt;!-- internal:start --&gt; ... &lt;!-- internal:end --&gt;</c>
    /// blocks across multiple lines. RegexOptions.Singleline lets <c>.</c>
    /// match newlines so the block can span any number of paragraphs.
    /// Non-greedy <c>.*?</c> ensures we don't accidentally swallow content
    /// between two unrelated start/end markers.
    /// </summary>
    private static readonly Regex InternalBlockRegex = new(
        @"<!--\s*internal:start\s*-->.*?<!--\s*internal:end\s*-->",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoLinks()
            .Build();

    private readonly Dictionary<string, string> _htmlCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _rawCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the rendered HTML for the current UI culture.</summary>
    public string GetHtml() => GetHtml(CultureInfo.CurrentUICulture);

    /// <summary>Returns the rendered HTML for the given culture.</summary>
    public string GetHtml(CultureInfo culture)
    {
        var key = ResourceKey(culture);
        if (_htmlCache.TryGetValue(key, out var cached)) return cached;
        var raw = LoadAndClean(key);
        var html = Markdown.ToHtml(raw, Pipeline);
        _htmlCache[key] = html;
        return html;
    }

    /// <summary>Returns the raw markdown (with internal blocks stripped).</summary>
    public string GetRaw() => GetRaw(CultureInfo.CurrentUICulture);

    /// <summary>Returns the raw markdown for the given culture.</summary>
    public string GetRaw(CultureInfo culture)
    {
        var key = ResourceKey(culture);
        if (_rawCache.TryGetValue(key, out var cached)) return cached;
        var raw = LoadAndClean(key);
        _rawCache[key] = raw;
        return raw;
    }

    private static string ResourceKey(CultureInfo culture)
    {
        // EN is the canonical fallback - its resource name has no language suffix.
        if (culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
            return FallbackResource;
        return $"CHANGELOG.{culture.TwoLetterISOLanguageName}.md";
    }

    private static string LoadAndClean(string preferredResource)
    {
        var raw = LoadResource(preferredResource) ?? LoadResource(FallbackResource);
        if (raw is null)
        {
            throw new InvalidOperationException(
                $"No CHANGELOG resource found in {typeof(ChangelogService).Assembly.FullName}. " +
                "Confirm <EmbeddedResource Include=\"..\\..\\CHANGELOG.md\" " +
                "LogicalName=\"CHANGELOG.md\" /> is present in Preflight.App.csproj.");
        }
        return InternalBlockRegex.Replace(raw, string.Empty).Trim();
    }

    private static string? LoadResource(string name)
    {
        var asm = typeof(ChangelogService).Assembly;
        using var stream = asm.GetManifestResourceStream(name);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
