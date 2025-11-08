using Microsoft.AspNetCore.Mvc;
using WebApp.Entities;
using System.Collections.Generic;

namespace WebApp.ViewComponents
{
    public class FooterQuickLinksViewComponent : ViewComponent
    {
        private List<FooterQuickLinks> _footerQuickLinks;

        public FooterQuickLinksViewComponent()
        {
            _footerQuickLinks = new List<FooterQuickLinks>
        {
            new FooterQuickLinks
            {
                Title = "Главная",
                Href = "/",
                Order = 1
            },
            new FooterQuickLinks
            {
                Title = "About us",
                Href = "/about/index",
                Order = 2
            },
            new FooterQuickLinks
            {
                Title = "Our Services",
                Href = "/Services/index",
                Order = 3
            },
            new FooterQuickLinks
            {
                Title = "Latest Blog",
                Href = "/",
                Order = 4
            },
            new FooterQuickLinks
            {
                Title = "Contact us",
                Href = "/about/contactus",
                Order = 5
            }
        };
        }

        public IViewComponentResult Invoke()
        {
            return View("FooterQuickLinks", _footerQuickLinks);
        }
    }
}
