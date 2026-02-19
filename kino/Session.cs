// TODO:
// 1. Реализовать создание сеансов для фильмов
// 2. Реализовать систему мест в зале
// 3. Реализовать бронирование и продажу билетов

using System;
using System.Collections.Generic;

namespace Cinema
{
    internal static class TicketIdGenerator
    {
        private static int _nextId = 10000;

        public static int Next()
        {
            return _nextId++;
        }
    }

    public class Session
    {
        public int Id { get; set; }                    // ID сеанса
        public Movie Movie { get; set; }               // Фильм
        public DateTime StartTime { get; set; }        // Время начала
        public string Hall { get; set; }               // Зал (№1, №2, VIP)
        public string Format { get; set; }             // Формат (2D, 3D, IMAX, 4DX)

        private int rows = 10;                         // Количество рядов
        private int seatsPerRow = 15;                  // Количество мест в ряду
        private bool[,] seats;                         // Матрица мест (true - занято, false - свободно)
        private List<Ticket> soldTickets = new List<Ticket>();

        // Цена билета с учетом формата (2D/3D/IMAX/4DX)
        public decimal TicketPrice { get; private set; }

        public Session(int id, Movie movie, DateTime startTime, string hall, string format)
        {
            Id = id;
            Movie = movie;
            StartTime = startTime;
            Hall = hall;
            Format = format;

            // Инициализировать матрицу мест
            seats = new bool[rows, seatsPerRow];

            // Рассчитать цену билета через movie.CalculatePrice(format)
            TicketPrice = movie.CalculatePrice(format);
        }

        // TODO 2: Показать схему зала
        public void ShowHallLayout()
        {
            Console.WriteLine($"\n=== ЗАЛ {Hall} ===");
            Console.WriteLine($"ЭКРАН");
            Console.WriteLine(new string('-', seatsPerRow * 3));

            // Вывести номера мест
            Console.Write("   ");
            for (int seat = 1; seat <= seatsPerRow; seat++)
            {
                Console.Write($"{seat:D2} ");
            }
            Console.WriteLine();

            // Вывести ряды с занятостью мест
            for (int row = 0; row < rows; row++)
            {
                Console.Write($"{row + 1:D2}: ");
                for (int seat = 0; seat < seatsPerRow; seat++)
                {
                    Console.Write(seats[row, seat] ? "[X]" : "[ ]");
                }
                Console.WriteLine();
            }
        }

        // TODO 2: Забронировать место
        public bool BookSeat(int row, int seat)
        {
            // Проверить корректность ряда и места
            // Проверить свободно ли место
            // Если свободно - отметить как занятое и вернуть true
            // Если занято или неверные координаты - вернуть false
            if (row < 1 || row > rows || seat < 1 || seat > seatsPerRow)
                return false;

            int r = row - 1;
            int s = seat - 1;
            if (seats[r, s])
                return false;

            seats[r, s] = true;
            return true;
        }

        // TODO 2: Освободить место
        public void FreeSeat(int row, int seat)
        {
            // Проверить корректность ряда и места
            // Если место занято - освободить его
            if (row < 1 || row > rows || seat < 1 || seat > seatsPerRow)
                return;

            int r = row - 1;
            int s = seat - 1;
            if (seats[r, s])
                seats[r, s] = false;
        }

        // TODO 3: Продать билет
        public Ticket SellTicket(int row, int seat, Customer customer = null)
        {
            // Проверить доступность места (BookSeat)
            // Создать новый билет с уникальным номером
            // Установить цену с учетом формата
            // Добавить билет в soldTickets
            // Вернуть созданный билет
            // Проверить доступность места (BookSeat)
            if (!BookSeat(row, seat))
                return null;

            Ticket ticket = new Ticket
            {
                Id = TicketIdGenerator.Next(),
                Session = this,
                Row = row,
                Seat = seat,
                Price = TicketPrice,
                PurchaseTime = DateTime.Now,
                Customer = customer
            };

            soldTickets.Add(ticket);
            return ticket;
        }

        // TODO 3: Получить количество свободных мест
        public int GetAvailableSeatsCount()
        {
            int count = 0;

            // Посчитать все свободные места в матрице seats
            for (int r = 0; r < rows; r++)
            {
                for (int s = 0; s < seatsPerRow; s++)
                {
                    if (!seats[r, s]) count++;
                }
            }

            return count;
        }

        // TODO 3: Проверить есть ли свободные места
        public bool HasAvailableSeats()
        {
            return GetAvailableSeatsCount() > 0;
        }

        // Показать информацию о сеансе
        public void ShowSessionInfo()
        {
            Console.WriteLine($"Сеанс #{Id}");
            Console.WriteLine($"Фильм: {Movie.Title}");
            Console.WriteLine($"Время: {StartTime:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Зал: {Hall}, Формат: {Format}");
            Console.WriteLine($"Свободных мест: {GetAvailableSeatsCount()}/{rows * seatsPerRow}");
        }
    }

    public class Ticket
    {
        public int Id { get; set; }
        public Session Session { get; set; }
        public int Row { get; set; }
        public int Seat { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseTime { get; set; }
        public Customer Customer { get; set; }

        public override string ToString()
        {
            return $"Билет #{Id}: {Session.Movie.Title}, место {Row}-{Seat}, {Price} руб.";
        }
    }
}