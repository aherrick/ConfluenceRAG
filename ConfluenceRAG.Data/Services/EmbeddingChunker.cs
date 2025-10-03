using Microsoft.ML.Tokenizers;

namespace ConfluenceRAG.Data.Services;

public static class EmbeddingChunker
{
    private const int MaxTokens = 8192;

    // Returns chunks that already include the title prefix and stay within the model limit.
    public static List<(string content, int tokenCount)> ChunkWithTitle(
        string title,
        string body,
        string modelName
    )
    {
        var tokenizer = TiktokenTokenizer.CreateForModel(modelName);
        var prefix = string.IsNullOrWhiteSpace(title) ? string.Empty : $"{title} ";
        var prefixCount = tokenizer.EncodeToIds(prefix).Count;

        // 1-token cushion to avoid rare boundary merges pushing us over the limit.
        var bodyMax = Math.Max(1, MaxTokens - prefixCount - 1);

        var bodyTokens = tokenizer.EncodeToIds(body);
        var results = new List<(string content, int tokenCount)>();

        if (bodyTokens.Count <= bodyMax)
        {
            var finalText = prefix + body;
            results.Add((finalText, prefixCount + bodyTokens.Count));
            return results;
        }

        for (int i = 0; i < bodyTokens.Count; i += bodyMax)
        {
            int length = Math.Min(bodyMax, bodyTokens.Count - i);

            // Copy tokens for this slice
            var slice = new int[length];
            for (int j = 0; j < length; j++)
                slice[j] = bodyTokens[i + j];

            var bodyChunk = tokenizer.Decode(slice);
            var finalText = prefix + bodyChunk;

            results.Add((finalText, prefixCount + length));
        }

        return results;
    }
}