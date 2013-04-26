namespace BackgammonR.WebUI.Controllers
{
    using System.Web.Mvc;
    using System.Web.Security;
    using BackgammonR.WebUI.ViewModels;

    public class HomeController : Controller
    {
        public RedirectToRouteResult Index()
        {
            return RedirectToAction(Request.IsAuthenticated ? "Game" : "Login");
        }

        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(vm.Name, string.Empty))
                {
                    FormsAuthentication.SetAuthCookie(vm.Name, false);
                    return RedirectToAction("Game");
                }
            }

            ModelState.AddModelError("Name", "We already have someone playing with that name.");
            return View();
        }

        [Authorize]
        public ViewResult Game()
        {
            return View(new GameViewModel { Name = User.Identity.Name });
        }

        public RedirectToRouteResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
