using AVLDBCacheService;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

//http://localhost:9999/getdata?dbname=Server=.\SQLEXPRESS;Database=TestingDatabase;User%20Id=cacheservice;Password=cacheservice;Encrypt=False&sql=select%20*%20from%20[TestingDatabase].[dbo].[testingtable]&cachelife=1s

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

        //cachelife formate is 1d2h3m4s
        var cacheLife = Util.ParseCacheLife(request.Query["cachelife"].ToString());

        if (dbname == "" || sql == "")
        {
            //invalid return out, empty string to avoid scraping
            return "";
        }

        //Try and Find the node in the tree
        var result = tree.Find(sql + dbname);

        var returnValue = "";

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
        } else
        {
            returnValue = result.result;
        }

        if (returnValue != null)
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
JArray getNewData(string dbname, string sql) {
    //request fresh data from DB
    var jsonOutput = new JArray();
    var connection = new SqlConnection(dbname);
    using (connection)
    {
        connection.Open();
        var command = new SqlCommand(sql, connection);
        var reader = command.ExecuteReader();

        int counter = 0;

        while (reader.Read())
        {
            var jsonRow = new JObject();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i);
                jsonRow.Add(new JProperty(name, value));
            }
            jsonOutput.Add(jsonRow);
            counter++;
        }

        connection.Close();
    }

    return jsonOutput;
}

app.Run();