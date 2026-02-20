// TODO:
// 1. Реализовать учет данных посетителя
// 2. Реализовать покупку билетов и историю посещений
// 3. Реализовать систему скидок и бонусов

using System;
using System.Collections.Generic;

namespace Cinema
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Тип посетителя (обычный, студент, пенсионер, ребенок)
        public string CustomerType { get; set; } = "обычный";

        private List<Ticket> purchasedTickets = new List<Ticket>();
        private decimal bonusPoints = 0; // Бонусные баллы
        private List<string> preferredGenres = new List<string>(); // Предпочитаемые жанры

        public IReadOnlyList<Ticket> PurchasedTickets => purchasedTickets.AsReadOnly();

        public int GetAge()
        {
            int age = DateTime.Now.Year - BirthDate.Year;
            if (BirthDate.Date > DateTime.Now.Date.AddYears(-age)) age--;
            return age;
        }

        // TODO 2: Купить билет
        public Ticket BuyTicket(Session session, int row, int seat)
        {
            if (session == null) return null;

            int age = GetAge();
            if (!session.Movie.IsAgeAppropriate(age))
            {
                Console.WriteLine($"Фильм имеет возрастное ограничение {session.Movie.AgeRating}. Вам нельзя покупать билет.");
                return null;
            }

            if (!session.HasAvailableSeats())
            {
                Console.WriteLine("На этот сеанс нет свободных мест.");
                return null;
            }

            Ticket ticket = session.SellTicket(row, seat, this);
            if (ticket == null)
            {
                Console.WriteLine("Не удалось купить билет: место занято или введены неверные координаты.");
                return null;
            }


            decimal discount = CalculateDiscount();
            decimal priceAfterDiscount = ticket.Price * (1 - discount);

           
            decimal usedBonus = UseBonusPoints(priceAfterDiscount);
            decimal finalPrice = priceAfterDiscount - usedBonus;
            if (finalPrice < 0) finalPrice = 0;
            ticket.Price = Math.Round(finalPrice, 0);
            ticket.PurchaseTime = DateTime.Now;

            purchasedTickets.Add(ticket);


            bonusPoints += Math.Round(ticket.Price * 0.05m, 0);


            AddPreferredGenre(session.Movie.Genre);
            return ticket;
        }

        // TODO 2: Вернуть билет
        public bool ReturnTicket(Ticket ticket)
        {

            if (ticket == null) return false;

            int idx = purchasedTickets.FindIndex(t => t.Id == ticket.Id);
            if (idx < 0) return false;

            ticket.Session?.FreeSeat(ticket.Row, ticket.Seat);

            purchasedTickets.RemoveAt(idx);


            
            decimal earned = Math.Round(ticket.Price * 0.05m, 0);
            bonusPoints = Math.Max(0, bonusPoints - earned);

            return true;
        }

        // TODO 3: Рассчитать скидку
        public decimal CalculateDiscount()
        {
            decimal discount = 0;
            string type = (CustomerType ?? "обычный").Trim().ToLowerInvariant();
            switch (type)
            {
                case "студент":
                    discount = 0.20m;
                    break;
                case "пенсионер":
                    discount = 0.30m;
                    break;
                case "ребенок":
                case "ребёнок":
                    discount = 0.25m;
                    break;
                default:
                    discount = 0m;
                    break;
            }



            if (purchasedTickets.Count >= 10)
                discount = Math.Min(0.35m, discount + 0.05m);

            return discount;
        }

        // TODO 3: Использовать бонусные баллы
        public decimal UseBonusPoints(decimal ticketPrice)
        {
            
            
            decimal maxBonus = ticketPrice * 0.5m;
            decimal usedBonus = Math.Min(bonusPoints, maxBonus);
            bonusPoints -= usedBonus;
            return usedBonus;
        }

        // TODO 1: Добавить предпочитаемый жанр
        public void AddPreferredGenre(string genre)
        {

            if (string.IsNullOrWhiteSpace(genre)) return;
            genre = genre.Trim().ToLowerInvariant();
            if (!preferredGenres.Contains(genre))
                preferredGenres.Add(genre);
        }

        // TODO 3: Получить рекомендации по фильмам
        public List<Movie> GetMovieRecommendations(List<Movie> allMovies)
        {
            List<Movie> recommendations = new List<Movie>();

            if (allMovies == null || allMovies.Count == 0) return recommendations;

            HashSet<int> watchedMovieIds = new HashSet<int>();
            foreach (var t in purchasedTickets)
            {
                if (t?.Session?.Movie != null)
                    watchedMovieIds.Add(t.Session.Movie.Id);
            }

            foreach (var m in allMovies)
            {
                if (m == null) continue;
                if (watchedMovieIds.Contains(m.Id)) continue;

                if (preferredGenres.Count == 0)
                {
                    recommendations.Add(m);
                    continue;
                }

                string g = (m.Genre ?? "").Trim().ToLowerInvariant();
                if (preferredGenres.Contains(g))
                    recommendations.Add(m);
            }

            // Сначала новинки
            recommendations.Sort((a, b) => b.Year.CompareTo(a.Year));
            return recommendations;
        }

        // Показать информацию о посетителе
        public void ShowCustomerInfo()
        {
            Console.WriteLine($"Посетитель: {FullName}");
            Console.WriteLine($"Дата рождения: {BirthDate:dd.MM.yyyy}");
            Console.WriteLine($"Телефон: {Phone}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Тип: {CustomerType}");
            Console.WriteLine($"Зарегистрирован: {RegistrationDate:dd.MM.yyyy}");
            Console.WriteLine($"Куплено билетов: {purchasedTickets.Count}");
            Console.WriteLine($"Бонусных баллов: {bonusPoints}");
            Console.WriteLine($"Предпочитаемые жанры: {string.Join(", ", preferredGenres)}");
        }
    }
}