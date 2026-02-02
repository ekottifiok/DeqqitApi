using Bogus;
using Core.Data;
using Core.Model;
using Core.Model.Faker;
using Microsoft.AspNetCore.Identity;
using NuGet.Packaging;

namespace Api.Middleware;

public static class SeedDatabaseMiddleware
{
    public static void UseSeedDatabaseMiddleware(this IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        if (context.Users.Count() > 3) return;

        SeedDatabase(context, userManager).Wait();
        if (!context.NoteTypeTemplates.Any() && !context.Decks.Any()) throw new Exception("Unable to Seed Database");
    }

    private static async Task SeedDatabase(DataContext context, UserManager<User> userManager)
    {
        const string password = "sv+4Fn6+VK2GU5W!";
        List<User> users = new UserFaker().GenerateBetween(2, 5);

        foreach (User user in users)
        {
            // 1. Create User (Identity saves here)
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) continue;

            // 2. Generate and Add Parents
            var userDecks = new DeckFaker(user.Id).GenerateBetween(3, 10);
            var userNoteTypes = new NoteTypeFaker(user.Id).GenerateBetween(2, 4);

            await context.Decks.AddRangeAsync(userDecks);
            await context.NoteTypes.AddRangeAsync(userNoteTypes);

            // 🔥 ADD THIS LINE: 
            // This pushes Decks/NoteTypes to the DB so their IDs become "real"
            await context.SaveChangesAsync(); 

            // 3. Prepare Child Entities
            var allNotesForUser = new List<Note>();
            foreach (var notes in from deck in userDecks from noteType in userNoteTypes select new NoteFaker(deck.Id, noteType.Id, user.Id).GenerateBetween(2, 5))
            {
                // ... (rest of your nested card logic)
                allNotesForUser.AddRange(notes);
            }

            // 4. Save the Notes and Cards
            await context.Notes.AddRangeAsync(allNotesForUser);
            await context.SaveChangesAsync(); // Push the final batch
        }
    }
}
