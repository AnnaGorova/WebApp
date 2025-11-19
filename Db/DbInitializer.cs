using Microsoft.EntityFrameworkCore;
using WebApp.Entities;

namespace WebApp.Db
{
    /// <summary>
    /// Клас ініціалізації бази даних.
    /// Заповнює базу даних початковими даними для всіх сутностей.
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Метод для ініціалізації бази даних та заповнення її початковими даними.
        /// </summary>
        public static void Initialize(AgencyDBContext context)
        {

            // Перевіряємо чи існує база даних
            if (!context.Database.CanConnect())
            {
                // Якщо бази немає - створюємо її
                context.Database.EnsureCreated();
            }
            else
            {
                // Якщо база існує - перевіряємо чи створені таблиці
                var tablesExist = CheckIfTablesExist(context);
                if (!tablesExist)
                {
                    context.Database.EnsureCreated();
                }
            }

            // Заповнюємо даними тільки якщо таблиці порожні
            SeedOptions(context);
            SeedNavigates(context);
            SeedClientMessages(context);
            SeedFooterData(context);


            // Нові методи для блогу
            SeedCategories(context);
            SeedTags(context);
            SeedPosts(context);

            context.SaveChanges();



            SeedComments(context);
            SeedPostTags(context);
            SeedPostCategories(context);


            context.SaveChanges();
        }

        /// <summary>
        /// Перевіряє чи існують основні таблиці в базі даних.
        /// </summary>
        private static bool CheckIfTablesExist(AgencyDBContext context)
        {
            try
            {
                // Спробуємо виконати прості запити до кожної таблиці
                // Якщо таблиці не існують - виникне виключення
                var optionsExist = context.Options.Any();
                var navigatesExist = context.Navigates.Any();
                var clientMessagesExist = context.ClientMessages.Any();


                var categoriesExist = context.Categories.Any();
                var tagsExist = context.Tags.Any();
                var postsExist = context.Posts.Any();
                var commentsExist = context.Comments.Any();


                return true; // Якщо дійшли сюди - таблиці існують
            }
            catch
            {
                return false; // Якщо виникла помилка - таблиць немає
            }
        }



        /// <summary>
        /// Заповнюємо таблицю Options початковими даними.
        /// </summary>
        private static void SeedOptions(AgencyDBContext context)
        {
            if (!context.Options.Any())
            {
                context.Options.AddRange(
                     new Option
                     {
                         Name = "site-logo",
                         Key = "<i class=\"fa fa-user-tie me-2\"></i>",
                         Value = "Щастинка щастя",
                         Relation = "logo",
                         Order = 1,
                         IsSystem = false
                     },
                    new Option
                    {
                        Name = "Facebook",
                        Key = "<i class=\"fab fa-facebook-f fw-normal\"></i>",
                        Value = "https://facebook.com",
                        Relation = "social-link",
                        Order = 2,
                        IsSystem = false
                    },
                    new Option
                    {
                        Name = "Twitter",
                        Key = "<i class=\"fab fa-twitter fw-normal\"></i>",
                        Value = "https://twitter.com",
                        Relation = "social-link",
                        Order = 3,
                        IsSystem = false
                    },
                    new Option
                    {
                        Name = "LinkedIn",
                        Key = "<i class=\"fab fa-linkedin-in fw-normal\"></i>",
                        Value = "https://linkedin.com",
                        Relation = "social-link",
                        Order = 4,
                        IsSystem = false
                    },
                    new Option
                    {
                        Name = "Instagram",
                        Key = "<i class=\"fab fa-instagram fw-normal\"></i>",
                        Value = "https://instagram.com",
                        Relation = "social-link",
                        Order = 5,
                        IsSystem = false
                    },
                     new Option
                     {
                         Name = "YouTube",
                         Key = "<i class=\"fab fa-youtube fw-normal\"></i>",
                         Value = "https://www.youtube.com/",
                         Relation = "social-link",
                         Order = 6,
                         IsSystem = false
                     },
                    new Option
                    {
                        Name = "Адреса",
                        Key = "<i class=\"fa fa-map-marker-alt me-2\"></i>",
                        Value = "123 Street, New York, USA",
                        Relation = "contact-info",
                        Order = 1,
                        IsSystem = true
                    },
                    new Option
                    {
                        Name = "Телефон",
                        Key = "<i class=\"fa fa-phone-alt me-2\"></i>",
                        Value = "+012 345 67890",
                        Relation = "contact-info",
                        Order = 2,
                        IsSystem = true
                    },
                    new Option
                    {
                        Name = "Email",
                        Key = "<i class=\"fa fa-envelope-open me-2\"></i>",
                        Value = "info@example.com",
                        Relation = "contact-info",
                        Order = 3,
                        IsSystem = true
                    }
                );
            }
        }

