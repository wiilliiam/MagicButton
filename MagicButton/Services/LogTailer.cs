using System.Text;

public static class LogTailer
{
    // Efficient tail: scans backwards for '\n' without loading full file
    public static async Task<string[]> ReadLastLinesAsync(string path, int maxLines = 500, CancellationToken ct = default)
    {
        if (!File.Exists(path)) return Array.Empty<string>();

        var lines = new List<string>(maxLines);
        var newline = (byte)'\n';
        var buffer = new byte[4096];
        int found = 0;
        long pos;

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        pos = fs.Length;

        var sb = new StringBuilder();

        while (pos > 0 && found <= maxLines)
        {
            var read = (int)Math.Min(buffer.Length, pos);
            pos -= read;
            fs.Position = pos;
            await fs.ReadAsync(buffer.AsMemory(0, read), ct);

            for (int i = read - 1; i >= 0; i--)
            {
                if (buffer[i] == newline)
                {
                    found++;
                    if (found > maxLines) break;
                }
                sb.Append((char)buffer[i]);
            }
        }

        // We built it backwards; fix order & line breaks
        var chunk = new string(sb.ToString().Reverse().ToArray());
        var all = chunk.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Take the last maxLines safely
        return all.Length <= maxLines ? all : all[^maxLines..];
    }
}
