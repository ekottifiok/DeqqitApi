using Core.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Test.Helper;

public class DatabaseSetupHelper
{
    private readonly SqliteConnection _connection;
    protected readonly List<string> _sqlLog = [];

    protected DatabaseSetupHelper()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .LogTo(log => _sqlLog.Add(log), LogLevel.Information)
            .Options;
        Context = new DataContext(options);
        Context.Database.EnsureCreated();
    }

    protected DataContext Context { get; set; }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}