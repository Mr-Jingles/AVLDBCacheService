using AVLDBCacheService;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Build our AVL Tree
var tree = new AVLTree();

// Configure the HTTP request pipeline.
app.MapGet("/getdata", async (
    HttpRequest request) =>
{
    try
    {
        var dbname = request.Query["dbname"];
        var sql = request.Query["sql"];
        var cacheLife = DateTime.Parse(request.Query["cachelife"]);

        if (dbname == "" || sql == "")
        {
            //invalid return out, empty string to avoid scraping
            return "";
        }

        //Try and Find the node in the tree
        var result = tree.Find(sql + dbname);

        var returnValue = result.result;

        //If we cannot find it make a DB request, cache time check is ugly but can sort that later
        if (result == null)
        {
            var dbOutput = getNewData(dbname, sql);

            //Add it to the Tree
            tree.Insert(sql, dbname, cacheLife, dbOutput.ToString());
            returnValue = dbOutput.ToString();

        }
        else if (Util.CompareTime(result.expiryTime, DateTime.Now) < 0)
        {
            //we have found the node but its out of date
            //delete 
            tree.Delete(sql + dbname);

            var dbOutput = getNewData(dbname, sql);

            //insert our new data
            tree.Insert(sql, dbname, cacheLife, dbOutput.ToString());

            returnValue = dbOutput.ToString();
        }

        if (result == null)
        {
            return returnValue;
        }
        else
        {
            return "";
        }
    } catch (Exception e)
    {
        // all exceptions should be swallowed for security
        return "";
    }
    
}).Produces(200);


// Makes our DB request to fetch new data
JObject getNewData(string dbname, string sql) {
    //request fresh data from DB
    var jsonOutput = new JObject();
    var connection = new SqlConnection(dbname);
    using (connection)
    {
        connection.Open();
        var command = new SqlCommand(sql, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var jsonRow = new JArray();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i);
                jsonRow.Add(new JProperty(name, value));
            }
            jsonOutput.Add(jsonRow);
        }

        connection.Close();
    }

    return jsonOutput;
}

app.Run();