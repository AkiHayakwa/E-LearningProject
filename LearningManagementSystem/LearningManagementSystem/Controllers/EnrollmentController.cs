using LearningManagementSystem.Data;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly LMSContext _context;

        public EnrollmentController(IEnrollmentRepository enrollmentRepository, LMSContext context)
        {
            _enrollmentRepository = enrollmentRepository;
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollments = _enrollmentRepository.GetEnrollmentsByUserId(userId);
            return View(enrollments);
        }

        [HttpPost]
        public IActionResult Unenroll(string enrollmentId)
        {
            var enrollment = _enrollmentRepository.GetById(enrollmentId);
            if (enrollment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (enrollment.UserId != userId) return Forbid();

            _enrollmentRepository.Delete(enrollment);
            _context.SaveChanges();

            TempData["Success"] = "Hủy đăng ký thành công!";
            return RedirectToAction("Index");
        }
    }
}
