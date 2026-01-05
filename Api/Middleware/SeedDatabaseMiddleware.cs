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
        // context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        if (context.Users.Count() > 3) return;

        SeedDatabase(context, userManager).Wait();
        if (!context.NoteTypeTemplates.Any() && !context.Decks.Any()) throw new Exception("Unable to Seed Database");
    }

    private static async Task SeedDatabase(DataContext context, UserManager<User> userManager)
    {
        const string password = "sv+4Fn6+VK2GU5W!";

        List<User>? users = new UserFaker().GenerateBetween(2, 5);

        foreach (User user in users)
        {
            IdentityResult result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) 
                throw new Exception($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            List<NoteType>? noteTypes = new NoteTypeFaker(user.Id).GenerateBetween(2, 4);
            List<Deck>? decks = new DeckFaker(user.Id).GenerateBetween(1, 2);

            foreach (Deck deck in decks)
            {
                foreach (NoteType noteType in noteTypes)
                {
                    List<Note>? notes = new NoteFaker(deck.Id, 0, user.Id).GenerateBetween(2, 5);

                    foreach (Note note in notes)
                    {
                        note.NoteType = noteType;

                        foreach (NoteTypeTemplate template in noteType.Templates)
                        {
                            List<Card>? cards = new CardFaker(0, 0).GenerateBetween(1, 2);
                            foreach (Card card in cards)
                            {
                                card.Template = template;
                                note.Cards.Add(card);
                            }
                        }
                    }

                    deck.Notes.AddRange(notes);
                }
            }

            context.NoteTypes.AddRange(noteTypes);
            context.Decks.AddRange(decks);
        }

        await context.SaveChangesAsync();
    }
}