        /// <summary>
        /// Заповнюємо таблицю Navigates початковими даними для навігаційного меню.
        /// </summary>
        private static void SeedNavigates(AgencyDBContext context)
        {
            if (!context.Navigates.Any())
            {
                var navigates = new List<Navigate>
                {
                    new Navigate { Title = "Home", Href = "/", Order = 1, ParentID = null },
                    new Navigate { Title = "About", Href = "/about", Order = 2, ParentID = null },
                    new Navigate { Title = "Services", Href = "/services", Order = 3, ParentID = null },
                    new Navigate { Title = "Blog", Href = "#", Order = 4, ParentID = null },
                    new Navigate { Title = "Pages", Href = "#", Order = 5, ParentID = null },
                    new Navigate { Title = "Contact", Href = "/About/ContactUs", Order = 6, ParentID = null }
        

                    //new Navigate { Title = "Головна", Href = "/", Order = 1, ParentID = null },
                    //new Navigate { Title = "Про нас", Href = "/about", Order = 2, ParentID = null },
                    //new Navigate { Title = "Послуги", Href = "/services", Order = 3, ParentID = null },
                    //new Navigate { Title = "Блог", Href = "/blog", Order = 4, ParentID = null },
                    //new Navigate { Title = "Контакти", Href = "/contact", Order = 5, ParentID = null }
                };

                context.Navigates.AddRange(navigates);
                context.SaveChanges();


                // Отримуємо ID створених елементів
                var blogNavigate = context.Navigates.First(n => n.Title == "Blog");
                var pagesNavigate = context.Navigates.First(n => n.Title == "Pages");

                // Дочірні елементи для Blog
                var blogChildren = new List<Navigate>
                {
                    new Navigate { Title = "Blog Grid", Href = "/Blog/BlogGridIndex", Order = 1, ParentID = blogNavigate.Id },
                    new Navigate { Title = "Blog Detail", Href = "/Blog/BlogDetailIndex", Order = 2, ParentID = blogNavigate.Id }
                };

                // Дочірні елементи для Pages (6 елементів як у шаблоні)
                var pagesChildren = new List<Navigate>
                {
                    new Navigate { Title = "Pricing Plan", Href = "/Pages/PricingPlan", Order = 1, ParentID = pagesNavigate.Id },
                    new Navigate { Title = "Our Features", Href = "/Pages/OurFeatures", Order = 2, ParentID = pagesNavigate.Id },
                    new Navigate { Title = "Team Members", Href = "/Pages/TeamMembers", Order = 3, ParentID = pagesNavigate.Id },
                    new Navigate { Title = "Testimonial", Href = "/Pages/Testimonial", Order = 4, ParentID = pagesNavigate.Id },
                    new Navigate { Title = "Free Quote", Href = "/Pages/FreeQuote", Order = 5, ParentID = pagesNavigate.Id }
                };

                context.Navigates.AddRange(blogChildren);
                context.Navigates.AddRange(pagesChildren);
                context.SaveChanges();



                var demoNavigate = new Navigate
                {
                    Title = "Multi Level",
                    Href = "#",
                    Order = 6,
                    ParentID = pagesNavigate.Id
                };
                context.Navigates.Add(demoNavigate);
                context.SaveChanges();

                // Створюємо структуру:
                // Multi Level
                //   → Level 1
                //     → Level 1.1
                //       → Level 1.1.1
                //   → Level 2
                var level1 = new Navigate { Title = "Level 1", Href = "#", Order = 1, ParentID = demoNavigate.Id };
                context.Navigates.Add(level1);
                context.SaveChanges();

                var level1_1 = new Navigate { Title = "Level 1.1", Href = "#", Order = 1, ParentID = level1.Id };
                context.Navigates.Add(level1_1);
                context.SaveChanges();

                var level1_1_1 = new Navigate { Title = "Level 1.1.1", Href = "/deep/final", Order = 1, ParentID = level1_1.Id };
                context.Navigates.Add(level1_1_1);
                context.SaveChanges();

                var level1_1_1_1 = new Navigate { Title = "Level 1.1.1.1", Href = "/deep/final/final1", Order = 1, ParentID = level1_1_1.Id };
                context.Navigates.Add(level1_1_1_1);
                context.SaveChanges();

                var level2 = new Navigate { Title = "Level 2", Href = "#", Order = 2, ParentID = demoNavigate.Id };
                context.Navigates.Add(level2);
                context.SaveChanges();

                var level2_2 = new Navigate { Title = "Level 2.2", Href = "/deep/level2.2", Order = 2, ParentID = level2.Id };
                context.Navigates.Add(level2_2);
                context.SaveChanges();




            }
        

        

        //var aboutNavigate = context.Navigates.First(n => n.Title == "Про нас");
        //        var aboutChildren = new List<Navigate>
        //        {
        //            new Navigate { Title = "Наша команда", Href = "/about/team", Order = 1, ParentID = aboutNavigate.Id },
        //            new Navigate { Title = "Історія", Href = "/about/history", Order = 2, ParentID = aboutNavigate.Id },
        //            new Navigate { Title = "Відгуки", Href = "/about/testimonials", Order = 3, ParentID = aboutNavigate.Id }
        //        };

        //        context.Navigates.AddRange(aboutChildren);
        //    }
        }


        private static List<Navigate> CreateDeepNavigationHierarchy(AgencyDBContext context, int parentId, int levels)
        {
            var result = new List<Navigate>();

            if (levels <= 0)
            {
                return result;
            }

            var currentParentId = parentId;
            for (var i = 1; i <= levels; i++)
            {
                var child = new Navigate
                {
                    Title = $"Sub Level {i}",
                    Href = $"/pages/level/{i}",
                    Order = i,
                    ParentID = currentParentId
                };

                // Додаємо до контексту і зберігаємо, щоб отримати реальний ID
                context.Navigates.Add(child);
                context.SaveChanges();

                result.Add(child);
                currentParentId = child.Id; // Тепер використовуємо реальний ID
            }

            return result;
        }
        /// <summary>
        /// Заповнюємо таблицю ClientMessages початковими даними.
        /// </summary>
        private static void SeedClientMessages(AgencyDBContext context)
        {
            if (!context.ClientMessages.Any())
            {
                context.ClientMessages.AddRange(
                    new ClientMessage
                    {
                        UserName = "Іван Петренко",
                        UserEmail = "ivan@example.com",
                        Subject = "Запитання щодо послуг",
                        Message = "Мене цікавлять ваші послуги. Надішліть, будь ласка, детальну інформацію.",
                        DateOfCreated = DateTime.Now.AddDays(-2),
                        IsAnswered = true
                    },
                    new ClientMessage
                    {
                        UserName = "Марія Коваленко",
                        UserEmail = "maria@example.com",
                        Subject = "Запит на підтримку",
                        Message = "Чи можете ви допомогти мені з проблемами в обліковому записі?",
                        DateOfCreated = DateTime.Now.AddDays(-1),
                        IsAnswered = false
                    }
                );
            }
        }

