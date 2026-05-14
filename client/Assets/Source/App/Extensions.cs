using System;
using System.Linq;

namespace Source.App
{
    public static class Extensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" ", words.Select(w => char.ToUpper(w[0]) + w[1..]));
        }
    }
}