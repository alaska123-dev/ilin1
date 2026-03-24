using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var notes = new List<Note>();
var nextId = 1;

app.MapGet("/health", () =>
{
    return new
    {
        status = "ok",
        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    };
});

app.MapGet("/version", (IConfiguration config) =>
{
    return new
    {
        name = config["App:Name"],
        version = config["App:Version"]
    };
});

app.MapGet("/api/notes", () =>
{
    return Results.Ok(notes);
});

app.MapGet("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note == null)
    {
        return Results.NotFound(new { error = "Заметка не найдена" });
    }
    return Results.Ok(note);
});

app.MapPost("/api/notes", (CreateNoteRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new { error = "Название заметки обязательно" });
    }
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new { error = "Текст заметки обязателен" });
    }
    if (request.Title.Length > 100)
    {
        return Results.BadRequest(new { error = "Название не должно превышать 100 символов" });
    }
    if (request.Text.Length > 1000)
    {
        return Results.BadRequest(new { error = "Текст не должен превышать 1000 символов" });
    }

    var note = new Note
    {
        Id = nextId++,
        Title = request.Title,
        Text = request.Text,
        CreatedAt = DateTime.Now
    };
    notes.Add(note);
    return Results.Created($"/api/notes/{note.Id}", note);
});

app.MapDelete("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note == null)
    {
        return Results.NotFound(new { error = "Заметка не найдена" });
    }
    notes.Remove(note);
    return Results.Ok(new { message = "Заметка удалена", id = id });
});

app.MapGet("/db/ping", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("Mssql");
    
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return Results.Ok(new { status = "ok", message = "Подключение к БД успешно" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { status = "error", message = ex.Message });
    }
});

app.Run();

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNoteRequest
{
    public string Title { get; set; }
    public string Text { get; set; }
}