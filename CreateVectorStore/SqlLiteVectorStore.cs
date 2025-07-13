using Microsoft.Data.Sqlite;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Data;
using System.Text;
using System.Text.Json;

public class SqliteVectorStore
{
    private readonly string _connectionString;
    private readonly OpenAIClient _openAIClient;
    private const string EmbeddingModel = "text-embedding-3-small";
    
    public SqliteVectorStore(string dbPath, string openAIKey)
    {
        _connectionString = $"Data Source={dbPath}";
        _openAIClient = new OpenAIClient(openAIKey);
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS document_chunks (
                id TEXT PRIMARY KEY,
                content TEXT NOT NULL,
                embedding TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
            
            CREATE INDEX IF NOT EXISTS idx_chunks_id ON document_chunks(id);";
        command.ExecuteNonQuery();
    }
    
    public async Task LoadDocumentAsync(string documentPath, bool clearExisting = true)
    {
        if (clearExisting)
        {
            await ClearAllChunksAsync();
        }
        
        Console.WriteLine("Reading document...");
        var text = await File.ReadAllTextAsync(documentPath);
        
        Console.WriteLine("Splitting into chunks...");
        var chunks = SplitIntoChunks(text, 1000, 200); // 1000 chars with 200 char overlap
        
        Console.WriteLine($"Processing {chunks.Count} chunks...");
        
        for (int i = 0; i < chunks.Count; i++)
        {
            Console.WriteLine($"Processing chunk {i + 1}/{chunks.Count}");
            
            var embedding = await GenerateEmbeddingAsync(chunks[i]);
            await StoreChunkAsync($"chunk_{i}", chunks[i], embedding);
            
            // Add small delay to avoid rate limiting
            await Task.Delay(100);
        }
        
        Console.WriteLine("Document loaded successfully!");
    }
    
    public async Task<List<(string Content, float Score)>> SearchAsync(string query, int topK = 5)
    {
        var queryEmbedding = await GenerateEmbeddingAsync(query);
        var allChunks = await GetAllChunksAsync();
        
        var results = allChunks
            .Select(chunk => new
            {
                Content = chunk.Content,
                Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => (x.Content, x.Score))
            .ToList();
            
        return results;
    }
    
    private async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        //var response = await _openAIClient.GetEmbeddingsAsync(
        //    EmbeddingModel, 
        //    new[] { text });
        var embedClient = _openAIClient.GetEmbeddingClient(EmbeddingModel);
        OpenAIEmbedding embedding = await embedClient.GenerateEmbeddingAsync(text);
        return embedding.ToFloats().ToArray() ;
    }
    
    private async Task StoreChunkAsync(string id, string content, ReadOnlyMemory<float> embedding)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR REPLACE INTO document_chunks (id, content, embedding) VALUES (@id, @content, @embedding)";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@content", content);
        command.Parameters.AddWithValue("@embedding", JsonSerializer.Serialize(embedding));
        
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task<List<(string Content, float[] Embedding)>> GetAllChunksAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT content, embedding FROM document_chunks";
        
        var chunks = new List<(string, float[])>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var content = reader.GetString(0);
            var embeddingJson = reader.GetString(1);
            var embedding = JsonSerializer.Deserialize<float[]>(embeddingJson);
            chunks.Add((content, embedding));
        }
        
        return chunks;
    }
    
    private async Task ClearAllChunksAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM document_chunks";
        await command.ExecuteNonQueryAsync();
    }
    
    private List<string> SplitIntoChunks(string text, int chunkSize, int overlap = 0)
    {
        var chunks = new List<string>();
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        
        var currentChunk = new StringBuilder();
        var currentLength = 0;
        
        foreach (var sentence in sentences)
        {
            var trimmedSentence = sentence.Trim();
            if (string.IsNullOrEmpty(trimmedSentence)) continue;
            
            var sentenceWithPunctuation = trimmedSentence + ".";
            
            if (currentLength + sentenceWithPunctuation.Length > chunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                
                // Handle overlap
                if (overlap > 0 && currentChunk.Length > overlap)
                {
                    var overlapText = currentChunk.ToString().Substring(currentChunk.Length - overlap);
                    currentChunk = new StringBuilder(overlapText);
                    currentLength = overlapText.Length;
                }
                else
                {
                    currentChunk.Clear();
                    currentLength = 0;
                }
            }
            
            currentChunk.Append(sentenceWithPunctuation + " ");
            currentLength += sentenceWithPunctuation.Length + 1;
        }
        
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }
        
        return chunks;
    }
    
    private float CosineSimilarity(float[] a, float[] b)
    {
        var dotProduct = a.Zip(b, (x, y) => x * y).Sum();
        var magnitudeA = Math.Sqrt(a.Sum(x => x * x));
        var magnitudeB = Math.Sqrt(b.Sum(x => x * x));
        return (float)(dotProduct / (magnitudeA * magnitudeB));
    }
    
    public async Task<int> GetChunkCountAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM document_chunks";
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}

// RAG Service that uses the vector store
public class RagService
{
    private readonly SqliteVectorStore _vectorStore;
    private readonly OpenAIClient _openAIClient;
    private readonly string _instructions =
        """
        You are mystical fortune teller using the Tarot Major Arcana 
        to interpret the past, present, and future. The question provides these cards.
        Each card provided will be explained in a way that reflects how its symbolism applies to the user's 
        question.It focuses on guiding the user through the cards' meanings in a thoughtful, mystical, 
        and engaging way, while also keeping a sense of wisdom and positive affirmation. 
        Positive insights are emphasized, ensuring that even challenging cards are presented with an 
        optimistic perspective to encourage growth and understanding. The tone is mystical yet 
        always offers an uplifting message, turning insights into affirmations for personal growth.
        Occasionally, a card may appear inverted (reversed), revealing a deeper, more nuanced meaning. 
        """;


    public RagService(string dbPath, string openAIKey)
    {
        _vectorStore = new SqliteVectorStore(dbPath, openAIKey);
        _openAIClient = new OpenAIClient(openAIKey);
    }
    
    public async Task InitializeFromDocumentAsync(string documentPath)
    {
        await _vectorStore.LoadDocumentAsync(documentPath);
    }
    
    public async Task<string> AskQuestionAsync(string question)
    {
        // Get relevant chunks
        var relevantChunks = await _vectorStore.SearchAsync(question, 5);
        
        if (!relevantChunks.Any())
        {
            return "I couldn't find relevant information to answer your question.";
        }
        
        var context = string.Join("\n\n", relevantChunks.Select(c => c.Content));
        
        // Generate answer using context
        var prompt = $@"
Instructions:
{_instructions}

Context:
{context}
";

        var chatClient = _openAIClient.GetChatClient("gpt-4o-mini");
        ChatCompletionOptions options = new ChatCompletionOptions();
        options.Temperature = (float?)0.7;
        options.MaxOutputTokenCount = 500;
        
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt),
            new UserChatMessage(question)
        };
        
        var response = await chatClient.CompleteChatAsync(messages, options);

        return response.Value.Content.ToString();
    }
    
    public async Task<int> GetIndexedChunkCountAsync()
    {
        return await _vectorStore.GetChunkCountAsync();
    }
}