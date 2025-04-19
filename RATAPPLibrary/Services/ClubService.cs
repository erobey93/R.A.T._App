using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing breeding clubs and breeder-club relationships.
    /// Handles operations for Club and BreederClub entities.
    /// </summary>
    public interface IClubService
    {
        Task<Club> CreateClubAsync(string name);
        Task<Club> GetClubByIdAsync(int id);
        Task<IEnumerable<Club>> GetAllClubsAsync();
        Task<Club> UpdateClubAsync(Club club);
        Task DeleteClubAsync(int id);
        Task<bool> AddBreederToClubAsync(int breederId, int clubId);
        Task<bool> RemoveBreederFromClubAsync(int breederId, int clubId);
        Task<IEnumerable<Club>> GetClubsByBreederAsync(int breederId);
        Task<IEnumerable<Breeder>> GetBreedersByClubAsync(int clubId);
    }

    public class ClubService : IClubService
    {
        private readonly RatAppDbContext _context;

        public ClubService(RatAppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new breeding club.
        /// </summary>
        public async Task<Club> CreateClubAsync(string name)
        {
            var existingClub = await _context.Club.FirstOrDefaultAsync(c => c.Name == name);
            if (existingClub != null)
            {
                throw new InvalidOperationException($"Club with name '{name}' already exists.");
            }

            var club = new Club { Name = name };
            _context.Club.Add(club);
            await _context.SaveChangesAsync();
            return club;
        }

        /// <summary>
        /// Retrieves a club by its ID.
        /// </summary>
        public async Task<Club> GetClubByIdAsync(int id)
        {
            var club = await _context.Club
                .Include(c => c.BreederClubs)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
            {
                throw new KeyNotFoundException($"Club with ID {id} not found.");
            }

            return club;
        }

        /// <summary>
        /// Retrieves all breeding clubs.
        /// </summary>
        public async Task<IEnumerable<Club>> GetAllClubsAsync()
        {
            return await _context.Club
                .Include(c => c.BreederClubs)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing club's information.
        /// </summary>
        public async Task<Club> UpdateClubAsync(Club club)
        {
            var existingClub = await _context.Club.FindAsync(club.Id);
            if (existingClub == null)
            {
                throw new KeyNotFoundException($"Club with ID {club.Id} not found.");
            }

            existingClub.Name = club.Name;
            await _context.SaveChangesAsync();
            return existingClub;
        }

        /// <summary>
        /// Deletes a club by its ID.
        /// </summary>
        public async Task DeleteClubAsync(int id)
        {
            var club = await _context.Club.FindAsync(id);
            if (club == null)
            {
                throw new KeyNotFoundException($"Club with ID {id} not found.");
            }

            _context.Club.Remove(club);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Associates a breeder with a club.
        /// </summary>
        public async Task<bool> AddBreederToClubAsync(int breederId, int clubId)
        {
            var existingAssociation = await _context.BreederClub
                .FirstOrDefaultAsync(bc => bc.BreederId == breederId && bc.ClubId == clubId);

            if (existingAssociation != null)
            {
                return false; // Association already exists
            }

            var breederClub = new BreederClub
            {
                BreederId = breederId,
                ClubId = clubId,
            };

            _context.BreederClub.Add(breederClub);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a breeder's association with a club.
        /// </summary>
        public async Task<bool> RemoveBreederFromClubAsync(int breederId, int clubId)
        {
            var association = await _context.BreederClub
                .FirstOrDefaultAsync(bc => bc.BreederId == breederId && bc.ClubId == clubId);

            if (association == null)
            {
                return false; // Association doesn't exist
            }

            _context.BreederClub.Remove(association);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all clubs that a breeder is a member of.
        /// </summary>
        public async Task<IEnumerable<Club>> GetClubsByBreederAsync(int breederId)
        {
            return await _context.BreederClub
                .Where(bc => bc.BreederId == breederId)
                .Select(bc => bc.Club)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all breeders that are members of a club.
        /// </summary>
        public async Task<IEnumerable<Breeder>> GetBreedersByClubAsync(int clubId)
        {
            return await _context.BreederClub
                .Where(bc => bc.ClubId == clubId)
                .Select(bc => bc.Breeder)
                .ToListAsync();
        }
    }
}
