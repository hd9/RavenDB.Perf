using Perf.Core.Entities;
using Perf.Core.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Core.Helpers
{
    public class ContentGenerator : StaticSvc
    {

        private readonly static List<string> names, words, categories, domains;
        private readonly static Random r = new Random();
        private readonly static int nLen, wLen, cLen, dLen;

        static ContentGenerator()
        {
            names = Resources.Wordlist_Names.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            words = Resources.Wordlist_Words.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            categories = Resources.Wordlist_Categories.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            domains = Resources.Wordlist_Domains.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            nLen = names.Count;
            wLen = words.Count;
            cLen = categories.Count;
            dLen = domains.Count;
        }

        public static List<Listing> CreateListings(int recs)
        {
            var i = 0;
            var ls = new List<Listing>();

            while (i < recs)
            {
                ls.Add(CreateListing());
                i++;
            }

            return ls;
        }

        public static Listing CreateListing(string name = null, User createdBy = null, int descWords = 2000)
        {
            var u = createdBy ?? GenerateUser();
            return new Listing
            {
                Id = $"Listings/{NewGuid()}",
                Name = name ?? RandomSentence(RandomNumber(2,10)),
                Category = RandomCat(),
                Description = RandomSentence(descWords),
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                CreatedBy = u,
                ModifiedBy = u
            };
        }

        public static User GenerateUser()
        {
            var first = RandomName();
            var last = RandomName();
            var e = ToEmail(first, last);

            var user = new User
            {
                Id = $"Users/{NewGuid()}",
                Email = e,
                FirstName = first,
                LastName = last
            };

            return user;
        }

        public static string RandomSentence(int maxWords = 2000, bool camelCase = true)
        {
            var vals = new List<string>();
            for (var i = 0; i < maxWords; i++)
            {
                var w = words[RandomNumber(0, wLen)];
                if (camelCase) w = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(w);
                vals.Add(w);
            }

            return string.Join(" ", vals);
        }

        public static int RandomNumber(int min = 10, int max = 100)
        {
            return r.Next(min, max);
        }

        public static string ToEmail(string first, string last)
        {
            return $"{first.ToLower()}-{last.ToLower()}@{RandomDomain()}";
        }

        public static string NewGuid(int size = 12)
        {
            return Guid.NewGuid().ToString().Substring(0, size).Replace("-", "");
        }

        private static string RandomCat()
        {
            return categories[RandomNumber(0, cLen)];
        }

        public static string RandomName()
        {
            return names[RandomNumber(0, nLen)];
        }

        private static object RandomDomain()
        {
            return domains[RandomNumber(0, dLen)];
        }
    }
}
