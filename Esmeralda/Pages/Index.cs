using AskOpenAI;
using AskDeepSeek;
using LoadTarot;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;

namespace Esmeralda.Pages
{
    public partial  class Index
    {
        private string? imageSource;
        private string? card1 = "img/1-23.png";
        private string? card2 = "img/1-23.png";
        private string? card3 = "img/1-23.png";
        private string? card1name = "Past";
        private string? card2name = "Present";
        private string? card3name = "Future";
        private bool card1Inverted = false;
        private bool card2Inverted = false;
        private bool card3Inverted = false;
        private string? enteredText = null;
        private bool disabled = false;
        private bool isLoading = false;
        private MarkupString? esmeraldaResponse = null;

        private async Task HandleQuestionKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !disabled)
            {
                await LoadCards();
            }
        }

        private async Task LoadCards()
        {
            disabled = true;
            isLoading = true;
            StateHasChanged();
            await Task.Yield();

            try
            {
                int numberOfCards = 3;
                TarotCard[] cardNumbers = Tarot.CreateCards(numberOfCards);
                card1 = cardNumbers[0].ImageAsBase64;
                card1name = cardNumbers[0].DisplayName;
                card1Inverted = cardNumbers[0].Inverted;
                card2 = cardNumbers[1].ImageAsBase64;
                card2name = cardNumbers[1].DisplayName;
                card2Inverted = cardNumbers[1].Inverted;
                card3 = cardNumbers[2].ImageAsBase64;
                card3name = cardNumbers[2].DisplayName;
                card3Inverted = cardNumbers[2].Inverted;

               // var fortune = await ReadTheCards.TellFortune(cardNumbers, enteredText);
                var fortune = await ReadDeepSeek.TellFortune(cardNumbers, enteredText);
                var safeText = System.Net.WebUtility.HtmlEncode(fortune);
                var htmlText = Markdown.ToHtml(safeText);
                esmeraldaResponse = (MarkupString)htmlText;
            }
            finally
            {
                disabled = false;
                isLoading = false;
            }
        }
    }
}
