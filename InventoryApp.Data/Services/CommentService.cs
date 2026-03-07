using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class CommentService
    {
        private readonly ApplicationDbContext _db;

        public CommentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Comment>> GetByInventoryAsync(int inventoryId)
        {
            return await _db.Comments
                .Include(c => c.Author)
                .Where(c => c.InventoryId == inventoryId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new Comment
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    InventoryId = c.InventoryId,
                    AuthorId = c.AuthorId,
                    Author = c.Author
                })
                .ToListAsync();
        }

        public async Task<Comment> AddCommentAsync(
            int inventoryId, string authorId, string content)
        {
            var comment = new Comment
            {
                Content = content,
                InventoryId = inventoryId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return comment;
        }
    }
}