// TODO:
// 1. Добавить свойства для классификации фильма (жанр, возрастной рейтинг)
// 2. Реализовать проверку корректности данных (длительность, год выпуска)
// 3. Реализовать информативное строковое представление фильма

namespace Cinema
{
    public class Movie
    {
        public int Id { get; set; }                    // ID фильма
        public string Title { get; set; }              // Название
        public string Director { get; set; }           // Режиссер
        public int Year { get; set; }                  // Год выпуска
        public int Duration { get; set; }              // Длительность в минутах
        public string Description { get; set; }        // Описание
        public decimal BasePrice { get; set; }         // Базовая цена билета

        // Жанр (боевик, комедия, драма, фантастика, ужасы и т.д.) 
        public string Genre { get; set; }

        // Возрастной рейтинг (0+, 6+, 12+, 16+, 18+)
        public string AgeRating { get; set; }

        public Movie(int id, string title, string director, int year, int duration,
                    string description, decimal price, string genre, string ageRating)
        {
            Id = id;
            Title = title;
            Director = director;
            // Проверка корректности года выпуска
            int currentYear = System.DateTime.Now.Year;
            if (year < 1888) year = 1888;
            if (year > currentYear) year = currentYear;
            Year = year;
            // Проверка корректности длительности
            if (duration < 1) duration = 60;
            Duration = duration;

            Description = description;

          // Проверка корректности базовой цены
            if (price < 0) price = 100;
            BasePrice = price;

            Genre = string.IsNullOrWhiteSpace(genre) ? "прочее" : genre.Trim();
            AgeRating = string.IsNullOrWhiteSpace(ageRating) ? "0+" : ageRating.Trim();
        }

        public override string ToString()
        {
            return $"{Title} ({Genre}, {AgeRating}, {Year}) - Длительность: {Duration} мин.";
        }

        // Рассчитать цену с учетом формата сеанса
        public decimal CalculatePrice(string format)
        {
            decimal price = BasePrice;

            // Увеличить цену в зависимости от формата:
            // "2D" - базовая цена
            // "3D" - +30%
            // "IMAX" - +50%
            // "4DX" - +100%

            if (string.IsNullOrWhiteSpace(format)) return price;

            switch (format.Trim().ToUpperInvariant())
            {
                case "3D":
                    price *= 1.30m;
                    break;
                case "IMAX":
                    price *= 1.50m;
                    break;
                case "4DX":
                    price *= 2.00m;
                    break;
                    // 2D и все прочие форматы = базовая цена
            }

            // Округлим до целых рублей, как обычно в кассе
            return System.Math.Round(price, 0);
        }

        // Проверить подходит ли фильм по возрасту
        public bool IsAgeAppropriate(int viewerAge)
        {
            // Преобразовать возрастной рейтинг в число (0+, 6+, 12+, 16+, 18+)
            // Сравнить с возрастом зрителя
            int minAge = 0;
            if (!string.IsNullOrWhiteSpace(AgeRating))
            {
                string digits = AgeRating.Replace("+", "").Trim();
                if (int.TryParse(digits, out int parsed))
                    minAge = parsed;
            }

            return viewerAge >= minAge;
        }
    }
}