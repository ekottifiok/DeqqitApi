// using Bogus;
// using Core.Data;
//
// namespace Core.Model.Faker;
//
// public static class LearningCardsFakerFactory
// {
//     public static async Task<User> SeedDatabase(DataContext context)
//     {
//         List<User> users = new UserFaker().GenerateBetween(1, 5);
//         User user = users.First();
//         List<Deck> decks = new DeckFaker(user.Id).GenerateBetween(1, 3);
//         List<NoteType> noteTypes = new NoteTypeFaker(user.Id).GenerateBetween(1, 4);
//         
//         user.Decks = decks;
//         user.NoteTypes = noteTypes;
//         await context.Users.AddRangeAsync(users);
//         await context.SaveChangesAsync();
//         
//         foreach (Deck deck in decks)
//         {
//             foreach (NoteType noteType in noteTypes)
//             {
//                 new NoteFaker(deck.Id, noteType.Id, user.Id).GenerateBetween(2,5).ForEach(deck.Notes.Add);
//             }
//         }
//         await context.SaveChangesAsync();
//         
//         noteTypes.ForEach(x =>
//         {
//             foreach (NoteTypeTemplate noteTypeTemplate in x.Templates)
//             {
//                 decks.ForEach(d =>
//                 {
//                     foreach (Note note in d.Notes)
//                     {
//                         new CardFaker(note.Id, noteTypeTemplate.Id).GenerateBetween(1, 5).ForEach(d.Cards.Add);
//                     }
//                 });
//             }
//         });
//         await context.SaveChangesAsync();
//         return user;
//     }
// }

