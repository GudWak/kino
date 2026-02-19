// TODO:
// 1. Регистрация новых посетителей
// 2. Управление расписанием сеансов
// 3. Учет продаж и популярности фильмов

using System;
using System.Collections.Generic;

namespace Cinema
{
    public class CinemaManager
    {
        private List<Customer> customers = new List<Customer>();
        private List<Movie> movies = new List<Movie>();
        private List<Session> sessions = new List<Session>();

        private int nextCustomerId = 1000;
        private int nextSessionId = 1;
        private int nextTicketId = 10000;
        private decimal dailyRevenue = 0;

        // TODO 1: Зарегистрировать нового посетителя
        public Customer RegisterCustomer(string fullName, DateTime birthDate, string phone, string email, string customerType)
        {
            // Простейшая защита от дубликатов по телефону
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var exists = FindCustomerByPhone(phone);
                if (exists != null)
                    return exists;
            }

            Customer customer = new Customer
            {
                Id = nextCustomerId,
                FullName = fullName ?? string.Empty,
                BirthDate = birthDate,
                Phone = phone ?? string.Empty,
                Email = email ?? string.Empty,
                RegistrationDate = DateTime.Today,
                CustomerType = string.IsNullOrWhiteSpace(customerType) ? "обычный" : customerType.Trim().ToLowerInvariant()
            };

            customers.Add(customer);
            nextCustomerId++;
            return customer;
        }

        // TODO 2: Найти посетителя по телефону
        public Customer FindCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            string normalized = phone.Trim();

            foreach (var c in customers)
            {
                if (c == null) continue;
                if (string.Equals(c.Phone?.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return null;
        }

        // TODO 2: Создать сеанс
        public Session CreateSession(Movie movie, DateTime startTime, string hall, string format)
        {
            if (movie == null) return null;

            Session session = new Session(nextSessionId, movie, startTime, hall, format);
            sessions.Add(session);
            nextSessionId++;
            return session;
        }

        // TODO 3: Продать билет
        public Ticket SellTicket(Customer customer, Session session, int row, int seat)
        {
            if (customer == null || session == null) return null;

            // Купить билет через customer.BuyTicket()
            Ticket ticket = customer.BuyTicket(session, row, seat);
            if (ticket == null) return null;

            // Зафиксировать продажу
            dailyRevenue += ticket.Price;
            return ticket;
        }

        // TODO 3: Найти сеансы по фильму
        public List<Session> FindSessionsByMovie(Movie movie)
        {
            List<Session> result = new List<Session>();

            if (movie == null) return result;

            foreach (var s in sessions)
            {
                if (s?.Movie == null) continue;
                if (s.Movie.Id == movie.Id)
                    result.Add(s);
            }
            return result;
        }

        // TODO 3: Найти ближайшие сеансы
        public List<Session> FindUpcomingSessions(DateTime fromTime, int hoursAhead = 24)
        {
            List<Session> result = new List<Session>();

            DateTime toTime = fromTime.AddHours(hoursAhead);
            foreach (var s in sessions)
            {
                if (s == null) continue;
                if (s.StartTime >= fromTime && s.StartTime <= toTime)
                    result.Add(s);
            }

            result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return result;
        }

        // TODO 3: Получить статистику по фильмам
        public Dictionary<Movie, int> GetMoviePopularity()
        {
            Dictionary<Movie, int> popularity = new Dictionary<Movie, int>();

            // Подсчитаем популярность по данным покупателей (список билетов у клиентов).
            // Это подходит для учебного проекта и не требует доступа к приватным полям сеанса.
            foreach (var c in customers)
            {
                if (c == null) continue;
                foreach (var t in c.PurchasedTickets)
                {
                    var m = t?.Session?.Movie;
                    if (m == null) continue;

                    if (!popularity.ContainsKey(m))
                        popularity[m] = 0;
                    popularity[m]++;
                }
            }
            return popularity;
        }

        // Готовые методы:
        public void AddMovie(Movie movie)
        {
            movies.Add(movie);
        }

        public List<Movie> GetAllMovies()
        {
            return movies;
        }

        public List<Session> GetAllSessions()
        {
            return sessions;
        }

        public List<Customer> GetAllCustomers()
        {
            return customers;
        }

        public decimal GetDailyRevenue()
        {
            return dailyRevenue;
        }

        public int GetNextTicketId()
        {
            return nextTicketId++;
        }

        public int GetCustomerCount()
        {
            return customers.Count;
        }
    }
}