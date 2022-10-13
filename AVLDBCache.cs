using AVLDBCacheService;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Build our AVL Tree
var tree = new AVLTree();

// Configure the HTTP request pipeline.
app.MapGet("/getdata", async (
    HttpRequest request) =>
{
    var dbname = request.Query["dbname"];
    var sql = request.Query["sql"];
    var cacheLife = DateTime.Parse(request.Query["cachelife"]);

    //Try and Find the node in the tree
    var result = tree.Find(sql + dbname);

    var returnValue = result.result;

    //If we cannot find it make a DB request, cache time check is ugly but can sort that later
    if (result == null )
    {
        var output = "";
        var connection = new SqlConnection(dbname);
        using (connection)
        {
            connection.Open();
            var command = new SqlCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                output += reader;
            }

            connection.Close();
        }

        //Add it to the Tree
        tree.Insert(sql, dbname, cacheLife, output);
        returnValue = output;

    } else if (result.expiryTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds < cacheLife.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds)
    {
        //we have found the node but its out of date
        //delete 
        tree.Delete(sql + dbname);

        //request fresh data from DB
        var output = "";
        var connection = new SqlConnection(dbname);
        using (connection)
        {
            connection.Open();
            var command = new SqlCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                output += reader;
            }

            connection.Close();
        }

        //insert our new data
        tree.Insert(sql, dbname, cacheLife, "");

        returnValue = output;
    }

    if (result == null)
    {
        return returnValue;
    } else
    {
        return "";
    }
}).Produces(200);

app.Run();