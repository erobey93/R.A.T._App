using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;

//TODO still working on structure, but for now I'm using services to handle the logic of creating new entities
//TODO I'm not sure if this is the best way to do this, but I'm trying to keep the controllers as clean as possible
namespace RATAPPLibrary.Services
{
    public class LineService
    {
        private readonly RatAppDbContext _context;

        public LineService(RatAppDbContext context)
        {
            _context = context;
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
    }
}
