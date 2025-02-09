//using Microsoft.AspNetCore.Mvc;
//using RATAPPLibrary.Data.Models;
//using RATAPPLibrary.Services;

//namespace RATWEB.Server.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly RATAPPLibrary.Data.DbContexts.UserDbContext _context;
//        private readonly PasswordHashing _passwordHashing;

//        public AccountController(RATAPPLibrary.Data.DbContexts.UserDbContext context, PasswordHashing passwordHashing)
//        {
//            _context = context;
//            _passwordHashing = passwordHashing;
//        }

//        // GET: /Account/Login
//        public IActionResult Login()
//        {
//            return View();
//        }

//        // POST: /Account/Login
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginRequest model)
//        {
//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // Perform login logic using your existing service
//                    var loginService = new LoginService(_context, _passwordHashing);
//                    var response = await loginService.Login(model);

//                    // Set the user session
//                    HttpContext.Session.SetString("Username", response.Username);
//                    HttpContext.Session.SetString("Role", response.Role);

//                    // Redirect to different dashboards based on role
//                    if (response.Role == "Admin")
//                    {
//                        return RedirectToAction("AdminDashboard", "Dashboard");
//                    }
//                    else if (response.Role == "User")
//                    {
//                        return RedirectToAction("UserDashboard", "Dashboard");
//                    }
//                }
//                catch (UnauthorizedAccessException)
//                {
//                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
//                }
//                catch (Exception ex)
//                {
//                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
//                }
//            }
//            return View(model);
//        }

//        public IActionResult Logout()
//        {
//            // Clear the session
//            HttpContext.Session.Clear();
//            return RedirectToAction("Login");
//        }
//    }
//}
