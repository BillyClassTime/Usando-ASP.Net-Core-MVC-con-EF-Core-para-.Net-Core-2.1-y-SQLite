using EntityFrameworkCore.Models;
using System.Linq;
namespace EntityFrameworkCore.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BlogContext context)
        {
            context.Database.EnsureCreated();
            if (context.Blogs.Any())
            {
                return; // Db has been seeded
            }
            var blogs = new Blog[]
            {
                new Blog { Url = "http://blogs.msdn.com/adonet", BlogId = 1 },
                new Blog { Url = "https://devblogs.microsoft.com/dotnet", BlogId = 2 },
                new Blog { Url = "https://daddyr.blogspot.com/", BlogId = 3 },
                new Blog { Url = "https://billyclasstime.com/", BlogId = 4}
            };
            foreach (Blog b in blogs)
            {
                context.Blogs.Add(b);
            }
            context.SaveChanges();
            var post = new Post[]
            {
                new Post                    {
                    Title = "Hello World",
                    Content = "I wrote an app using EF Core!",
                    BlogId = 1
                },
                                    new Post                    {
                    Title = "EF Core vs EF Framework",
                    Content = "The difference bettewn two regular versions",
                    BlogId = 1
                },
                new Post                    {
                    Title = "ASP.Net Core using EF 6",
                    Content = "Has future the EF 6 with Dot Net 5?",
                    BlogId = 2
                },
                new Post                    {
                    Title = "Performance with EF 6",
                    Content = "Mits and realities with ORM ultra typeed",
                    BlogId = 3
                },
                                    new Post                    {
                    Title = "Xamarin and the Dot Net 5",
                    Content = "Is Dot Net 5 sure enought to implement mobil apps in IOS? ",
                    BlogId = 4
                }
            };
            context.SaveChanges();
        }
    }
}