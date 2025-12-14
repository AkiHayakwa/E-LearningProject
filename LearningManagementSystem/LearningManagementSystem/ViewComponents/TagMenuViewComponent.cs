using LearningManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.ViewComponents
{
    public class TagMenuViewComponent : ViewComponent
    {
        private readonly LMSContext _context;

        public TagMenuViewComponent(LMSContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string template = "Default")
        {
            var tags = await _context.Tags
                .AsNoTracking()
                .Where(t => t.IsActive)
                .ToListAsync();

            var groups = tags
                .GroupBy(t => t.Category ?? "Khác")
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(t => t.Name).ToList());

            return View(template, groups);
        }
    }
}

