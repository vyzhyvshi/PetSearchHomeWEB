$assembly = "$env:USERPROFILE\.nuget\packages\npgsql\9.0.0\lib\net8.0\Npgsql.dll"
Add-Type -Path $assembly
Add-Type -ReferencedAssemblies $assembly -TypeDefinition @"
using System;
using Npgsql;

public static class DbInspector
{
    public static void Run()
    {
        var connectionString = "Host=20.238.24.15;Port=5433;Database=appdb_new;Username=postgres;Password=z0K3R95txNS?";
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("select listing_id, title, status, user_id from listings order by listing_id", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var title = reader.IsDBNull(1) ? "<null>" : reader.GetString(1);
            var status = reader.GetInt32(2);
            var userId = reader.GetInt32(3);
            Console.WriteLine($"{id}: {title} (status={status}, user={userId})");
        }
    }
}
"@

[DbInspector]::Run()
