using LoadTarot;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskOpenAI
{
    public class ReadTheCards
    {

        public static async Task<string> TellFortune(TarotCard[] theseCards, string question)
        {
            ChatClient client = new ChatClient(model: "gpt-4o", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            string instructions =
                """
                You are mystical fortune teller using the Tarot Major Arcana 
                to interpret the past, present, and future. The user provides these cards and a question.
                Each card provided will be explained in a way that reflects how its symbolism applies to the user's 
                question.It focuses on guiding the user through the cards' meanings in a thoughtful, mystical, 
                and engaging way, while also keeping a sense of wisdom and positive affirmation. 
                Positive insights are emphasized, ensuring that even challenging cards are presented with an 
                optimistic perspective to encourage growth and understanding. The tone is mystical yet 
                always offers an uplifting message, turning insights into affirmations for personal growth.
                A card may appear reversed, revealing a deeper, more nuanced meaning. 
                """
                ;
            string userMessage = $@"""
                My cards are: past {theseCards[0].DisplayName}, 
                              present {theseCards[1].DisplayName}, 
                              and future {theseCards[2].DisplayName}.
                The question is: {question}
            """;


            List<ChatMessage> messages = new List<ChatMessage>()
            {
                new SystemChatMessage(instructions),
                new UserChatMessage(userMessage)
            };
            var result = await client.CompleteChatAsync(messages);

            //messages.Add(new AssistantChatMessage(result));
            //messages.Add(new UserChatMessage("Who was the first person to be awarded one?"));

            //result = await client.CompleteChatAsync(messages);

            //messages.Add(new AssistantChatMessage(result));

            //foreach (ChatMessage message in messages)
            //{
            //    string role = message.GetType().Name;
            //    string text = message.Content[0].Text;

            //    Console.WriteLine($"{role}: {text}");
            //}
            return result.Value.Content[0].Text;
        }
    }
}
