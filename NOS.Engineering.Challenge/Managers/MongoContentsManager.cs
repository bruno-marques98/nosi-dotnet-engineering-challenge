using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace NOS.Engineering.Challenge.Managers
{
    private readonly MongoDBContext _context;

    public MongoContentsManager(MongoDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Content>> GetManyContents()
    {
        return await _context.Contents.Find(_ => true).ToListAsync();
    }

    public async Task<Content> GetContent(Guid id)
    {
        return await _context.Contents.Find(content => content.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Content> CreateContent(ContentDto contentDto)
    {
        var content = new Content(contentDto);
        await _context.Contents.InsertOneAsync(content);
        return content;
    }

    public async Task<Content> UpdateContent(Guid id, ContentDto contentDto)
    {
        var content = new Content(contentDto);
        var result = await _context.Contents.ReplaceOneAsync(c => c.Id == id, content);
        return result.IsAcknowledged ? content : null;
    }

    public async Task<Guid?> DeleteContent(Guid id)
    {
        var result = await _context.Contents.DeleteOneAsync(content => content.Id == id);
        return result.IsAcknowledged ? id : (Guid?)null;
    }
}
