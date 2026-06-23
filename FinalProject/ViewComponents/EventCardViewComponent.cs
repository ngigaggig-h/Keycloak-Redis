using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.ViewComponents;

public class EventCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(HomeEventCardViewModel eventCard)
    {
        return View(eventCard);
    }
}
