using System.Security.Cryptography.X509Certificates;
using System.Text.Json;


namespace LoadTarot
{
    public class Program : Tarot
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            int numberOfCards = 3;
            TarotCard[] cardNumbers = CreateCards(numberOfCards);
            //Console.WriteLine(cardNumbers[cardNumbers.Length - 1]);

        }
    }
}
   
    
