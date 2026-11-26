namespace KT43
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            {
                Console.WriteLine("Синхронное освобождение");

                using (new DatabaseConnection())
                {
                    Console.WriteLine("Работаем с базой данных");
                }

                Console.WriteLine();

                Console.WriteLine("Асинхронное освобождение");

                await using (new DatabaseConnection())
                {
                    Console.WriteLine("Работаем с базой данных");
                }

                Console.WriteLine();

                Console.WriteLine("Проверка финилизатора");

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine("Проверка завершена");

                Console.WriteLine();

                Console.WriteLine("Эксперимент без Dispose");

                CreateConnectionWithoutDispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine("Эксперимент завершен");
            }

            static void CreateConnectionWithoutDispose()
            {
                DatabaseConnection connection = new DatabaseConnection();
            }
        }

        class DatabaseConnection : IDisposable, IAsyncDisposable
        {
            private List<string> logs = new List<string>();

            private int connectionId;

            private bool disposed = false;

            public DatabaseConnection()
            {
                connectionId = new Random().Next(1000, 9999);

                Console.WriteLine($"Создано соединение №{connectionId}");

                logs.Add("Подключение к базе данных");
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        Console.WriteLine("Освобождение управляемых ресурсов");

                        foreach (string log in logs)
                        {
                            Console.WriteLine($"Лог: {log}");
                        }

                        logs.Clear();
                    }

                    Console.WriteLine($"Закрытие нативного ресурса №{connectionId}");

                    connectionId = 0;

                    disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);

                Console.WriteLine("Выполнен Dispose()");
            }

            protected virtual async ValueTask DisposeAsyncCore()
            {
                Console.WriteLine("Начато асинхронное освобождение ресурсов");

                await Task.Delay(10);

                Console.WriteLine("DisposeAsyncCore выполнен");
            }

            public async ValueTask DisposeAsync()
            {
                await DisposeAsyncCore();

                Dispose(false);

                GC.SuppressFinalize(this);

                Console.WriteLine("Выполнен DisposeAsync()");
            }

            ~DatabaseConnection()
            {
                Console.WriteLine("Вызван финализатор");

                Dispose(false);
            }
        }
    }
}
