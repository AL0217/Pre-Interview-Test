using Newtonsoft.Json;
using interview_test;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PlanetDB>(opt => opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();
List<Planet> response = new List<Planet>();

using (HttpClient httpClient = new HttpClient())
{
    string secondApiUrl = ""; // Extract the URL from the response

    //Get the List
    int i = 1;
    //Get the planet one by one until it returns 404
    while (true)
    {
        secondApiUrl = "https://swapi.dev/api/planets/" + i;
        HttpResponseMessage response2 = await httpClient.GetAsync(secondApiUrl);

        var secondApiResponse = await response2.Content.ReadAsStringAsync();
        if (!response2.IsSuccessStatusCode)
        {
            break;
        }
        secondApiResponse = await response2.Content.ReadAsStringAsync();
        var p = JsonConvert.DeserializeObject<Planet>(secondApiResponse);
        response.Add(p);
        i++;
    }
}


app.MapGet("/planets", () =>
   {
       return Results.Ok(response);
   });


app.MapGet("/favourites/", (PlanetDB db) =>
{
    //return the list of the planets
    if(db.Planets != null)
    {
        return Results.Ok(db.Planets.ToList());
    }
    return Results.NotFound();
});

//return the planet according to id
app.MapGet("/favourites/{id}", async (int id, PlanetDB db) =>
    await db.Planets.FindAsync(id)
        is Planet planet
            ? Results.Ok(planet)
            : Results.NotFound());


//Post the planet to favourite and save to the database
app.MapPost("/favourites", async (Planet planet, PlanetDB db) =>
{
    foreach (var p in db.Planets)
    {
        if(p.name == planet.name)
        {
            return Results.NotFound("Already favourited");
        }
    }
    db.Planets.Add(planet);
    await db.SaveChangesAsync();
    return Results.Created($"/planets/{planet.Id}", planet);
});

app.MapGet("/random", (PlanetDB db) =>
{
    Random rand = new Random();
    int randID = rand.Next(0, response.Count);
    while (db.Planets.Any(planet => planet.name == response[randID].name))
    {
        randID = rand.Next(0, response.Count);
    }
    return response[randID];
});


//Delete a planet according the id
app.MapDelete("/favourites/{id}", async (int id, PlanetDB db) =>
{
    if (await db.Planets.FindAsync(id) is Planet planet)
    {
        db.Planets.Remove(planet);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
