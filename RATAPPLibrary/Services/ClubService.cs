using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing breeding clubs and breeder-club relationships in the R.A.T. App.
    /// Handles the creation, management, and association of breeding clubs and their members.
    /// 
    /// Key Features:
    /// - Club Management:
    ///   * Create and maintain breeding clubs
    ///   * Track club memberships
    ///   * Manage breeder associations
    /// 
    /// Data Structure:
    /// - Club: Represents a breeding organization
    /// - BreederClub: Many-to-many relationship between breeders and clubs
    /// 
    /// Relationships:
    /// - One breeder can belong to multiple clubs
    /// - One club can have multiple breeders
    /// - All relationships tracked through BreederClub entity
    /// 
    /// Known Limitations:
    /// - No club roles/permissions
    /// - Basic club metadata only
    /// - No club event tracking
    /// - Not using BaseService pattern
    /// 
    /// Planned Improvements:
    /// - Implement BaseService pattern
    /// - Add club roles and permissions
    /// - Add club event management
    /// - Support club communication
    /// - Add club resource sharing
    /// 
    /// Dependencies:
    /// - RatAppDbContext for data access
    /// - Will integrate with BaseService (planned)
    /// </summary>
    public interface IClubService //: BaseService TODO use new BaseService + context factory pattern 
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
        /// Creates a new breeding club with the specified name.
        /// 
        /// Process:
        /// 1. Validates name is unique
        /// 2. Creates club record
        /// 3. Saves to database
        /// 
        /// Validation:
        /// - Checks for existing club with same name
        /// - Name cannot be empty/null (enforced by model)
        /// 
        /// Throws:
        /// - InvalidOperationException if club name exists
        /// </summary>
        /// <param name="name">Name for the new club</param>
        /// <returns>Created Club object</returns>
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
        /// Associates a breeder with a club as a member.
        /// 
        /// Process:
        /// 1. Checks if association already exists
        /// 2. Creates new BreederClub record if not
        /// 3. Saves to database
        /// 
        /// Note:
        /// - Silent success if association exists
        /// - No validation of breeder/club existence
        /// 
        /// TODO:
        /// - Add breeder/club existence validation
        /// - Add role/permission support
        /// - Add membership metadata (join date, etc.)
        /// </summary>
        /// <param name="breederId">ID of breeder to add</param>
        /// <param name="clubId">ID of club to add to</param>
        /// <returns>True if association created, false if already exists</returns>
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
