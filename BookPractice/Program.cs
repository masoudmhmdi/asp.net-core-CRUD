using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using System.Net.Mime;

var WebApplicationBuilder = WebApplication.CreateBuilder(args);

WebApplicationBuilder.Services.AddHttpLogging(opts => opts.LoggingFields = HttpLoggingFields.RequestProperties);

WebApplicationBuilder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Information);


var app = WebApplicationBuilder.Build();

var _fruit = new ConcurrentDictionary<string, Fruit>();

app.MapGet("/", () => "Hello World!");

//app.MapGet("/fruit", () => _fruit);


app.MapGet("/fruit/{id}", (string id) =>
{
    return _fruit.TryGetValue(id, out var fruit) ? TypedResults.Ok(fruit) : Results.Problem(statusCode: 404,detail:"id is not exist");
});


app.MapPost("/fruit/{id}", (string id, Fruit fruit) =>
  _fruit.TryAdd(id, fruit)
    ? TypedResults.Created($"/fruit/{id}", fruit)
      :Results.ValidationProblem(new Dictionary<string, string[]>()
      {
          {"id",new[] {"A fruit with this id already exists"} }
      })
      );


app.MapPut("/fruit/{id}", (string id, Fruit fruit) =>
{
    _fruit[id] = fruit;
    return TypedResults.NoContent();
});


app.MapDelete("/fruit/{id}", (string id) =>
{
    _fruit.TryRemove(id, out _);
    return TypedResults.NoContent();
});

//app.MapPost("/fruits/{id}", HandleFruit.AddFruit);
//app.MapGet("/fruits", HandleFruit.GetAllOfTheFruit);
//app.MapPut("fruits/{id}",HandleFruit.UpdateFruit);



app.MapGet("/teapot", (HttpResponse httpResponse) =>
{
    httpResponse.StatusCode = 200;
    httpResponse.ContentType = MediaTypeNames.Text.Plain;
    return httpResponse.WriteAsync("i'm a teapot!");
});

app.Run();


public record Person(string FirstName, string LastName);


public record Fruit
{
    public string Id { get; set; }
    public string Name { get; set; }

    public Guid GUID { get; set; } = new();

}
public class HandleFruit
{
    public static Dictionary<string, Fruit> Fruits = new();


    public static string AddFruit(string id, Fruit value)
    {
        if (value.Id == null || value.Name == null) return "data is wrong";
        Fruits.Add(id, value);
        return "ok";
    }


    public static Dictionary<string, Fruit> GetAllOfTheFruit()
    {
        Dictionary<string, Fruit> temDic = new();

        foreach (var Fruit in Fruits)
        {
            Fruit.Value.GUID = Guid.NewGuid();
            temDic.Add(Fruit.Key, Fruit.Value);
        }
        return Fruits;
    }

    public static string UpdateFruit(string id, Fruit updatedValue)
    {
        if (Fruits.ContainsKey(id))
        {
            Fruits[id] = updatedValue;
            return "operation is successful";
        }
        else
        {
            return "data is not exist";
        }

    }


    public static Fruit GetFruitById(string id)
    {
        return Fruits[id];
    }
}
