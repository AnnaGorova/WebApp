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
                    new Navigate { Title = "Contact", Href = "/contact", Order = 6, ParentID = null }
        

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
                    new Navigate { Title = "Blog Detail", Href = "/Blog/BlogGridIndex", Order = 2, ParentID = blogNavigate.Id }
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
    }
}