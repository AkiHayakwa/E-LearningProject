using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly LMSContext _context;

        public TopicRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Topic> GetAll()
        {
            return _context.Topics
                .Include(t => t.Forum)
                .Include(t => t.User)
                .Include(t => t.Posts)
                .Include(t => t.TopicTags)
                .AsQueryable();
        }

        public Topic? GetById(string id)
        {
            return _context.Topics
                .Include(t => t.Forum)
                .Include(t => t.User)
                .Include(t => t.Posts)
                    .ThenInclude(p => p.User)
                .Include(t => t.Posts)
                    .ThenInclude(p => p.PostLikes)
                .Include(t => t.Posts)
                    .ThenInclude(p => p.PostAttachments)
                .Include(t => t.TopicTags)
                .FirstOrDefault(t => t.TopicId == id);
        }

        public IQueryable<Topic> GetByForumId(string forumId)
        {
            return _context.Topics
                .Include(t => t.Forum)
                .Include(t => t.User)
                .Include(t => t.Posts)
                .Where(t => t.ForumId == forumId && t.IsActive)
                .AsQueryable();
        }

        public IQueryable<Topic> GetByUserName(string userName)
        {
            return _context.Topics
                .Include(t => t.Forum)
                .Include(t => t.User)
                .Where(t => t.UserName == userName && t.IsActive)
                .AsQueryable();
        }

        public void Add(Topic topic)
        {
            _context.Topics.Add(topic);
        }

        public void Update(Topic topic)
        {
            _context.Topics.Update(topic);
        }

        public void Delete(string id)
        {
            var topic = GetById(id);
            if (topic != null)
            {
                topic.IsActive = false;
                Update(topic);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