        /// <summary>
        /// Заповнюємо дані для футера.
        /// </summary>
        private static void SeedFooterData(AgencyDBContext context)
        {
            if (!context.FooterQuickLinks.Any())
            {
                context.FooterQuickLinks.AddRange(
                    new FooterQuickLinks { Title = "Home", Href = "/", Order = 1 },
                    new FooterQuickLinks { Title = "About Us", Href = "/about", Order = 2 },
                    new FooterQuickLinks { Title = "Services", Href = "/services", Order = 3 }
                );
            }

            if (!context.FooterLinks.Any())
            {
                context.FooterLinks.AddRange(
                    new FooterLink { Title = "Home", Href = "/", Order = 1 },
                    new FooterLink { Title = "About Us", Href = "/about", Order = 2 },
                    new FooterLink { Title = "Contact", Href = "/contact", Order = 3 }
                );
            }
        }











        /// <summary>
        /// Заповнюємо таблицю Categories початковими даними.
        /// </summary>
        private static void SeedCategories(AgencyDBContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
            new Category
            {
                Name = "Web Development",
                Slug = "web-development",
                Description = "Articles about web technologies and frameworks",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Mobile Development",
                Slug = "mobile-development",
                Description = "Mobile app development for iOS and Android",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Cloud Computing",
                Slug = "cloud-computing",
                Description = "Cloud services and infrastructure",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Artificial Intelligence",
                Slug = "artificial-intelligence",
                Description = "AI and machine learning technologies",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "DevOps",
                Slug = "devops",
                Description = "Development operations and deployment",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Data Science",
                Slug = "data-science",
                Description = "Data analysis, visualization and machine learning",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Cybersecurity",
                Slug = "cybersecurity",
                Description = "Security practices and threat protection",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Blockchain",
                Slug = "blockchain",
                Description = "Distributed ledger technology and cryptocurrencies",
                ImageSrc = "/img/team-2.jpg"
            },
            new Category
            {
                Name = "Internet of Things",
                Slug = "iot",
                Description = "Connected devices and smart technology",
                ImageSrc = "/img/team-3.jpg"
            },
            new Category
            {
                Name = "Game Development",
                Slug = "game-development",
                Description = "Creating video games and interactive experiences",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "UI/UX Design",
                Slug = "ui-ux-design",
                Description = "User interface and user experience design",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Database Management",
                Slug = "database-management",
                Description = "Database design, optimization and administration",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Software Testing",
                Slug = "software-testing",
                Description = "Quality assurance and testing methodologies",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Project Management",
                Slug = "project-management",
                Description = "Agile, Scrum and project delivery",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Programming Languages",
                Slug = "programming-languages",
                Description = "C#, JavaScript, Python and other languages",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Frontend Development",
                Slug = "frontend-development",
                Description = "HTML, CSS, JavaScript and modern frameworks",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Backend Development",
                Slug = "backend-development",
                Description = "Server-side programming and APIs",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Full Stack Development",
                Slug = "full-stack-development",
                Description = "Combining frontend and backend technologies",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Microservices",
                Slug = "microservices",
                Description = "Distributed systems and service architecture",
                ImageSrc = "/img/team-1.jpg"
            },
            new Category
            {
                Name = "Career & Learning",
                Slug = "career-learning",
                Description = "Career advice and learning resources",
                ImageSrc = "/img/team-1.jpg"
            }
                );

