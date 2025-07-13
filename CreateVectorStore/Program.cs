using System.Threading.Tasks;
using LoadTarot;

namespace CreateVectorStore
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, Seeker!");
            string OpenAI_Api_Key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            string filePath = Path.Combine(AppContext.BaseDirectory, "data", "IllustratedKeyToTheTarot.txt");
            string dbPath = Path.Combine(AppContext.BaseDirectory, "data", "TarotVector.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            SqliteVectorStore thisDb = new SqliteVectorStore(dbPath,OpenAI_Api_Key);
            Console.WriteLine("Database Ready");
            //await thisDb.LoadDocumentAsync(filePath);
            //Console.WriteLine("Document Loaded");
            int numberOfCards = 3;
            
            var theseCards = Tarot.CreateCards(numberOfCards);
            string cards = $@"My cards are: past {theseCards[0].DisplayName}, present {theseCards[1].DisplayName}, and future {theseCards[2].DisplayName}";
            Console.Write("Ask a question: ");
            string question = Console.ReadLine();
            string thisQuestion = cards + " " + question;
            var cardresult = await thisDb.SearchAsync(theseCards[0].Name, 5);
            RagService thisRag = new RagService(dbPath, OpenAI_Api_Key);
            var thisAnswer = await thisRag.AskQuestionAsync(thisQuestion);
          
        }
    }
}
