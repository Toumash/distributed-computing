using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace wsb.distributedcomputing
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var zadanie = int.Parse(Console.ReadLine());

            if (zadanie == 0)
            {
                var http = new HttpClient();
                var url = Console.ReadLine();

                var pageTask = http.GetStringAsync(url);

                var start = DateTime.Now;

                while (!pageTask.IsCompleted)
                {
                    Console.WriteLine($"DOING MAGIC {(DateTime.Now - start).TotalMilliseconds}");
                }
                Console.WriteLine("Strona ściągnięta");


                Console.WriteLine($"Ilosc znakow na stronie: {(await pageTask).Length}");
            }
            else if (zadanie == 1)
            {
                var rand = new Random();
                var ksiazki = Enumerable.Range(0, 200).Select(i => new Ksiazka() { Id = i }).ToList();
                var uzytkownicy = Enumerable.Range(0, 20).Select(i => new Uzytkownik()
                {
                    Id = i,
                    KsiazkiDoPrzeczytania = ksiazki.Skip(rand.Next(0, 160)).Take(rand.Next(40, 60)).ToList()
                }).ToList();

                var tasks = new List<Task>();
                foreach (var user in uzytkownicy)
                    tasks.Add(Task.Run(() =>
                    {
                        foreach (var ksiazka in user.KsiazkiDoPrzeczytania)
                            user.PrzeczytajKsiazke(ksiazka);
                    }));
                Task.WaitAll(tasks.ToArray());
            }
            else if (zadanie == 2)
            {
                var rand = new Random();
                var ksiazki = Enumerable.Range(0, 200).Select(i => new Ksiazka() { Id = i }).ToList();
                var uzytkownicy = Enumerable.Range(0, 20).Select(i => new Uzytkownik()
                {
                    Id = i,
                    KsiazkiDoPrzeczytania = ksiazki.Skip(rand.Next(0, 160)).Take(rand.Next(40, 60)).ToList()
                }).ToList();

                var tasks = new List<Task>();
                foreach (var user in uzytkownicy)
                    tasks.Add(Task.Run(() =>
                    {
                        foreach (var ksiazka in user.KsiazkiDoPrzeczytania)
                            user.PrzeczytajKsiazkeSemaphore(ksiazka);
                    }));
                Task.WaitAll(tasks.ToArray());
            }
        }
    }

    class Ksiazka
    {
        public int Id { get; set; }
        public int IloscStron => 1000;

        public Semaphore Semaphore { get; set; } = new Semaphore(4, 4);

    }

    class Uzytkownik
    {
        public int Id { get; set; }
        public IEnumerable<Ksiazka> KsiazkiDoPrzeczytania { get; set; }

        public void PrzeczytajKsiazke(Ksiazka ksiazka)
        {
            Console.WriteLine($"Uzytkownik {this.Id} chce przeczytac ksiazke {ksiazka.Id}");
            lock (ksiazka)
            {
                Console.WriteLine($"Uzytkownik {this.Id} rozpoczal czytanie ksiazki {ksiazka.Id}");
                Thread.Sleep(ksiazka.IloscStron);
                Console.WriteLine($"Uzytkownik {this.Id} przeczytal ksiazke {ksiazka.Id}");
            }
        }
        public void PrzeczytajKsiazkeSemaphore(Ksiazka ksiazka)
        {
            Console.WriteLine($"Uzytkownik {this.Id} chce przeczytac ksiazke {ksiazka.Id}");
            ksiazka.Semaphore.WaitOne();
            Console.WriteLine($"Uzytkownik {this.Id} rozpoczal czytanie ksiazki {ksiazka.Id}");
            Thread.Sleep(ksiazka.IloscStron);
            ksiazka.Semaphore.Release();
            Console.WriteLine($"Uzytkownik {this.Id} przeczytal ksiazke {ksiazka.Id}");
        }
    }
}
