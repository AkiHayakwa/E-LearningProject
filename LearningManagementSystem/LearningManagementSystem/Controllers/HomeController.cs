using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly LMSContext _context;
        private readonly ICourseRepository _courseRepository;

        public HomeController(LMSContext context, ICourseRepository courseRepository)
        {
            _context = context;
            _courseRepository = courseRepository;
        }

        public IActionResult Index()
        {
            var userName = User.Identity.Name;
            User user = null;
            if (!string.IsNullOrEmpty(userName))
            {
                user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            }

            var courses = _courseRepository.GetAll().ToList();

            var viewModel = new HomeViewModel
            {
                User = user,
                Courses = courses
            };

            return View(viewModel);
        }
    }
}