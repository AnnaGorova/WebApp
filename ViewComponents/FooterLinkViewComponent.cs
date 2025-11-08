using Microsoft.AspNetCore.Mvc;
using WebApp.Entities;
using System.Collections.Generic;

namespace WebApp.ViewComponents
{
    public class FooterLinkViewComponent : ViewComponent
    {
        private List<FooterLink> _footerLinks;

        public FooterLinkViewComponent()
        {
            _footerLinks = new List<FooterLink>
            {
                new FooterLink
                {
                    Title = "Home",
                    Href = "/",
                    Order = 1
                },
                new FooterLink
                {
                    Title = "About us",
                    Href = "/about/index",
                    Order = 2
                },
                new FooterLink
                {
                    Title = "Our Services",
                    Href = "/Services/index",
                    Order = 3
                },
                new FooterLink
                {
                    Title = "Latest Blog",
                    Href = "/",
                    Order = 4
                },
                new FooterLink
                {
                    Title = "Contact us",
                    Href = "/about/contactus",
                    Order = 5
                }
            };
        }

        public IViewComponentResult Invoke()
        {
            return View("FooterLinks", _footerLinks);
        }
    }
}
