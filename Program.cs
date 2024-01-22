using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EfCoreShadowProperties
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var context = new AppDbContext();

            //Shadow Properties - entity de olmayana ama efcore da olan ozelliklerdir
            //Table da gosterilmesini istemediyimiz, emeliyyat aparmayacagimiz columnlar icin ist olunur
            //shadow propertiesin deyerleri change trackir terefinden idare olunur

            //biz eslinde Foreign Key columnlarinda bunu istifade etmisik
            //var blogs = context.Blogs.Include(b => b.Posts)
            //    .ToList();
            //Console.WriteLine();

            //Bir Entity uzerinde Shadow Properties yaratma1 ucun mutleq Fluent API ist. etmelisen
            //Shadow Property yaratmaq ucun
            //modelBuilder.Entity<Blog>()
            //    .Property<DateTime>("CreatedDate");

            //ChangeTracker ile access
            //var blog = await context.Blogs.FirstAsync();
            //var createdDate = context.Entry(blog).Property("CreatedDate");
            //Console.WriteLine(createdDate.CurrentValue);//in memory value
            //Console.WriteLine(createdDate.OriginalValue);
            //createdDate.CurrentValue = DateTime.Now;
            //await context.SaveChangesAsync();

            //EF.Property uzerinden access - ozellikle LINQ sorgularinda
            var blog = await context.Blogs.OrderBy(b => EF.Property<DateTime>(b, "CreatedDate")).ToListAsync();
            var blog2 = await context.Blogs.Where(b => EF.Property<DateTime>(b, "CreatedDate").Year>2020).ToListAsync();
            Console.WriteLine();


        }

        //---------------------------------



        //1 to 1
        public class Person
        {
            public int PersonId { get; set; }
            public string name;
            //yuxaridaki fieldi backin filed kimi ya getter setter ile, ya da attribute ile teyin etmek olar
            //public string Name { get=>name.Substring(0,3); set=>name=value.Substring(0,3); }
            //[BackingField(nameof(name))]

            //Ya da Fluent API de conf. edilir
            //public string Name { get; set; }

            //Field-Only Properties
            public string GetName()
                => name;
            public string SetName(string value)
                => name = value;

            // Navigation property for one-to-one relationship
            public Address Address { get; set; }
        }
        public class Address
        {
            public int AddressId { get; set; }
            public string Street { get; set; }
            public string City { get; set; }

            // Navigation property for one-to-one relationship
            public Person Person { get; set; }
            public int PersonId { get; set; }
        }

        //1 to n
        public class Blog
        {
            public int BlogId { get; set; }
            public string Title { get; set; }

            // Navigation property for one-to-many relationship
            public ICollection<Post> Posts { get; set; }
        }
        public class Post
        {
            public int PostId { get; set; }
            public string Content { get; set; }

            // Foreign key property for one-to-many relationship
            public int BlogId { get; set; }//default conventionda bunu yazmaga ehtiyyac yoxdur, cunki bu shadow properties rolunu oynayir

            // Navigation property for one-to-many relationship
            public Blog Blog { get; set; }
        }

        //n to n
        public class Book
        {
            public int BookId { get; set; }
            public string Title { get; set; }

            // Navigation property for the many-to-many relationship
            public ICollection<BookAuthor> BookAuthors { get; set; }
        }
        public class Author
        {
            public int AuthorId { get; set; }
            public string Name { get; set; }

            // Navigation property for the many-to-many relationship
            public ICollection<BookAuthor> BookAuthors { get; set; }
        }

        // Join entity for the many-to-many relationship
        public class BookAuthor
        {
            public int BookId { get; set; }
            public Book Book { get; set; }

            public int AuthorId { get; set; }
            public Author Author { get; set; }
        }

        public class AppDbContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EfCoreShadowProperties;Trusted_Connection=True;");
            }

            public DbSet<Person> Persons { get; set; }
            public DbSet<Address> Addresses { get; set; }
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }
            public DbSet<Book> Books { get; set; }
            public DbSet<Author> Authors { get; set; }
            public DbSet<BookAuthor> BookAuthors { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                //1 to 1
                modelBuilder.Entity<Person>()
                    //.Property(p => p.Name);
                    //Field-Only Properties ucun HasField olmamalidir ve asagidaki kimi olacaq
                    .Property(nameof(Person.name));
                //.HasField(nameof(Person.name));//backing field in fluent api

                modelBuilder.Entity<Person>()
                .HasOne(p => p.Address)
                .WithOne(a => a.Person)
                .HasForeignKey<Address>(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade);


                //1 to many
                modelBuilder.Entity<Blog>()
                .HasMany(b => b.Posts)   // One Blog has many Posts
                .WithOne(p => p.Blog)    // Each Post belongs to one Blog
                .HasForeignKey(p => p.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

                //Shadow Property yaratmaq ucun
                modelBuilder.Entity<Blog>()
                    .Property<DateTime>("CreatedDate");

                //many to many
                modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

                modelBuilder.Entity<BookAuthor>()
                    .HasOne(ba => ba.Book)
                    .WithMany(b => b.BookAuthors)
                    .HasForeignKey(ba => ba.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<BookAuthor>()
                    .HasOne(ba => ba.Author)
                    .WithMany(a => a.BookAuthors)
                    .HasForeignKey(ba => ba.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

            }

        
        }

    }


}







