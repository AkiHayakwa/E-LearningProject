using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly LMSContext _context;

        public ReportRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Report> GetAll()
        {
            return _context.Reports
                .Include(r => r.Post)
                .Include(r => r.Topic)
                .Include(r => r.ReportedByUser)
                .Include(r => r.ResolvedByUser)
                .AsQueryable();
        }

        public Report? GetById(string id)
        {
            return _context.Reports
                .Include(r => r.Post)
                .Include(r => r.Topic)
                .Include(r => r.ReportedByUser)
                .Include(r => r.ResolvedByUser)
                .FirstOrDefault(r => r.ReportId == id);
        }

        public IQueryable<Report> GetByStatus(string status)
        {
            return _context.Reports
                .Include(r => r.Post)
                .Include(r => r.Topic)
                .Include(r => r.ReportedByUser)
                .Include(r => r.ResolvedByUser)
                .Where(r => r.Status == status)
                .AsQueryable();
        }

        public void Add(Report report)
        {
            _context.Reports.Add(report);
        }

        public void Update(Report report)
        {
            _context.Reports.Update(report);
        }

        public void Delete(string id)
        {
            var report = GetById(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

