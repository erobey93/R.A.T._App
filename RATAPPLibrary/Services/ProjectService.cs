using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing breeding projects.
    /// Handles operations for Project entities and their relationships.
    /// </summary>
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(string name, int lineId, string? description = null);
        Task<Project> GetProjectByIdAsync(int id);
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsByLineAsync(int lineId);
        Task<Project> UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(int id);
        Task<Project> GetDefaultProjectAsync();
    }

    public class ProjectService : IProjectService
    {
        private readonly RatAppDbContext _context;
        private readonly LineService _lineService;

        public ProjectService(RatAppDbContext context)
        {
            _context = context;
            _lineService = new LineService(context);
        }

        /// <summary>
        /// Creates a new breeding project.
        /// </summary>
        public async Task<Project> CreateProjectAsync(string name, int lineId, string? description = null)
        {
            // Validate line exists
            var line = await _lineService.GetLineAsync_ById(lineId);
            if (line == null)
            {
                throw new InvalidOperationException($"Line with ID {lineId} not found.");
            }

            // Check if project name already exists for this line
            var existingProject = await _context.Project
                .FirstOrDefaultAsync(p => p.Name == name && p.LineId == lineId);
            if (existingProject != null)
            {
                throw new InvalidOperationException($"Project with name '{name}' already exists for this line.");
            }

            var project = new Project
            {
                Name = name,
                LineId = lineId,
                Description = description,
                CreatedOn = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.Project.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        /// <summary>
        /// Retrieves a project by its ID.
        /// </summary>
        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _context.Project
                .Include(p => p.Line)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                throw new KeyNotFoundException($"Project with ID {id} not found.");
            }

            return project;
        }

        /// <summary>
        /// Retrieves all breeding projects.
        /// </summary>
        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Project
                .Include(p => p.Line)
                .OrderByDescending(p => p.LastUpdated)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all projects for a specific breeding line.
        /// </summary>
        public async Task<IEnumerable<Project>> GetProjectsByLineAsync(int lineId)
        {
            return await _context.Project
                .Include(p => p.Line)
                .Where(p => p.LineId == lineId)
                .OrderByDescending(p => p.LastUpdated)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing project's information.
        /// </summary>
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Project.FindAsync(project.Id);
            if (existingProject == null)
            {
                throw new KeyNotFoundException($"Project with ID {project.Id} not found.");
            }

            // Validate line exists if it's being changed
            if (existingProject.LineId != project.LineId)
            {
                var line = await _lineService.GetLineAsync_ById(project.LineId);
                if (line == null)
                {
                    throw new InvalidOperationException($"Line with ID {project.LineId} not found.");
                }
            }

            existingProject.Name = project.Name;
            existingProject.LineId = project.LineId;
            existingProject.Description = project.Description;
            existingProject.Notes = project.Notes;
            existingProject.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProject;
        }

        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        public async Task DeleteProjectAsync(int id)
        {
            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project with ID {id} not found.");
            }

            // Check if this is the default project
            if (await IsDefaultProject(id))
            {
                throw new InvalidOperationException("Cannot delete the default project.");
            }

            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets or creates the default project.
        /// The default project is used when no specific project is specified for pairings.
        /// </summary>
        public async Task<Project> GetDefaultProjectAsync()
        {
            var defaultProject = await _context.Project
                .FirstOrDefaultAsync(p => p.Name == "Default Project");

            if (defaultProject == null)
            {
                // Get or create a default line
                var defaultLine = await _lineService.GetOrCreateLineAsync_ByName(1); // Assuming 1 is your default line ID

                defaultProject = new Project
                {
                    Name = "Default Project",
                    LineId = defaultLine.Id,
                    Description = "Default project for unassigned pairings",
                    CreatedOn = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                _context.Project.Add(defaultProject);
                await _context.SaveChangesAsync();
            }

            return defaultProject;
        }

        private async Task<bool> IsDefaultProject(int projectId)
        {
            var defaultProject = await GetDefaultProjectAsync();
            return defaultProject.Id == projectId;
        }
    }
}
