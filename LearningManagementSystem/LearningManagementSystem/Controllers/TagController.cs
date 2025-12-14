using System;
using System.Linq;
using System.Globalization;
using System.Text;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TagController : Controller
    {
        private readonly LMSContext _context;

        public TagController(LMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tags = await _context.Tags
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();
            return View("~/Views/Tag/Index.cshtml", tags);
        }

        public IActionResult Create()
        {
            return View("~/Views/Tag/Create.cshtml", new Tag());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tag model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Tag/Create.cshtml", model);
            }

            model.TagId = Guid.NewGuid().ToString("N").Substring(0, 10);
            model.Slug = await EnsureUniqueSlugAsync(model.Name);
            model.CreatedAt = DateTime.UtcNow;
            model.CreatedBy = User.Identity?.Name;

            _context.Tags.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã tạo tag mới.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            return View("~/Views/Tag/Edit.cshtml", tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Tag model)
        {
            if (id != model.TagId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Tag/Edit.cshtml", model);
            }

            var tag = await _context.Tags
                .Include(t => t.CourseTags)
                .FirstOrDefaultAsync(t => t.TagId == id);
            if (tag == null) return NotFound();

            tag.Name = model.Name;
            tag.Category = model.Category;
            tag.IsActive = model.IsActive;
            tag.Slug = await EnsureUniqueSlugAsync(model.Name, tag.TagId);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật tag.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Tag không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var tag = await _context.Tags
                .Include(t => t.CourseTags)
                .FirstOrDefaultAsync(t => t.TagId == id);
            if (tag == null)
            {
                TempData["Error"] = "Không tìm thấy tag.";
                return RedirectToAction(nameof(Index));
            }

            if (tag.CourseTags.Any())
            {
                TempData["Error"] = "Không thể xóa tag đang được sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa tag.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> EnsureUniqueSlugAsync(string text, string? excludeId = null)
        {
            var baseSlug = GenerateSlug(text);
            var slug = baseSlug;
            var counter = 1;
            while (await _context.Tags.AnyAsync(t => t.Slug == slug && t.TagId != excludeId))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }

        private static string GenerateSlug(string phrase)
        {
            phrase = phrase.Trim().ToLowerInvariant();
            var normalized = phrase.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            var cleaned = sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "d");

            var slug = new string(cleaned.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
            slug = System.Text.RegularExpressions.Regex.Replace(slug, "-{2,}", "-").Trim('-');
            return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N").Substring(0, 8) : slug;
        }
    }
}

