using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;

//TODO still working on structure, but for now I'm using services to handle the logic of creating new entities
//TODO I'm not sure if this is the best way to do this, but I'm trying to keep the controllers as clean as possible
namespace RATAPPLibrary.Services
{
    public class LineService
    {
        private readonly AnimalDbContext _context;

        public LineService(AnimalDbContext context)
        {
            _context = context;
        }

        public async Task<Line> GetOrCreateLineAsync(string variety)
        {
            // Find the correct LineId based on the variety
            var line = await _context.Lines.FirstOrDefaultAsync(l => l.Name == variety);

            if (line == null)
            {
                // Create a new Line if it doesn't exist
                line = new Line
                {
                    Name = variety
                };

                _context.Lines.Add(line);
                await _context.SaveChangesAsync();
            }

            return line;
        }
    }
}
