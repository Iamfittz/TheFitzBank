using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Infrastructure.RandomDataBase {
    public static class DbInitializer {
        private static readonly string[] FirstNames = new[] {
        "Artem", "Volodymyr", "Vitaliy", "Alex", "Niko", "Maria", "Eugene", "Sophia", 
        "Anastasia", "Isabella", "Denis", "Sergey", "Dmytro", "Ivan", "Valeriy"
    };

        private static readonly string[] LastNames = new[] {
        "Milan", "Madrid", "Barcelona", "Kyiv", "Vinnytsa", "Donetsk", "London", "Chicago",
        "Stambul", "Lisbon", "Tottenham", "Paris", "Dortmund", "Malta", "New-York"
    };

        private static readonly string[] Currencies = new[] { "USD", "EUR", "GBP", "JPY", "UAH" };

        public static async Task SeedAsync(BankingContext context) {
            if (await context.Accounts.AnyAsync()) return;

            var random = new Random();

            var fakeAccounts = Enumerable.Range(1, 1000)
                .Select(i => {
                    var firstName = FirstNames[random.Next(FirstNames.Length)];
                    var lastName = LastNames[random.Next(LastNames.Length)];
                    var fullName = $"{firstName} {lastName}";
                    var currency = Currencies[random.Next(Currencies.Length)];
                    var balance = Math.Round((decimal)(random.NextDouble() * 5000 + 50), 2); // от 50 до 5050

                    return new Account(
                        $"ACC{i:000000}",
                        fullName,
                        currency,
                        balance
                    );
                });

            await context.Accounts.AddRangeAsync(fakeAccounts);
            await context.SaveChangesAsync();
        }
    }
}
