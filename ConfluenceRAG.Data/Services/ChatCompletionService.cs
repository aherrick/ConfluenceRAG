using System.Text;
using Azure;
using Azure.AI.OpenAI;
using ConfluenceRAG.Data.Models.Config;
using OpenAI.Chat;

namespace ConfluenceRAG.Data.Services;

public class ChatCompletionService
{
    private readonly ChatClient _chatClient;

    public ChatCompletionService(AzureOpenAIConfig config)
    {
        var openAIClient = new AzureOpenAIClient(
            new Uri(config.Endpoint),
            new AzureKeyCredential(config.ApiKey)
        );

        _chatClient = openAIClient.GetChatClient(config.ChatDeployment);
    }

    public async Task GetAnswerWithContextAsync(
        string question,
        IEnumerable<(string Title, string Content)> ragResults
    )
    {
        // Build context from RAG results
        var context = new StringBuilder();
        foreach (var (Title, Content) in ragResults)
        {
            context.AppendLine($"## {Title}");
            context.AppendLine(Content);
            context.AppendLine();
        }

        // Messages
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a helpful assistant. "
                    + "Answer questions based only on the provided context. "
                    + "Respond simply and concisely. "
                    + "If you don't know, say 'I don't know.'"
            ),
            new UserChatMessage($"Context:\n{context}\n\nQuestion: {question}"),
        };

        // Stream the response
        await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages))
        {
            foreach (var content in update.ContentUpdate)
            {
                Console.Write(content.Text); // print as it streams
            }
        }

        Console.WriteLine(); // newline at the end
    }
}