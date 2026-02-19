// TODO:
// 1. Реализовать просмотр афиши и расписания
// 2. Реализовать покупку билетов на сеансы
// 3. Реализовать рекомендации фильмов и статистику

using System;
using System.Collections.Generic;

namespace Cinema
{
    public class CinemaMenu
    {
        private CinemaManager manager;

        public CinemaMenu()
        {
            manager = new CinemaManager();
            InitializeCinemaData();
        }

        private void InitializeCinemaData()
        {
            // Инициализация тестовых данных - фильмы
            Movie movie1 = new Movie(1, "Аватар: Путь воды", "Джеймс Кэмерон",
                2022, 192, "Продолжение истории об аватарах Пандоры",
                400, "фантастика", "12+");
            Movie movie2 = new Movie(2, "Оппенгеймер", "Кристофер Нолан",
                2023, 180, "История создания атомной бомбы",
                350, "драма", "16+");
            Movie movie3 = new Movie(3, "Чебурашка", "Дмитрий Дьяченко",
                2022, 113, "Современная экранизация советского мультфильма",
                300, "комедия", "6+");

            manager.AddMovie(movie1);
            manager.AddMovie(movie2);
            manager.AddMovie(movie3);

            // Создание тестовых сеансов
            DateTime today = DateTime.Today;
            manager.CreateSession(movie1, today.AddHours(12).AddMinutes(30), "Зал 1", "3D");
            manager.CreateSession(movie1, today.AddHours(16), "Зал 2", "IMAX");
            manager.CreateSession(movie2, today.AddHours(14), "Зал 3", "2D");
            manager.CreateSession(movie3, today.AddHours(11), "Зал 1", "2D");
        }

        // TODO 1: Показать афишу фильмов
        public void ShowMovies()
        {
            Console.WriteLine("=== АФИША ФИЛЬМОВ ===");

            var movies = manager.GetAllMovies();
            if (movies.Count == 0)
            {
                Console.WriteLine("Фильмов нет.");
                return;
            }

            // Группировка по жанру
            var byGenre = new Dictionary<string, List<Movie>>();
            foreach (var m in movies)
            {
                string g = (m.Genre ?? "прочее").Trim().ToLowerInvariant();
                if (!byGenre.ContainsKey(g))
                    byGenre[g] = new List<Movie>();
                byGenre[g].Add(m);
            }

            foreach (var kv in byGenre)
            {
                Console.WriteLine($"\nЖанр: {kv.Key}");
                foreach (var m in kv.Value)
                {
                    Console.WriteLine($"  {m.Id}. {m}");
                    Console.WriteLine($"     Режиссёр: {m.Director}. Цена от: {m.BasePrice} руб.");
                    Console.WriteLine($"     Описание: {m.Description}");
                }
            }
        }

        // TODO 1: Показать расписание сеансов
        public void ShowSchedule()
        {
            Console.WriteLine("=== РАСПИСАНИЕ СЕАНСОВ ===");

            var sessions = manager.FindUpcomingSessions(DateTime.Now, 72);
            if (sessions.Count == 0)
            {
                Console.WriteLine("Ближайших сеансов нет.");
                return;
            }

            DateTime? currentDate = null;
            foreach (var s in sessions)
            {
                var date = s.StartTime.Date;
                if (currentDate == null || currentDate.Value != date)
                {
                    currentDate = date;
                    Console.WriteLine($"\n--- {date:dd.MM.yyyy} ---");
                }
                Console.WriteLine($"#{s.Id}: {s.StartTime:HH:mm} | {s.Movie.Title} | {s.Hall} | {s.Format} | Цена: {s.TicketPrice} | Свободно: {s.GetAvailableSeatsCount()}");
            }
        }

