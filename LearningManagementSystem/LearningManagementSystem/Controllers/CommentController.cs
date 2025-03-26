using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;

[Authorize(Roles = "Admin")]
public class CommentController : Controller
{
    private readonly LMSContext _context;

    public CommentController(LMSContext context)
    {
        _context = context;
    }

    // GET: Comment/ManageComments
    public IActionResult ManageComments()
    {
        var comments = _context.Comments.ToList();
        return View(comments);
    }

    // GET: Comment/CreateComment
    public IActionResult CreateComment()
    {
        return View();
    }

    // POST: Comment/CreateComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateComment(Comment model)
    {
        if (ModelState.IsValid)
        {
            model.CommentId = Guid.NewGuid().ToString();
            model.CreatedDate = DateTime.Now;
            model.UserName = User.Identity.Name; // Gán UserName của người dùng hiện tại
            _context.Comments.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Thêm bình luận thành công.";
            return RedirectToAction("ManageComments");
        }
        return View(model);
    }

    // GET: Comment/EditComment/{id}
    public IActionResult EditComment(string id)
    {
        var comment = _context.Comments.FirstOrDefault(c => c.CommentId == id);
        if (comment == null)
        {
            return NotFound();
        }
        return View(comment);
    }

    // POST: Comment/EditComment/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditComment(string id, Comment model)
    {
        if (id != model.CommentId)
        {
            return NotFound();
        }

        var comment = _context.Comments.FirstOrDefault(c => c.CommentId == id);
        if (comment == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            comment.Content = model.Content;
            comment.CourseId = model.CourseId;
            _context.Update(comment);
            _context.SaveChanges();
            TempData["Success"] = "Chỉnh sửa bình luận thành công.";
            return RedirectToAction("ManageComments");
        }
        return View(model);
    }

    // POST: Comment/DeleteComment/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteComment(string id)
    {
        var comment = _context.Comments.FirstOrDefault(c => c.CommentId == id);
        if (comment == null)
        {
            TempData["Error"] = "Bình luận không tồn tại.";
            return RedirectToAction("ManageComments");
        }

        _context.Comments.Remove(comment);
        _context.SaveChanges();
        TempData["Success"] = "Xóa bình luận thành công.";
        return RedirectToAction("ManageComments");
    }
}