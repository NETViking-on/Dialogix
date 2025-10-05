using System;
using Npgsql;

public class DatabaseTest
{
    public static void TestConnection(string connectionString)
    {
        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("Успешное подключение к базе данных!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка подключения: " + ex.Message);
        }
    }
}