                Console.WriteLine("✅ Categories table seeded successfully");
            }
        }

        /// <summary>
        /// Заповнюємо таблицю Tags початковими даними.
        /// </summary>
        private static void SeedTags(AgencyDBContext context)
        {
            if (!context.Tags.Any())
            {
                context.Tags.AddRange(
                    new Tag { Name = "ASP.NET Core", Slug = "aspnet-core" },
                    new Tag { Name = "Entity Framework", Slug = "entity-framework" },
                    new Tag { Name = "MVC", Slug = "mvc" },
                    new Tag { Name = "SQL Server", Slug = "sql-server" },
                    new Tag { Name = "Bootstrap", Slug = "bootstrap" },
                    new Tag { Name = "JavaScript", Slug = "javascript" },
                    new Tag { Name = "TypeScript", Slug = "typescript" },
                    new Tag { Name = "React", Slug = "react" },
                    new Tag { Name = "Vue.js", Slug = "vue-js" },
                    new Tag { Name = "Angular", Slug = "angular" },
                    new Tag { Name = "Python", Slug = "python" },
                    new Tag { Name = "Django", Slug = "django" },
                    new Tag { Name = "Docker", Slug = "docker" },
                    new Tag { Name = "Kubernetes", Slug = "kubernetes" },
                    new Tag { Name = "Azure", Slug = "azure" },
                    new Tag { Name = "AWS", Slug = "aws" },
                    new Tag { Name = "Git", Slug = "git" },
                    new Tag { Name = "REST API", Slug = "rest-api" },
                    new Tag { Name = "GraphQL", Slug = "graphql" },
                    new Tag { Name = "Microservices", Slug = "microservices" }
                );

                Console.WriteLine("✅ Tags table seeded successfully");
            }
        }

        /// <summary>
        /// Заповнюємо таблицю Posts початковими даними.
        /// </summary>
        private static void SeedPosts(AgencyDBContext context)
        {
            if (!context.Posts.Any())
            {
                var posts = new List<Post>
        {
                new Post
                {
                    Name = "Getting Started with ASP.NET Core",
                    Slug = "getting-started-aspnet-core",
                    Description = "Learn the basics of ASP.NET Core framework",
                    ImageSrc = "/img/blog-1.jpg",
                    Context = "ASP.NET Core is a cross-platform framework for building modern cloud-based web applications...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-20),
                    DataOfUpdated = DateTime.Now.AddDays(-2),
                    DataOfPublished = DateTime.Now.AddDays(-19)
                },
                new Post
                {
                    Name = "Entity Framework Core Tutorial",
                    Slug = "entity-framework-core-tutorial",
                    Description = "Complete guide to Entity Framework Core",
                    ImageSrc = "/img/blog-2.jpg",
                    Context = "Entity Framework Core is a lightweight, extensible ORM for .NET applications...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-18),
                    DataOfUpdated = DateTime.Now.AddDays(-1),
                    DataOfPublished = DateTime.Now.AddDays(-17)
                },
                new Post
                {
                    Name = "Building RESTful APIs with Web API",
                    Slug = "building-restful-apis-web-api",
                    Description = "How to create powerful REST APIs",
                    ImageSrc = "/img/blog-3.jpg",
                    Context = "ASP.NET Core Web API makes it easy to build HTTP services that reach a broad range of clients...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-16),
                    DataOfUpdated = DateTime.Now.AddDays(-1),
                    DataOfPublished = DateTime.Now.AddDays(-15)
                },
                new Post
                {
                    Name = "Introduction to Blazor",
                    Slug = "introduction-to-blazor",
                    Description = "Building web apps with C# instead of JavaScript",
                    ImageSrc = "/img/carousel-1.jpg",
                    Context = "Blazor lets you build interactive web UIs using C# instead of JavaScript...",
                    PostStatuses = PostStatuses.Draft,
                    DataOfCreated = DateTime.Now.AddDays(-14),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now.AddDays(1)
                },
                new Post
                {
                    Name = "SQL Server Performance Tips",
                    Slug = "sql-server-performance-tips",
                    Description = "Optimize your database queries",
                    ImageSrc = "/img/carousel-2.jpg",
                    Context = "SQL Server performance optimization is crucial for application scalability...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-12),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now.AddDays(-11)
                },
                new Post
                {
                    Name = "Mastering JavaScript ES6+ Features",
                    Slug = "mastering-javascript-es6",
                    Description = "Modern JavaScript features you need to know",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "ES6 introduced many powerful features that changed how we write JavaScript...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-10),
                    DataOfUpdated = DateTime.Now.AddDays(-3),
                    DataOfPublished = DateTime.Now.AddDays(-9)
                },
                new Post
                {
                    Name = "React Hooks Complete Guide",
                    Slug = "react-hooks-complete-guide",
                    Description = "Understanding and using React Hooks effectively",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "React Hooks revolutionized functional components in React...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-8),
                    DataOfUpdated = DateTime.Now.AddDays(-2),
                    DataOfPublished = DateTime.Now.AddDays(-7)
                },
                new Post
                {
                    Name = "Docker for Developers",
                    Slug = "docker-for-developers",
                    Description = "Containerize your applications with Docker",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "Docker simplifies application deployment and environment consistency...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-6),
                    DataOfUpdated = DateTime.Now.AddDays(-1),
                    DataOfPublished = DateTime.Now.AddDays(-5)
                },
                new Post
                {
                    Name = "Microservices Architecture Patterns",
                    Slug = "microservices-architecture-patterns",
                    Description = "Design patterns for microservices systems",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "Building scalable systems with microservices requires careful planning...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-4),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now.AddDays(-3)
                },
                new Post
                {
                    Name = "Python for Web Development",
                    Slug = "python-web-development",
                    Description = "Building web applications with Python and Django",
                    ImageSrc = "/img/team-2.jpg",
                    Context = "Python offers powerful frameworks for web development like Django and Flask...",
                    PostStatuses = PostStatuses.Draft,
                    DataOfCreated = DateTime.Now.AddDays(-2),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now.AddDays(2)
                },
                new Post
                {
                    Name = "CSS Grid vs Flexbox",
                    Slug = "css-grid-vs-flexbox",
                    Description = "When to use CSS Grid and when to use Flexbox",
                    ImageSrc = "/img/team-3.jpg",
                    Context = "Understanding the differences between CSS Grid and Flexbox is crucial for modern layouts...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-15),
                    DataOfUpdated = DateTime.Now.AddDays(-5),
                    DataOfPublished = DateTime.Now.AddDays(-14)
                },
                new Post
                {
                    Name = "Git Best Practices",
                    Slug = "git-best-practices",
                    Description = "Professional Git workflow and collaboration",
                    ImageSrc = "/img/testimonial-1.jpg",
                    Context = "Mastering Git is essential for effective team collaboration and code management...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-13),
                    DataOfUpdated = DateTime.Now.AddDays(-4),
                    DataOfPublished = DateTime.Now.AddDays(-12)
                },
                new Post
                {
                    Name = "Azure Cloud Services Overview",
                    Slug = "azure-cloud-services",
                    Description = "Introduction to Microsoft Azure cloud platform",
                    ImageSrc = "/img/testimonial-2.jpg",
                    Context = "Azure provides a comprehensive suite of cloud services for modern applications...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-11),
                    DataOfUpdated = DateTime.Now.AddDays(-3),
                    DataOfPublished = DateTime.Now.AddDays(-10)
                },
                new Post
                {
                    Name = "Vue.js 3 Composition API",
                    Slug = "vuejs-composition-api",
                    Description = "New features in Vue.js 3 and Composition API",
                    ImageSrc = "/img/testimonial-3.jpg",
                    Context = "Vue.js 3 introduces the Composition API for better code organization...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-9),
                    DataOfUpdated = DateTime.Now.AddDays(-2),
                    DataOfPublished = DateTime.Now.AddDays(-8)
                },
                new Post
                {
                    Name = "Database Design Principles",
                    Slug = "database-design-principles",
                    Description = "Fundamental principles of good database design",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "Proper database design is crucial for application performance and scalability...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-7),
                    DataOfUpdated = DateTime.Now.AddDays(-1),
                    DataOfPublished = DateTime.Now.AddDays(-6)
                },
                new Post
                {
                    Name = "TypeScript for Large Applications",
                    Slug = "typescript-large-applications",
                    Description = "Using TypeScript in enterprise-scale projects",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "TypeScript brings type safety and better tooling to JavaScript projects...",
                    PostStatuses = PostStatuses.Draft,
                    DataOfCreated = DateTime.Now.AddDays(-5),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now.AddDays(3)
                },
                new Post
                {
                    Name = "REST API Security Best Practices",
                    Slug = "rest-api-security",
                    Description = "Securing your REST APIs from common vulnerabilities",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "API security is critical for protecting data and preventing attacks...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-3),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now.AddDays(-2)
                },
                new Post
                {
                    Name = "Kubernetes for Beginners",
                    Slug = "kubernetes-beginners",
                    Description = "Getting started with container orchestration",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "Kubernetes helps manage containerized applications at scale...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-1),
                    DataOfUpdated = DateTime.Now,
                    DataOfPublished = DateTime.Now
                },
                new Post
                {
                    Name = "GraphQL vs REST",
                    Slug = "graphql-vs-rest",
                    Description = "Comparing GraphQL and REST API approaches",
                    ImageSrc = "/img/team-2.jpg",
                    Context = "GraphQL offers a flexible alternative to traditional REST APIs...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-17),
                    DataOfUpdated = DateTime.Now.AddDays(-6),
                    DataOfPublished = DateTime.Now.AddDays(-16)
                },
                new Post
                {
                    Name = "Career Growth for Developers",
                    Slug = "career-growth-developers",
                    Description = "Advancing your career in software development",
                    ImageSrc = "/img/team-1.jpg",
                    Context = "Building a successful career requires continuous learning and strategic planning...",
                    PostStatuses = PostStatuses.Published,
                    DataOfCreated = DateTime.Now.AddDays(-19),
                    DataOfUpdated = DateTime.Now.AddDays(-7),
                    DataOfPublished = DateTime.Now.AddDays(-18)
                }
        };

                context.Posts.AddRange(posts);
                Console.WriteLine("✅ Posts table seeded successfully");
            }
        }


        private static void SeedComments(AgencyDBContext context)
        {
            if (!context.Comments.Any())
            {
                var posts = context.Posts.ToList();
                var allComments = new List<Comment>();
                var commentId = 1;

                // Список унікальних аватарок з сервісу
                var avatarUrls = new List<string>
        {
            "https://i.pravatar.cc/150?img=1",
            "https://i.pravatar.cc/150?img=2",
            "https://i.pravatar.cc/150?img=3",
            "https://i.pravatar.cc/150?img=4",
            "https://i.pravatar.cc/150?img=5",
            "https://i.pravatar.cc/150?img=6",
            "https://i.pravatar.cc/150?img=7",
            "https://i.pravatar.cc/150?img=8",
            "https://i.pravatar.cc/150?img=9",
            "https://i.pravatar.cc/150?img=10",
            "https://i.pravatar.cc/150?img=11",
            "https://i.pravatar.cc/150?img=12",
            "https://i.pravatar.cc/150?img=13",
            "https://i.pravatar.cc/150?img=14",
            "https://i.pravatar.cc/150?img=15"
        };

                var avatarIndex = 0;

                foreach (var post in posts)
                {
                    // ==== РІВЕНЬ 1: Кореневі коментарі ====
                    var rootComments = new List<Comment>
            {
                new Comment {
                    Id = commentId++,
                    UserLogin = "alex",
                    UserEmail = "alex@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Автоматично вибирає аватарку
                    Text = "Дуже цікава стаття про " + post.Name + "!",
                    DateOfCreated = DateTime.Now.AddMinutes(-200),
                    PostId = post.Id,
                    ParentCommentId = null,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },
                new Comment {
                    Id = commentId++,
                    UserLogin = "maria",
                    UserEmail = "maria@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Повністю погоджуюсь з автором статті!",
                    DateOfCreated = DateTime.Now.AddMinutes(-180),
                    PostId = post.Id,
                    ParentCommentId = null,
                     IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ

                },
                new Comment {
                    Id = commentId++,
                    UserLogin = "john",
                    UserEmail = "john@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Маю кілька зауважень щодо цієї теми.",
                    DateOfCreated = DateTime.Now.AddMinutes(-150),
                    PostId = post.Id,
                    ParentCommentId = null,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },
                new Comment {
                    Id = commentId++,
                    UserLogin = "olga",
                    UserEmail = "olga@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Чи можете ви розповісти більше про " + post.Name + "?",
                    DateOfCreated = DateTime.Now.AddMinutes(-120),
                    PostId = post.Id,
                    ParentCommentId = null,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                }
            };

                    // ==== РІВЕНЬ 2: Відповіді на кореневі коментарі ====
                    var level2Comments = new List<Comment>
            {
                // Відповіді на коментар alex
                new Comment {
                    Id = commentId++,
                    UserLogin = "kate",
                    UserEmail = "kate@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Так, я теж знайшла багато корисного в цій статті!",
                    DateOfCreated = DateTime.Now.AddMinutes(-170),
                    PostId = post.Id,
                    ParentCommentId = rootComments[0].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },
                new Comment {
                    Id = commentId++,
                    UserLogin = "max",
                    UserEmail = "max@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Особливо сподобалася практична частина.",
                    DateOfCreated = DateTime.Now.AddMinutes(-160),
                    PostId = post.Id,
                    ParentCommentId = rootComments[0].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },

                // Відповіді на коментар maria
                new Comment {
                    Id = commentId++,
                    UserLogin = "igor",
                    UserEmail = "igor@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Я також повністю згоден з Марією!",
                    DateOfCreated = DateTime.Now.AddMinutes(-140),
                    PostId = post.Id,
                    ParentCommentId = rootComments[1].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },

                // Відповіді на коментар john
                new Comment {
                    Id = commentId++,
                    UserLogin = "anna",
                    UserEmail = "anna@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Які саме зауваження у вас є? Мені цікаво почути.",
                    DateOfCreated = DateTime.Now.AddMinutes(-130),
                    PostId = post.Id,
                    ParentCommentId = rootComments[2].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },
                new Comment {
                    Id = commentId++,
                    UserLogin = "artem",
                    UserEmail = "artem@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Дійсно, є кілька суперечливих моментів.",
                    DateOfCreated = DateTime.Now.AddMinutes(-125),
                    PostId = post.Id,
                    ParentCommentId = rootComments[2].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },

                // Відповіді на коментар olga
                new Comment {
                    Id = commentId++,
                    UserLogin = "sergiy",
                    UserEmail = "sergiy@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Я можу допомогти з додатковою інформацією!",
                    DateOfCreated = DateTime.Now.AddMinutes(-110),
                    PostId = post.Id,
                    ParentCommentId = rootComments[3].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                }
            };

                    // ==== РІВЕНЬ 3: Відповіді на відповіді ====
                    var level3Comments = new List<Comment>
            {
                // Відповідь на відповідь kate
                new Comment {
                    Id = commentId++,
                    UserLogin = "natalia",
                    UserEmail = "natalia@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Кате, яка частина була найкориснішою для вас?",
                    DateOfCreated = DateTime.Now.AddMinutes(-155),
                    PostId = post.Id,
                    ParentCommentId = level2Comments[0].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },

                // Відповідь на відповідь anna
                new Comment {
                    Id = commentId++,
                    UserLogin = "john", // Той самий john відповідає
                    UserEmail = "john@mail.com",
                    UserAvatar = rootComments[2].UserAvatar, // ✅ Той самий аватар, що й у першого коментаря john
                    Text = "Анно, звичайно! Мені здається, що автор недооцінив важливість...",
                    DateOfCreated = DateTime.Now.AddMinutes(-120),
                    PostId = post.Id,
                    ParentCommentId = level2Comments[3].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },

                // Відповідь на відповідь artem
                new Comment {
                    Id = commentId++,
                    UserLogin = "viktor",
                    UserEmail = "viktor@mail.com",
                    UserAvatar = avatarUrls[avatarIndex++ % avatarUrls.Count], // ✅ Наступна аватарка
                    Text = "Артеме, які саме моменти ви вважаєте суперечливими?",
                    DateOfCreated = DateTime.Now.AddMinutes(-115),
                    PostId = post.Id,
                    ParentCommentId = level2Comments[4].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                }
            };

                    // ==== РІВЕНЬ 4: Глибша вкладеність ====
                    var level4Comments = new List<Comment>
            {
                // Відповідь на відповідь natalia
                new Comment {
                    Id = commentId++,
                    UserLogin = "kate", // Той самий kate відповідає
                    UserEmail = "kate@mail.com",
                    UserAvatar = level2Comments[0].UserAvatar, // ✅ Той самий аватар, що й у першого коментаря kate
                    Text = "Наталіє, мені найбільше сподобався розділ про впровадження на практиці!",
                    DateOfCreated = DateTime.Now.AddMinutes(-145),
                    PostId = post.Id,
                    ParentCommentId = level3Comments[0].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                },

                // Відповідь на відповідь john
                new Comment {
                    Id = commentId++,
                    UserLogin = "artem", // Той самий artem відповідає
                    UserEmail = "artem@mail.com",
                    UserAvatar = level2Comments[4].UserAvatar, // ✅ Той самий аватар, що й у першого коментаря artem
                    Text = "Джоне, я згоден з твоїми зауваженнями! Особливо щодо масштабованості.",
                    DateOfCreated = DateTime.Now.AddMinutes(-115),
                    PostId = post.Id,
                    ParentCommentId = level3Comments[1].Id,
                    IsRequired = true,
                    IsApproved= true // ІСНУЮЧІ КОМЕНТАРІ ВЖЕ СХВАЛЕНІ
                }
            };

                    // Додаємо всі коментарі поста до загального списку
                    allComments.AddRange(rootComments);
                    allComments.AddRange(level2Comments);
                    allComments.AddRange(level3Comments);
                    allComments.AddRange(level4Comments);
                }

                // Зберігаємо всі коментарі в базу
                context.Database.OpenConnection();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Comments ON");
                context.Comments.AddRange(allComments);
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Comments OFF");
                context.Database.CloseConnection();
            }
        }

        //private static void SeedComments(AgencyDBContext context)
        //{
        //    if (!context.Comments.Any())
        //    {
        //        var posts = context.Posts.ToList();
        //        var allComments = new List<Comment>();
        //        var commentId = 1;


        //        // Список унікальних аватарок з сервісу
        //        var avatarUrls = new List<string>
        //        {
        //            "https://i.pravatar.cc/150?img=1",
        //            "https://i.pravatar.cc/150?img=2",
        //            "https://i.pravatar.cc/150?img=3",
        //            "https://i.pravatar.cc/150?img=4",
        //            "https://i.pravatar.cc/150?img=5",
        //            "https://i.pravatar.cc/150?img=6",
        //            "https://i.pravatar.cc/150?img=7",
        //            "https://i.pravatar.cc/150?img=8",
        //            "https://i.pravatar.cc/150?img=9",
        //            "https://i.pravatar.cc/150?img=10",
        //            "https://i.pravatar.cc/150?img=11",
        //            "https://i.pravatar.cc/150?img=12",
        //            "https://i.pravatar.cc/150?img=13",
        //            "https://i.pravatar.cc/150?img=14",
        //            "https://i.pravatar.cc/150?img=15"
        //        };

        //        var avatarIndex = 0;


        //        foreach (var post in posts)
        //        {
        //            // ==== РІВЕНЬ 1: Кореневі коментарі ====
        //            var rootComments = new List<Comment>
        //    {
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "alex",
        //            UserEmail = "alex@mail.com",
        //            UserAvatar = "avatar1.png",
        //            Text = "Дуже цікава стаття про " + post.Name + "!",
        //            DateOfCreated = DateTime.Now.AddMinutes(-200),
        //            PostId = post.Id, // ✅ Використовуємо ID поточного поста
        //            ParentCommentId = null,
        //            IsRequired = true
        //        },
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "maria",
        //            UserEmail = "maria@mail.com",
        //            UserAvatar = "~/img/user.jpg",
        //            Text = "Повністю погоджуюсь з автором статті!",
        //            DateOfCreated = DateTime.Now.AddMinutes(-180),
        //            PostId = post.Id, // ✅ Використовуємо ID поточного поста
        //            ParentCommentId = null,
        //            IsRequired = true
        //        },
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "john",
        //            UserEmail = "john@mail.com",
        //            UserAvatar = "~/img/avatar3.png",
        //            Text = "Маю кілька зауважень щодо цієї теми.",
        //            DateOfCreated = DateTime.Now.AddMinutes(-150),
        //            PostId = post.Id, // ✅ Використовуємо ID поточного поста
        //            ParentCommentId = null,
        //            IsRequired = true
        //        },
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "olga",
        //            UserEmail = "olga@mail.com",
        //            UserAvatar = "~/img/avatar4.png",
        //            Text = "Чи можете ви розповісти більше про " + post.Name + "?",
        //            DateOfCreated = DateTime.Now.AddMinutes(-120),
        //            PostId = post.Id, // ✅ Використовуємо ID поточного поста
        //            ParentCommentId = null,
        //            IsRequired = true
        //        }
        //    };

        //            // ==== РІВЕНЬ 2: Відповіді на кореневі коментарі ====
        //            var level2Comments = new List<Comment>
        //    {
        //        // Відповіді на коментар alex
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "kate",
        //            UserEmail = "kate@mail.com",
        //            UserAvatar = "~/img/avatar5.png",
        //            Text = "Так, я теж знайшла багато корисного в цій статті!",
        //            DateOfCreated = DateTime.Now.AddMinutes(-170),
        //            PostId = post.Id,
        //            ParentCommentId = rootComments[0].Id, // Відповідь на alex
        //            IsRequired = true
        //        },
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "max",
        //            UserEmail = "max@mail.com",
        //            UserAvatar = "/img/avatar6.png",
        //            Text = "Особливо сподобалася практична частина.",
        //            DateOfCreated = DateTime.Now.AddMinutes(-160),
        //            PostId = post.Id,
        //            ParentCommentId = rootComments[0].Id, // Відповідь на alex
        //            IsRequired = true
        //        },

        //        // Відповіді на коментар maria
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "igor",
        //            UserEmail = "igor@mail.com",
        //            UserAvatar = "/img/avatar7.png",
        //            Text = "Я також повністю згоден з Марією!",
        //            DateOfCreated = DateTime.Now.AddMinutes(-140),
        //            PostId = post.Id,
        //            ParentCommentId = rootComments[1].Id, // Відповідь на maria
        //            IsRequired = true
        //        },

        //        // Відповіді на коментар john
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "anna",
        //            UserEmail = "anna@mail.com",
        //            UserAvatar = "/img/avatar8.png",
        //            Text = "Які саме зауваження у вас є? Мені цікаво почути.",
        //            DateOfCreated = DateTime.Now.AddMinutes(-130),
        //            PostId = post.Id,
        //            ParentCommentId = rootComments[2].Id, // Відповідь на john
        //            IsRequired = true
        //        },
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "artem",
        //            UserEmail = "artem@mail.com",
        //            UserAvatar = "/img/avatar9.png",
        //            Text = "Дійсно, є кілька суперечливих моментів.",
        //            DateOfCreated = DateTime.Now.AddMinutes(-125),
        //            PostId = post.Id,
        //            ParentCommentId = rootComments[2].Id, // Відповідь на john
        //            IsRequired = true
        //        },

        //        // Відповіді на коментар olga
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "sergiy",
        //            UserEmail = "sergiy@mail.com",
        //            UserAvatar = "/img/avatar10.png",
        //            Text = "Я можу допомогти з додатковою інформацією!",
        //            DateOfCreated = DateTime.Now.AddMinutes(-110),
        //            PostId = post.Id,
        //            ParentCommentId = rootComments[3].Id, // Відповідь на olga
        //            IsRequired = true
        //        }
        //    };

        //            // ==== РІВЕНЬ 3: Відповіді на відповіді ====
        //            var level3Comments = new List<Comment>
        //    {
        //        // Відповідь на відповідь kate
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "natalia",
        //            UserEmail = "natalia@mail.com",
        //            UserAvatar = "/img/avatar11.png",
        //            Text = "Кате, яка частина була найкориснішою для вас?",
        //            DateOfCreated = DateTime.Now.AddMinutes(-155),
        //            PostId = post.Id,
        //            ParentCommentId = level2Comments[0].Id, // Відповідь на kate
        //            IsRequired = true
        //        },

        //        // Відповідь на відповідь anna
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "john", // Той самий john відповідає
        //            UserEmail = "john@mail.com",
        //            UserAvatar = "/img/avatar3.png",
        //            Text = "Анно, звичайно! Мені здається, що автор недооцінив важливість...",
        //            DateOfCreated = DateTime.Now.AddMinutes(-120),
        //            PostId = post.Id,
        //            ParentCommentId = level2Comments[3].Id, // Відповідь на anna
        //            IsRequired = true
        //        },

        //        // Відповідь на відповідь artem
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "viktor",
        //            UserEmail = "viktor@mail.com",
        //            UserAvatar = "/img/avatar12.png",
        //            Text = "Артеме, які саме моменти ви вважаєте суперечливими?",
        //            DateOfCreated = DateTime.Now.AddMinutes(-115),
        //            PostId = post.Id,
        //            ParentCommentId = level2Comments[4].Id, // Відповідь на artem
        //            IsRequired = true
        //        }
        //    };

        //            // ==== РІВЕНЬ 4: Глибша вкладеність ====
        //            var level4Comments = new List<Comment>
        //    {
        //        // Відповідь на відповідь natalia
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "kate", // Той самий kate відповідає
        //            UserEmail = "kate@mail.com",
        //            UserAvatar = "/img/avatar5.png",
        //            Text = "Наталіє, мені найбільше сподобався розділ про впровадження на практиці!",
        //            DateOfCreated = DateTime.Now.AddMinutes(-145),
        //            PostId = post.Id,
        //            ParentCommentId = level3Comments[0].Id, // Відповідь на natalia
        //            IsRequired = true
        //        },

        //        // Відповідь на відповідь john
        //        new Comment {
        //            Id = commentId++,
        //            UserLogin = "artem",
        //            UserEmail = "artem@mail.com",
        //            UserAvatar = "/img/avatar9.png",
        //            Text = "Джоне, я згоден з твоїми зауваженнями! Особливо щодо масштабованості.",
        //            DateOfCreated = DateTime.Now.AddMinutes(-115),
        //            PostId = post.Id,
        //            ParentCommentId = level3Comments[1].Id, // Відповідь на john (рівень 3)
        //            IsRequired = true
        //        }
        //    };

        //            // Додаємо всі коментарі поста до загального списку
        //            allComments.AddRange(rootComments);
        //            allComments.AddRange(level2Comments);
        //            allComments.AddRange(level3Comments);
        //            allComments.AddRange(level4Comments);
        //        }

        //        // Зберігаємо всі коментарі в базу
        //        context.Database.OpenConnection();
        //        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Comments ON");

        //        context.Comments.AddRange(allComments);
        //        context.SaveChanges();

        //        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Comments OFF");
        //        context.Database.CloseConnection();

        //    }
        //}


        /// <summary>
        /// Заповнюємо таблицю PostTags початковими даними.
        /// </summary>
        private static void SeedPostTags(AgencyDBContext context)
        {
            if (!context.PostTags.Any())
            {
                var posts = context.Posts.ToList();
                var tags = context.Tags.ToList();

                var postTags = new List<PostTags>
        {
           new PostTags { PostId = posts[0].Id, TagId = tags[0].Id },  // ASP.NET Core - ASP.NET Core
            new PostTags { PostId = posts[0].Id, TagId = tags[1].Id },  // ASP.NET Core - Entity Framework
            new PostTags { PostId = posts[1].Id, TagId = tags[1].Id },  // Entity Framework - Entity Framework
            new PostTags { PostId = posts[1].Id, TagId = tags[3].Id },  // Entity Framework - SQL Server
            new PostTags { PostId = posts[2].Id, TagId = tags[0].Id },  // Web API - ASP.NET Core
            new PostTags { PostId = posts[2].Id, TagId = tags[2].Id },  // Web API - MVC
            new PostTags { PostId = posts[3].Id, TagId = tags[0].Id },  // Blazor - ASP.NET Core
            new PostTags { PostId = posts[4].Id, TagId = tags[3].Id },  // SQL Server - SQL Server
            new PostTags { PostId = posts[5].Id, TagId = tags[5].Id },  // JavaScript ES6 - JavaScript
            new PostTags { PostId = posts[5].Id, TagId = tags[6].Id },  // JavaScript ES6 - TypeScript
            new PostTags { PostId = posts[6].Id, TagId = tags[7].Id },  // React Hooks - React
            new PostTags { PostId = posts[7].Id, TagId = tags[12].Id }, // Docker - Docker
            new PostTags { PostId = posts[7].Id, TagId = tags[13].Id }, // Docker - Kubernetes
            new PostTags { PostId = posts[8].Id, TagId = tags[19].Id }, // Microservices - Microservices
            new PostTags { PostId = posts[9].Id, TagId = tags[10].Id }, // Python Django - Python
            new PostTags { PostId = posts[9].Id, TagId = tags[11].Id }, // Python Django - Django
            new PostTags { PostId = posts[10].Id, TagId = tags[4].Id }, // CSS Grid - Bootstrap
            new PostTags { PostId = posts[11].Id, TagId = tags[16].Id }, // Git - Git
            new PostTags { PostId = posts[12].Id, TagId = tags[14].Id }, // Azure - Azure
            new PostTags { PostId = posts[13].Id, TagId = tags[8].Id }  // Vue.js - Vue.js
        };

                context.PostTags.AddRange(postTags);
                Console.WriteLine("✅ PostTags table seeded successfully");
            }
        }

        private static void SeedPostCategories(AgencyDBContext context)
        {
            if (!context.PostCategories.Any())
            {
                var posts = context.Posts.ToList();
                var categories = context.Categories.ToList();

                var postCategories = new List<PostCategories>
        {
            new PostCategories { PostId = posts[0].Id, CategoryId = categories[0].Id },  // ASP.NET Core - Web Development
            new PostCategories { PostId = posts[1].Id, CategoryId = categories[0].Id },  // EF Core - Web Development
            new PostCategories { PostId = posts[1].Id, CategoryId = categories[3].Id },  // EF Core - Database Management
            new PostCategories { PostId = posts[2].Id, CategoryId = categories[0].Id },  // Web API - Web Development
            new PostCategories { PostId = posts[3].Id, CategoryId = categories[0].Id },  // Blazor - Web Development
            new PostCategories { PostId = posts[4].Id, CategoryId = categories[2].Id },  // SQL Server - Database Management
            new PostCategories { PostId = posts[5].Id, CategoryId = categories[0].Id },  // JavaScript - Web Development
            new PostCategories { PostId = posts[5].Id, CategoryId = categories[15].Id }, // JavaScript - Frontend Development
            new PostCategories { PostId = posts[6].Id, CategoryId = categories[0].Id },  // React - Web Development
            new PostCategories { PostId = posts[6].Id, CategoryId = categories[15].Id }, // React - Frontend Development
            new PostCategories { PostId = posts[7].Id, CategoryId = categories[4].Id },  // Docker - DevOps
            new PostCategories { PostId = posts[8].Id, CategoryId = categories[18].Id }, // Microservices - Microservices
            new PostCategories { PostId = posts[8].Id, CategoryId = categories[16].Id }, // Microservices - Backend Development
            new PostCategories { PostId = posts[9].Id, CategoryId = categories[0].Id },  // Python - Web Development
            new PostCategories { PostId = posts[10].Id, CategoryId = categories[10].Id }, // CSS Grid - UI/UX Design
            new PostCategories { PostId = posts[11].Id, CategoryId = categories[13].Id }, // Git - Project Management
            new PostCategories { PostId = posts[12].Id, CategoryId = categories[2].Id },  // Azure - Cloud Computing
            new PostCategories { PostId = posts[13].Id, CategoryId = categories[0].Id },  // Vue.js - Web Development
            new PostCategories { PostId = posts[14].Id, CategoryId = categories[11].Id }, // Database Design - Database Management
            new PostCategories { PostId = posts[15].Id, CategoryId = categories[14].Id }  // TypeScript - Programming Languages
        };

                context.PostCategories.AddRange(postCategories);
                Console.WriteLine("✅ PostCategories table seeded successfully");
            }
        }





    }
}