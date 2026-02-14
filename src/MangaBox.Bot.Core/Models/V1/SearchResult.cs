namespace MangaBox.Bot.Core.Models.V1;

public class SearchResult
{
    public const string SOURCE_FALLBACK = "cba fallback";
    public const string SOURCE_GOOGLE = "google vision";
    public const string SOURCE_TITLE = "title lookup";
    public const string SOURCE_SAUCENAO = "sauce nao";

    [JsonPropertyName("match")]
    public MatchResult[] Match { get; set; } = [];

    [JsonPropertyName("vision")]
    public VisionResult[] Vision { get; set; } = [];

    [JsonPropertyName("textual")]
    public BaseResult[] Textual { get; set; } = [];

    [JsonPropertyName("bestGuess")]
    public Manga? BestGuess { get; set; }

    public record class Manga(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("nsfw")] bool Nsfw,
        [property: JsonPropertyName("cover")] string Cover,
        [property: JsonPropertyName("tags")] string[] Tags);

    public record class MetaData(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("type")] int Type,
        [property: JsonPropertyName("mangaId")] string MangaId,
        [property: JsonPropertyName("chapterId")] string? ChapterId,
        [property: JsonPropertyName("page")] int? Page);

    public class BaseResult
    {
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("exactMatch")]
        public bool ExactMatch { get; set; }

        [JsonPropertyName("manga")]
        public Manga? Manga { get; set; }
    }

    public class MatchResult : BaseResult
    {
        [JsonPropertyName("metadata")]
        public MetaData? MetaData { get; set; }
    }

    public class VisionResult : BaseResult
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("filteredTitle")]
        public string? FilteredTitle { get; set; }
    }
}
