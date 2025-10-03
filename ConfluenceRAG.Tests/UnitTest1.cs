using ConfluenceRAG.Data.Services;

namespace ConfluenceRAG.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void ChunkingLogic_WithLargeStory_Creates3Chunks()
    {
        // Arrange
        const string modelName = "text-embedding-3-small";
        const string title = "Large Document Test";

        // A story with between 16,500-20,000 tokens
        var bodyText = GetLargeStoryText();

        // Act
        var chunks = EmbeddingChunker.ChunkWithTitle(title, bodyText, modelName);

        // Assert
        Assert.That(chunks, Has.Count.EqualTo(3), "Should create exactly 3 chunks");

        // Verify each chunk is within the model limit (8192 tokens)
        foreach (var (content, tokenCount) in chunks)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    tokenCount,
                    Is.LessThanOrEqualTo(8192),
                    $"Chunk token count ({tokenCount}) should not exceed model limit"
                );
                Assert.That(
                    content,
                    Does.StartWith(title),
                    "Each chunk should start with the title prefix"
                );
            });
        }

        // Print chunk information for debugging
        for (int i = 0; i < chunks.Count; i++)
        {
            Console.WriteLine(
                $"Chunk {i + 1}: {chunks[i].tokenCount} tokens, Content length: {chunks[i].content.Length} characters"
            );
        }
    }

    private static string GetLargeStoryText()
    {
        // Create a story that repeats content to reach approximately 16,500-20,000 tokens
        var baseStory =
            @"In the heart of the vast digital landscape, where data flows like rivers through interconnected networks, there exists a remarkable story of innovation and discovery that spans decades of technological advancement and human ingenuity. This tale begins in a bustling metropolis where technology companies compete fiercely for market dominance, each striving to create the next breakthrough that will revolutionize how we interact with information, process data, and understand the complex relationships between different forms of digital content.

The protagonist of our story is Dr. Sarah Chen, a brilliant computer scientist who has dedicated her career to developing advanced artificial intelligence systems that can comprehend, analyze, and generate human-like responses with unprecedented accuracy and sophistication. Her latest project involves creating a sophisticated natural language processing system that represents the culmination of years of research and development in machine learning algorithms, neural network architectures, computational linguistics, and cognitive science principles.

Dr. Chen's international team consists of talented engineers, researchers, data scientists, linguists, and domain experts from universities and research institutions around the world. Each member brings unique expertise, cultural perspectives, and innovative approaches to the project, creating a collaborative environment where breakthrough innovations thrive through diverse thinking, rigorous experimentation, and continuous knowledge sharing across disciplinary boundaries.

The development process is fraught with numerous technical challenges and obstacles that test the team's resilience, creativity, and problem-solving capabilities on a daily basis. They encounter complex issues with data quality assurance, model bias detection and mitigation, computational efficiency optimization, scalability requirements, memory management, distributed processing coordination, and real-time performance constraints that demand innovative solutions and careful architectural decisions.

As the project progresses through multiple phases of research and development, Dr. Chen and her dedicated team make significant breakthroughs in understanding how advanced language models can be trained, fine-tuned, and optimized to better comprehend contextual nuances, emotional sentiment, cultural implications, domain-specific terminology, and complex linguistic patterns that characterize human communication across different languages, cultures, and professional domains.

The research involves extensive experimentation with cutting-edge neural network architectures, including transformer models with attention mechanisms, convolutional networks for pattern recognition, recurrent structures for sequential processing, graph neural networks for relationship modeling, and hybrid architectures that combine multiple approaches to achieve superior performance across diverse natural language understanding and generation tasks.

Throughout the comprehensive development process, the team faces important ethical considerations and must implement responsible artificial intelligence practices that ensure fairness, transparency, accountability, and societal benefit. They establish rigorous testing procedures to identify and mitigate potential algorithmic biases, implement robust privacy protection mechanisms, develop explainability frameworks, and create governance structures that promote responsible deployment and monitoring of AI systems.

The technical architecture of the Cognitive Reasoning Engine involves multiple interconnected components working in sophisticated harmony to process, understand, and generate human language. The system includes specialized modules for advanced text preprocessing, tokenization algorithms, contextual embedding generation, multi-head attention computation, transformer layer processing, response synthesis, quality assessment, and continuous learning mechanisms that improve performance over time.

Dr. Chen's team employs state-of-the-art optimization techniques to dramatically improve the system's performance across multiple dimensions including accuracy, speed, memory efficiency, and resource utilization. They implement advanced gradient descent algorithms, sophisticated backpropagation methods, regularization strategies to prevent overfitting, learning rate scheduling, batch normalization techniques, and distributed training approaches that leverage massive computational resources effectively.";

        // Repeat the story multiple times to reach the target token count
        var repeatedStory = new System.Text.StringBuilder();
        for (int i = 0; i < 25; i++) // Increase to 25 times to get sufficient tokens for 3 chunks
        {
            repeatedStory.AppendLine(baseStory);
            repeatedStory.AppendLine();

            // Add some variation to make it more realistic
            repeatedStory.AppendLine(
                $"Chapter {i + 1}: Advanced Research and Development Continues"
            );
            repeatedStory.AppendLine(
                "The research team continues their groundbreaking work in artificial intelligence, pushing the boundaries of what is possible with modern technology. They explore new methodologies, implement innovative algorithms, and collaborate with researchers from around the world to advance the field of natural language processing and machine learning."
            );
            repeatedStory.AppendLine();
        }

        return repeatedStory.ToString();
    }
}