        // TODO 2: Купить билет
        public void BuyTicket()
        {
            Console.WriteLine("=== ПОКУПКА БИЛЕТА ===");

            // 1. Поиск/регистрация посетителя
            Console.Write("Телефон: ");
            string phone = Console.ReadLine();
            Customer customer = manager.FindCustomerByPhone(phone);
            if (customer == null)
            {
                Console.WriteLine("Посетитель не найден. Регистрируем нового.");
                Console.Write("ФИО: ");
                string name = Console.ReadLine();
                Console.Write("Дата рождения (дд.мм.гггг): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime birthDate))
                {
                    Console.WriteLine("Некорректная дата.");
                    return;
                }
                Console.Write("Email: ");
                string email = Console.ReadLine();

                int age = DateTime.Now.Year - birthDate.Year;
                if (birthDate.Date > DateTime.Now.Date.AddYears(-age)) age--;
                string type = age < 18 ? "ребенок" : age > 60 ? "пенсионер" : "обычный";
                customer = manager.RegisterCustomer(name, birthDate, phone, email, type);
            }

            // 2. Расписание
            ShowSchedule();
            Console.Write("\nВведите ID сеанса: ");
            if (!int.TryParse(Console.ReadLine(), out int sessionId))
            {
                Console.WriteLine("Неверный ID.");
                return;
            }

            Session session = null;
            foreach (var s in manager.GetAllSessions())
            {
                if (s.Id == sessionId) { session = s; break; }
            }
            if (session == null)
            {
                Console.WriteLine("Сеанс не найден.");
                return;
            }

            // 3. Схема зала
            session.ShowHallLayout();

            Console.Write("Ряд: ");
            if (!int.TryParse(Console.ReadLine(), out int row))
            {
                Console.WriteLine("Неверный ряд.");
                return;
            }
            Console.Write("Место: ");
            if (!int.TryParse(Console.ReadLine(), out int seat))
            {
                Console.WriteLine("Неверное место.");
                return;
            }

            // 4. Продажа
            Ticket ticket = manager.SellTicket(customer, session, row, seat);
            if (ticket == null)
                return;

            Console.WriteLine("\n✅ Билет куплен!");
            Console.WriteLine(ticket);
            Console.WriteLine($"QR: CINEMA-{ticket.Id}-{ticket.Session.Id}-{ticket.Row}-{ticket.Seat}");
        }

        // TODO 2: Вернуть билет
        public void ReturnTicket()
        {
            Console.WriteLine("=== ВОЗВРАТ БИЛЕТА ===");

            Console.Write("Телефон: ");
            string phone = Console.ReadLine();
            Customer customer = manager.FindCustomerByPhone(phone);
            if (customer == null)
            {
                Console.WriteLine("Посетитель не найден.");
                return;
            }

            if (customer.PurchasedTickets.Count == 0)
            {
                Console.WriteLine("У посетителя нет купленных билетов.");
                return;
            }

            Console.WriteLine("Купленные билеты:");
            foreach (var t in customer.PurchasedTickets)
                Console.WriteLine($"  {t.Id}: {t.Session.Movie.Title} | {t.Session.StartTime:dd.MM HH:mm} | {t.Row}-{t.Seat} | {t.Price} руб.");

            Console.Write("Введите ID билета для возврата: ");
            if (!int.TryParse(Console.ReadLine(), out int ticketId))
            {
                Console.WriteLine("Неверный ID.");
                return;
            }

            Ticket ticket = null;
            foreach (var t in customer.PurchasedTickets)
                if (t.Id == ticketId) { ticket = t; break; }

            if (ticket == null)
            {
                Console.WriteLine("Билет не найден.");
                return;
            }

            // До сеанса должно быть больше часа
            if (ticket.Session.StartTime <= DateTime.Now.AddHours(1))
            {
                Console.WriteLine("Возврат возможен только если до сеанса больше 1 часа.");
                return;
            }

            bool ok = customer.ReturnTicket(ticket);
            if (!ok)
            {
                Console.WriteLine("Не удалось вернуть билет.");
                return;
            }

            decimal refund = Math.Round(ticket.Price * 0.8m, 0);
            Console.WriteLine($"✅ Билет возвращён. Возврат денег: {refund} руб. (80%)");
        }

        // TODO 3: Рекомендации фильмов
        public void ProvideRecommendations()
        {
            Console.WriteLine("=== РЕКОМЕНДАЦИИ ФИЛЬМОВ ===");

            Console.Write("Телефон: ");
            string phone = Console.ReadLine();
            Customer customer = manager.FindCustomerByPhone(phone);
            if (customer == null)
            {
                Console.WriteLine("Посетитель не найден.");
                return;
            }

            Console.Write("Добавить любимый жанр (или Enter чтобы пропустить): ");
            string g = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(g)) customer.AddPreferredGenre(g);

            var rec = customer.GetMovieRecommendations(manager.GetAllMovies());
            if (rec.Count == 0)
            {
                Console.WriteLine("Пока нет рекомендаций.");
                return;
            }

            Console.WriteLine("Рекомендуем посмотреть:");
            foreach (var m in rec)
                Console.WriteLine($"  {m.Id}. {m}");
        }

