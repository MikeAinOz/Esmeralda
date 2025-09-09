using Azure;
using Azure.AI.Inference;
using LoadTarot;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Environment;


namespace AskDeepSeek
{
    public class ReadDeepSeek

    {
        public static async Task<string> TellFortune(TarotCard[] theseCards, string question)
        {
            var endpoint = GetEnvironmentVariable("AZURE_INFERENCE_SDK_ENDPOINT") ?? "https://mike-mfc82j16-australiaeast.services.ai.azure.com/models";
            var uri = new Uri(endpoint);

            var key = GetEnvironmentVariable("AZURE_INFERENCE_SDK_KEY") ?? "YOUR_KEY_HERE";
            AzureKeyCredential credential = new AzureKeyCredential(key);
            AzureAIInferenceClientOptions clientOptions = new AzureAIInferenceClientOptions();

            var deploymentName = "DeepSeek-R1";
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
            var client = new ChatCompletionsClient(uri, credential, clientOptions);

            var requestOptions = new ChatCompletionsOptions()
            {
                Messages = {
                    new ChatRequestSystemMessage(instructions),
                    new ChatRequestUserMessage(userMessage)
                },
                MaxTokens = 2048,
                Model = deploymentName
            };

            Response<ChatCompletions> response = client.Complete(requestOptions);
            return Regex.Replace(response.Value.Content, @"<think>.*?</think>", string.Empty, RegexOptions.Singleline);
        }
    }
}
