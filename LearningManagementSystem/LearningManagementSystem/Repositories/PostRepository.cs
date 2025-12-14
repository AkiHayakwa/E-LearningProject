using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly LMSContext _context;

        public PostRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Post> GetAll()
        {
            return _context.Posts
                .Include(p => p.Topic)
                .Include(p => p.User)
                .Include(p => p.ParentPost)
                .Include(p => p.PostLikes)
                .Include(p => p.PostAttachments)
                .AsQueryable();
        }

        public Post? GetById(string id)
        {
            return _context.Posts
                .Include(p => p.Topic)
                .Include(p => p.User)
                .Include(p => p.ParentPost)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.User)
                .Include(p => p.PostLikes)
                .Include(p => p.PostAttachments)
                .FirstOrDefault(p => p.PostId == id);
        }

        public IQueryable<Post> GetByTopicId(string topicId)
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.ParentPost)
                .Include(p => p.PostLikes)
                .Include(p => p.PostAttachments)
                .Where(p => p.TopicId == topicId && p.IsActive && p.ParentPostId == null) // Chỉ lấy posts chính, không lấy replies
                .AsQueryable();
        }

        public IQueryable<Post> GetRepliesByPostId(string postId)
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.PostLikes)
                .Include(p => p.PostAttachments)
                .Where(p => p.ParentPostId == postId && p.IsActive)
                .AsQueryable();
        }

        public void Add(Post post)
        {
            _context.Posts.Add(post);
        }

        public void Update(Post post)
        {
            _context.Posts.Update(post);
        }

        public void Delete(string id)
        {
            var post = GetById(id);
            if (post != null)
            {
                post.IsActive = false;
                Update(post);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

