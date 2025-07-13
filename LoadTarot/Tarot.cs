using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace LoadTarot
{
    public class Tarot
    {

        public static TarotCard[] CreateCards(int _numberOfCards)
        {
            MajorArcana majorArcana = MajorArcana.LoadJson();
            var imageService = new ImageService();
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
                byte[]? cardImage;
                if (inverted)
                {
                    using (var image = Image.Load(imageBytes))
                    {
                        image.Mutate(x => x.Rotate(180));

                        // Convert back to bytes
                        using (var outputStream = new MemoryStream())
                        {
                            image.SaveAsPng(outputStream); // or SaveAsJpeg, etc.
                            cardImage = outputStream.ToArray();

                            
                        }
                    }
                } else
                {
                    cardImage = imageBytes;
                }
                //    var cardImage = inverted ? imageService.(imageBytes) : imageBytes;
                tarotCards[j] = new TarotCard(
                    newRandom,
                    majorArcana.items[newRandom].name,
                    inverted,
                    cardImage,
                    majorArcana.items[newRandom].divination
                    );
                Console.WriteLine(" Card " + tarotCards[j].DisplayName);
            }

            return tarotCards;
        }
    }
}