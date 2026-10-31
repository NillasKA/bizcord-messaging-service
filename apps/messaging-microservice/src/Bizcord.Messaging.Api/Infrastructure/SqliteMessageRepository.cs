using System.Globalization;
using Bizcord.Messaging.Api.Application;
using Bizcord.Messaging.Api.Domain;
using Microsoft.Data.Sqlite;

namespace Bizcord.Messaging.Api.Infrastructure;

public sealed class SqliteMessageRepository(string connectionString) : IMessageRepository
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var path = new SqliteConnectionStringBuilder(connectionString).DataSource;
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (directory is not null) Directory.CreateDirectory(directory);

        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS messages (
                id TEXT PRIMARY KEY,
                channel_id TEXT NOT NULL,
                author_id TEXT NOT NULL,
                content TEXT NOT NULL,
                posted_at TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ix_messages_channel ON messages(channel_id);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveAsync(Message message, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO messages (id, channel_id, author_id, content, posted_at) VALUES ($id, $channel, $author, $content, $posted);";
        command.Parameters.AddWithValue("$id", message.Id.ToString());
        command.Parameters.AddWithValue("$channel", message.ChannelId.ToString());
        command.Parameters.AddWithValue("$author", message.AuthorId.ToString());
        command.Parameters.AddWithValue("$content", message.Content.Value);
        command.Parameters.AddWithValue("$posted", message.PostedAt.ToString("O", CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Message?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, channel_id, author_id, content, posted_at FROM messages WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadMessage(reader) : null;
    }

    public async Task<IReadOnlyList<Message>> ListAsync(Guid? channelId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, channel_id, author_id, content, posted_at FROM messages"
            + (channelId.HasValue ? " WHERE channel_id = $channel" : "") + " ORDER BY posted_at, id;";
        if (channelId.HasValue) command.Parameters.AddWithValue("$channel", channelId.Value.ToString());
        var result = new List<Message>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) result.Add(ReadMessage(reader));
        return result;
    }

    public async Task<bool> UpdateAsync(Message message, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE messages SET content = $content WHERE id = $id;";
        command.Parameters.AddWithValue("$content", message.Content.Value);
        command.Parameters.AddWithValue("$id", message.Id.ToString());
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM messages WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString());
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static Message ReadMessage(SqliteDataReader reader)
        => Message.Restore(
            Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)),
            reader.GetString(3), DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
}
