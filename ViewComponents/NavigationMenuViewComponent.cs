using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Models;

namespace WebApp.ViewComponents
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly NavigationModel _navigationModel;

        public NavigationMenuViewComponent(AgencyDBContext agencyDBContext)
        {
            _navigationModel = new NavigationModel(agencyDBContext);
        }

        public IViewComponentResult Invoke()
        {
            var navigationTree = _navigationModel.GetNavigationTree();  
            return View("NavigationMenu", navigationTree);
        }

    }






    //public class NavigationMenuViewComponent : ViewComponent
    //{
    //    private List<Navigate> _navigateThree;

    //    public NavigationMenuViewComponent()
    //    {
    //        _navigateThree = new List<Navigate>();
    //        _navigateThree.Add(new Navigate() 
    //        { 
    //            Id = 1, 
    //            Title = "Главная", 
    //            Href = "/", 
    //            Order = 1, 
    //            ParentID = null 
    //        });

    //        Navigate about = new Navigate() 
    //        { 
    //            Id = 2, 
    //            Title = "About", 
    //            Href = "/about/index", 
    //            Order = 2, 
    //            ParentID = null 
    //        };
    //        //Navigate contactus = new Navigate() 
    //        //{ 
    //        //    Id = 3, 
    //        //    Title = "Contac", 
    //        //    Href = "/about/contactus", 
    //        //    ParentID = 2, 
    //        //    Order = 1 
    //        //};

    //        //about.Childs.Add(contactus);


    //        _navigateThree.Add(about);


    //        Navigate services = new Navigate()
    //        {
    //            Id = 5,
    //            Title = "Services",
    //            Href = "/Services/index",
    //            ParentID = null,
    //            Order = 1
    //        };
    //        _navigateThree.Add(services);

    //        Navigate blog = new Navigate()
    //        {
    //            Id = 6,
    //            Title = "Blog",
    //            Href = "#",
    //            ParentID = null,
    //            Order = 1
    //        };

    //        Navigate blogGridIndex = new Navigate()
    //        {
    //            Id = 7,
    //            Title = "Blog Grid",
    //            Href = "/Blog/BlogGridIndex",
    //            ParentID = 6,
    //            Order = 1
    //        };
    //        blog.Childs.Add(blogGridIndex);

    //        Navigate blogDetailIndex = new Navigate()
    //        {
    //            Id = 8,
    //            Title = "Blog Detail",
    //            Href = "/Blog/BlogDetailIndex",
    //            ParentID = 6,
    //            Order = 2
    //        };
    //        blog.Childs.Add(blogDetailIndex);
    //        _navigateThree.Add(blog);



    //        Navigate pages = new Navigate()
    //        {
    //            Id = 9,
    //            Title = "Pages",
    //            Href = "#",
    //            ParentID = null,
    //            Order = 1
    //        };

    //        Navigate freeQuote = new Navigate()
    //        {
    //            Id = 10,
    //            Title = "Free Quote",
    //            Href = "/Pages/FreeQuote",
    //            ParentID = 10,
    //            Order = 1
    //        };
    //        pages.Childs.Add(freeQuote);

    //        Navigate ourFeatures = new Navigate()
    //        {
    //            Id = 11,
    //            Title = "Our Features",
    //            Href = "/Pages/OurFeatures",
    //            ParentID = 10,
    //            Order = 2
    //        };
    //        pages.Childs.Add(ourFeatures);

    //        Navigate pricingPlan = new Navigate()
    //        {
    //            Id = 12,
    //            Title = "Pricing Plan",
    //            Href = "/Pages/PricingPlan",
    //            ParentID = 10,
    //            Order = 3
    //        };
    //        pages.Childs.Add(pricingPlan);

    //        Navigate teamMembers = new Navigate()
    //        {
    //            Id = 12,
    //            Title = "Team Members",
    //            Href = "/Pages/TeamMembers",
    //            ParentID = 10,
    //            Order = 4
    //        };
    //        pages.Childs.Add(teamMembers);

    //        Navigate testimonial = new Navigate()
    //        {
    //            Id = 12,
    //            Title = "Testimonial",
    //            Href = "/Pages/Testimonial",
    //            ParentID = 10,
    //            Order = 5
    //        };
    //        pages.Childs.Add(testimonial);
    //        _navigateThree.Add(pages);

    //        Navigate contact = new Navigate()
    //        {
    //            Id = 13,
    //            Title = "Contact",
    //            Href = "/about/contactus",
    //            ParentID = null,
    //            Order = 1
    //        };
    //        _navigateThree.Add(contact);
    //    }

    //    public IViewComponentResult Invoke()
    //    {
    //        return View("NavigationMenu", _navigateThree);
    //    }
    //}
}
