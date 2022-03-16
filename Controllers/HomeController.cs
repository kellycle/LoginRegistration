using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LoginRegistration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace LoginRegistration.Controllers
{
    public class HomeController : Controller
    {
        private LoginRegistrationContext db;
        public HomeController(LoginRegistrationContext context)
        {
            db = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            // Refactor: Return redirect if user is session and tries to access login/registration page

            return View("Index");
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            // Check initial ModelState
            if(ModelState.IsValid)
            {
                // If a User exists with provided email
                if(db.Users.Any(u => u.Email == newUser.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                // Save your user object to the database
                db.Add(newUser);
                db.SaveChanges();

                // Refactor: Instead of saving the entire User into session, just save UserId and User name from newUser

                User retrievedUser = db.Users.FirstOrDefault(user => user.UserId == newUser.UserId);

                HttpContext.Session.SetObjectAsJson("currentUser", retrievedUser);

                User currentUser = HttpContext.Session.GetObjectFromJson<User>("currentUser");

                Console.WriteLine(currentUser.FirstName);

                return RedirectToAction("Success");
            } else {
                return View("Index");
            }
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            User currentUser = HttpContext.Session.GetObjectFromJson<User>("currentUser");

            if(currentUser != null)
            {
                return View("Success");
            } else {
                return RedirectToAction("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If initial ModelState is valid, query for a user with provided email
                var userInDb = db.Users.FirstOrDefault(u => u.Email == userSubmission.loginEmail);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("loginEmail", "Email doesn't exist");
                    return View("Index");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // Refactor: Avoid using var, hover over var to find the data type

                // Verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.loginPassword);

                // Result can be compared to 0 for failure
                if(result == 0)
                {
                    // Handle failure (this should be similar to how "existing email" is handled)
                    // Manually add a ModelState error to the Email field, with provided error message
                    ModelState.AddModelError("loginEmail", "Invalid email/password");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                User retrievedUser = db.Users.FirstOrDefault(user => user.UserId == userInDb.UserId);

                HttpContext.Session.SetObjectAsJson("currentUser", userInDb);

                User currentUser = HttpContext.Session.GetObjectFromJson<User>("currentUser");

                Console.WriteLine(currentUser.FirstName);

                return RedirectToAction("Success");
            } else {
                return View("Index");
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