        // TODO 3: Показать статистику кинотеатра
        public void ShowCinemaStats()
        {
            Console.WriteLine("=== СТАТИСТИКА КИНОТЕАТРА ===");

            Console.WriteLine($"Дневная выручка: {manager.GetDailyRevenue()} руб.");
            Console.WriteLine($"Посетителей зарегистрировано: {manager.GetCustomerCount()}");

            var popularity = manager.GetMoviePopularity();
            if (popularity.Count > 0)
            {
                Console.WriteLine("\nПопулярность фильмов (продано билетов):");
                foreach (var kv in popularity)
                    Console.WriteLine($"  {kv.Key.Title}: {kv.Value}");
            }
            else
            {
                Console.WriteLine("\nПока нет продаж, статистика популярности недоступна.");
            }

            Console.WriteLine("\nЗаполняемость сеансов:");
            foreach (var s in manager.GetAllSessions())
            {
                Console.WriteLine($"  #{s.Id} {s.Movie.Title} {s.StartTime:dd.MM HH:mm} | Свободно: {s.GetAvailableSeatsCount()} | Формат: {s.Format}");
            }
        }

        // Готовый метод - главное меню
        public void ShowMainMenu()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("=== КИНОТЕАТР 'ЭКРАН' ===");
                Console.WriteLine("1. Афиша фильмов");
                Console.WriteLine("2. Расписание сеансов");
                Console.WriteLine("3. Купить билет");
                Console.WriteLine("4. Вернуть билет");
                Console.WriteLine("5. Рекомендации фильмов");
                Console.WriteLine("6. Статистика кинотеатра");
                Console.WriteLine("7. Регистрация посетителя");
                Console.WriteLine("8. Поиск посетителя");
                Console.WriteLine("9. Выход");
                Console.Write("Выберите: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowMovies();
                        break;
                    case "2":
                        ShowSchedule();
                        break;
                    case "3":
                        BuyTicket();
                        break;
                    case "4":
                        ReturnTicket();
                        break;
                    case "5":
                        ProvideRecommendations();
                        break;
                    case "6":
                        ShowCinemaStats();
                        break;
                    case "7":
                        RegisterCustomer();
                        break;
                    case "8":
                        SearchCustomer();
                        break;
                    case "9":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nНажмите Enter...");
                    Console.ReadLine();
                }
            }
        }

        // Метод регистрации посетителя
        private void RegisterCustomer()
        {
            Console.WriteLine("=== РЕГИСТРАЦИЯ НОВОГО ПОСЕТИТЕЛЯ ===");

            Console.Write("ФИО: ");
            string name = Console.ReadLine();

            Console.Write("Дата рождения (дд.мм.гггг): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime birthDate))
            {
                Console.Write("Телефон: ");
                string phone = Console.ReadLine();

                Console.Write("Email: ");
                string email = Console.ReadLine();

                // Определить тип посетителя по возрасту
                int age = DateTime.Now.Year - birthDate.Year;
                string customerType = age < 18 ? "ребенок" : age > 60 ? "пенсионер" : "обычный";

                manager.RegisterCustomer(name, birthDate, phone, email, customerType);
                Console.WriteLine("Посетитель успешно зарегистрирован!");
            }
            else
            {
                Console.WriteLine("Некорректная дата рождения!");
            }
        }

        // Метод поиска посетителя
        private void SearchCustomer()
        {
            Console.Write("Введите телефон посетителя: ");
            string phone = Console.ReadLine();

            Customer customer = manager.FindCustomerByPhone(phone);
            if (customer != null)
            {
                customer.ShowCustomerInfo();
            }
            else
            {
                Console.WriteLine("Посетитель не найден");
            }
        }
    }
}