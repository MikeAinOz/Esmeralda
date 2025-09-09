using OpenAI.Chat;
using LoadTarot;
using AskOpenAI;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, Chatterbox!");

        // await TestOpenAiApi();
        int numberOfCards = 3;
        TarotCard[] cardSelection = Tarot.CreateCards(numberOfCards);
        var question = "What does my day hold for me";
        var fortune = await ReadTheCards.TellFortune(cardSelection, question);
        Console.WriteLine(fortune);

        static async Task TestOpenAiApi()
        {
            ChatClient client = new ChatClient(model: "gpt-4o", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            List<ChatMessage> messages = new List<ChatMessage>()
            {
                new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage("When was the Nobel Prize founded?")
            };

            var result = await client.CompleteChatAsync(messages);

            messages.Add(new AssistantChatMessage(result));
            messages.Add(new UserChatMessage("Who was the first person to be awarded one?"));

            result = await client.CompleteChatAsync(messages);

            messages.Add(new AssistantChatMessage(result));

            foreach (ChatMessage message in messages)
            {
                string role = message.GetType().Name;
                string text = message.Content[0].Text;

                Console.WriteLine($"{role}: {text}");
            }
        }
    }
}