// See https://aka.ms/new-console-template for more information

using ConsoleApp;

Console.WriteLine("Hello, World!");
DeckServiceTestFaker testFaker = new();
await testFaker.GetSummary_DeckWithMixedCardsDueInFuture_ExcludesFutureDueCards();
testFaker.PrintSqlCommand();