using MySql.Data.MySqlClient;

namespace TaskTracker;

/// <summary>
/// simple console task list backed by MySQL.
/// </summary>
internal static class Program
{
    // set TASKTRACKER_CONNECTION_STRING so passwords never live in source control
    private static string ConnectionString = null!;

    private static void Main()
    {
        var cs = Environment.GetEnvironmentVariable("TASKTRACKER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(cs))
        {
            Console.WriteLine("Missing TASKTRACKER_CONNECTION_STRING.");
            Console.WriteLine("Set it to your MySQL ADO.NET connection string, then run again.");
            return;
        }

        ConnectionString = cs;

        Console.WriteLine("TaskTracker — MySQL task list");
        Console.WriteLine("------------------------------");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1) Add task");
            Console.WriteLine("2) List all tasks");
            Console.WriteLine("3) Mark task completed (by ID)");
            Console.WriteLine("4) Delete task (by ID)");
            Console.WriteLine("5) Exit");
            Console.Write("Choose an option (1-5): ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        AddTask();
                        break;
                    case "2":
                        ListTasks();
                        break;
                    case "3":
                        CompleteTask();
                        break;
                    case "4":
                        DeleteTask();
                        break;
                    case "5":
                        Console.WriteLine("Goodbye.");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Enter 1-5.");
                        break;
                }
            }
            catch (MySqlException ex)
            {
                // surface DB errors (wrong password, unknown DB)
                Console.WriteLine($"Database error: {ex.Message}");
            }
        }
    }

    /// <summary> prompts for title and description, inserts one row</summary>
    private static void AddTask()
    {
        Console.Write("Title: ");
        var title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Title is required.");
            return;
        }

        Console.Write("Description (optional): ");
        var description = Console.ReadLine() ?? string.Empty;

        // parameterized INSERT - values are never concatenated into SQL text
        const string sql = "INSERT INTO tasks (title, description) VALUES (@title, @description);";

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@description", description);
        command.ExecuteNonQuery();

        Console.WriteLine("Task added.");
    }

    /// <summary>reads all rows and prints them</summary>
    private static void ListTasks()
    {
        const string sql = """
            SELECT id, title, description, is_completed, created_at
            FROM tasks
            ORDER BY id;
            """;

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        using var command = new MySqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        if (!reader.HasRows)
        {
            Console.WriteLine("No tasks yet.");
            return;
        }

        while (reader.Read())
        {
            var id = reader.GetInt32("id");
            var title = reader.GetString("title");
            var description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? ""
                : reader.GetString("description");
            var done = reader.GetBoolean("is_completed");
            var created = reader.GetDateTime("created_at");

            var status = done ? "done" : "open";
            Console.WriteLine($"[{id}] {title} — {status}");
            if (!string.IsNullOrEmpty(description))
                Console.WriteLine($"    {description}");
            Console.WriteLine($"    created: {created:yyyy-MM-dd HH:mm:ss}");
        }
    }

    /// <summary>sets is_completed = TRUE for the given id</summary>
    private static void CompleteTask()
    {
        Console.Write("Task ID to complete (number in [brackets] from list): ");
        if (!int.TryParse(Console.ReadLine(), out var id) || id <= 0)
        {
            Console.WriteLine("Please enter a positive integer ID.");
            return;
        }

        const string sql = "UPDATE tasks SET is_completed = TRUE WHERE id = @id;";

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        var affected = command.ExecuteNonQuery();

        if (affected == 0)
            Console.WriteLine("No task with that ID.");
        else
            Console.WriteLine("Task marked completed.");
    }

    /// <summary>deletes the row with the given id</summary>
    private static void DeleteTask()
    {
        Console.Write("Task ID to delete (number in [brackets] from list): ");
        if (!int.TryParse(Console.ReadLine(), out var id) || id <= 0)
        {
            Console.WriteLine("Please enter a positive integer ID.");
            return;
        }

        const string sql = "DELETE FROM tasks WHERE id = @id;";

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        var affected = command.ExecuteNonQuery();

        if (affected == 0)
            Console.WriteLine("No task with that ID.");
        else
            Console.WriteLine("Task deleted.");
    }
}
