using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTarot
{
    public record TarotCard
    {
        public int Number { get; init; }
        public string Name { get; init; }
        public bool Inverted { get; init; }
        public byte[] Image { get; init; }
        public string Divination { get; init; }

        public TarotCard(int number, string name, bool inverted, byte[] image, string divination)
        {
            if (number < 0 || number > 22)
                throw new ArgumentOutOfRangeException(nameof(number));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Image = image ?? throw new ArgumentNullException(nameof(image));

            Number = number;
            Inverted = inverted;
            Divination = divination;
        }

        public string DisplayName => Inverted ? $"{Name} (Reversed)" : Name;

        public string ImageAsBase64 => Image != null
            ? $"data:image/png;base64,{Convert.ToBase64String(Image)}"
            : string.Empty;
    }

}
