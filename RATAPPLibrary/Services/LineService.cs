using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

//TODO still working on structure, but for now I'm using services to handle the logic of creating new entities
//TODO I'm not sure if this is the best way to do this, but I'm trying to keep the controllers as clean as possible
namespace RATAPPLibrary.Services
{
    public class LineService : BaseService
    {
        private readonly RatAppDbContext _context;

        public LineService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
        }

        //get line by name is a unique case because if it doesn't exist, it should be created 
        //since we know the species (if we're assuming stock is organized by species)
        //and we have the name of the line, so that's all that's needed 
        public async Task<Line> GetOrCreateLineAsync_ByName(int lineId)
        {
            // Find the correct LineId based on the variety
            var line = await _context.Line.FirstOrDefaultAsync(l => l.Id == lineId);

            if (line == null)
            {
                // Create a new Line if it doesn't exist
                line = new Line
                {
                    Name = "TODO",
                    StockId = 2 //TODO these should have some automatic way of being set unless the user is allowed to set them 
                };

                _context.Line.Add(line);
                await _context.SaveChangesAsync();
            }

            return line;
        }

        public async Task<Line> GetLineAsync_ById(int id)
        {
            // Find the correct LineId based on the variety
            var line = await _context.Line.FirstOrDefaultAsync(l => l.Id == id);

            if (line == null)
            {
                // return error message - line not found 
                throw new Exception("Line not found");
            }

            return line;
        }


        // Get all lines
        public async Task<List<Line>> GetAllLinesAsync()
        {
            return await _context.Line.ToListAsync();
        }

        // Create a new line
        public async Task<Line> CreateLineAsync(Line line)
        {
            _context.Line.Add(line);
            await _context.SaveChangesAsync();
            return line;
        }

        // Update an existing line
        public async Task<Line> UpdateLineAsync(Line line)
        {
            var existingLine = await _context.Line.FindAsync(line.Id);

            if (existingLine == null)
            {
                throw new Exception("Line not found");
            }

            _context.Entry(existingLine).CurrentValues.SetValues(line);
            await _context.SaveChangesAsync();

            return line;
        }

        // Delete a line by ID
        public async Task DeleteLineAsync(int id)
        {
            var line = await _context.Line.FindAsync(id);

            if (line == null)
            {
                throw new Exception("Line not found");
            }

            _context.Line.Remove(line);
            await _context.SaveChangesAsync();
        }
    }
}
