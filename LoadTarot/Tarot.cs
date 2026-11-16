namespace LoadTarot
{
    public class Tarot
    {

        public static TarotCard[] CreateCards(int _numberOfCards)
        {
            MajorArcana majorArcana = MajorArcana.LoadJson();
            int[] cardNumbers = new int[_numberOfCards];
            TarotCard[] tarotCards = new TarotCard[_numberOfCards];
            for (int i = 0; i < cardNumbers.Length; i++)
            {
                cardNumbers[i] = -1;
            }
            int j = 0;
            var random = new Random();
            int newRandom;
            for (j = 0; j < cardNumbers.Length; j++)
            {
                do
                {
                    newRandom = random.Next(0, 22);
                }
                while (Array.Exists(cardNumbers, x => x == newRandom));
                cardNumbers[j] = newRandom;
                string filePath = Path.Combine(AppContext.BaseDirectory, "data", majorArcana.items[newRandom].filename);
                var imageBytes = File.ReadAllBytes(filePath);
                var inverted = random.Next(2) == 1;
                // Reversed cards are rotated 180° in the browser via CSS, so we
                // always keep the original image bytes here.
                tarotCards[j] = new TarotCard(
                    newRandom,
                    majorArcana.items[newRandom].name,
                    inverted,
                    imageBytes,
                    majorArcana.items[newRandom].divination
                    );
                Console.WriteLine(" Card " + tarotCards[j].DisplayName);
            }

            return tarotCards;
        }
    }
}