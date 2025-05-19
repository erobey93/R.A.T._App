using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing breeding research projects in the R.A.T. App.
    /// Handles the organization and tracking of breeding programs, including
    /// project metadata, line associations, and breeding records.
    /// 
    /// Key Features:
    /// - Project Management:
    ///   * Create and track breeding projects
    ///   * Associate projects with specific lines
    ///   * Maintain project metadata and notes
    /// 
    /// Special Features:
    /// - Default Project Support:
    ///   * Automatic creation of default project
    ///   * Fallback for unassigned pairings
    ///   * Protected from deletion
    /// 
    /// Data Structure:
    /// - Projects contain:
    ///   * Name and description
    ///   * Line association
    ///   * Creation and update timestamps
    ///   * Optional notes and metadata
    /// 
    /// Known Limitations:
    /// - No support for project hierarchies
    /// - Limited project status tracking
    /// - Basic metadata support only
    /// 
    /// Dependencies:
    /// - LineService: For line validation and default line creation
    /// - Inherits from BaseService for database operations
    /// </summary>
    public interface IProjectService 
    {
        Task<Project> CreateProjectAsync(string name, int lineId, string? description = null);
        Task<Project> GetProjectByIdAsync(int id);
        Task<List<Project>> GetAllProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsByLineAsync(int lineId);
        Task<Project> UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(int id);
        Task<Project> GetDefaultProjectAsync();
    }

    public class ProjectService : BaseService, IProjectService
    {
        //private readonly RatAppDbContext _context;
        private readonly LineService _lineService;

        public ProjectService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
            _lineService = new LineService(contextFactory);
        }

        /// <summary>
        /// Creates a new breeding project with the specified details.
        /// 
        /// Process:
        /// 1. Validates line exists
        /// 2. Checks for duplicate project names within line
        /// 3. Creates project with timestamps
        /// 
        /// Required Fields:
        /// - name: Project identifier
        /// - lineId: Associated breeding line
        /// 
        /// Optional:
        /// - description: Project details
        /// 
        /// Throws:
        /// - InvalidOperationException if line not found or project name exists
        /// </summary>
        public async Task<Project> CreateProjectAsync(string name, int lineId, string? description = null)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            }); 
        }

        /// <summary>
        /// Retrieves a project by its ID.
        /// </summary>
        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var project = await _context.Project
                .Include(p => p.Line)
                .Include(p => p.Line.Stock)
                .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    throw new KeyNotFoundException($"Project with ID {id} not found.");
                }

                return project;
            }); 
        }

        /// <summary>
        /// Retrieves all breeding projects.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Project
                .Include(p => p.Line)
                .Include(p => p.Line.Stock)
                .Include(p => p.Line.Animals)
                .Include(p => p.Line.Stock.Species)
                .OrderByDescending(p => p.LastUpdated)
                .ToListAsync();
            });
        }

        /// <summary>
        /// Retrieves all projects for a specific breeding line.
        /// </summary>
        public async Task<IEnumerable<Project>> GetProjectsByLineAsync(int lineId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Project
                .Include(p => p.Line)
                .Include(p => p.Line.Stock)
                .Include(p => p.Line.Animals)
                 .Include(p => p.Line.Stock.Species)
                .Where(p => p.LineId == lineId)
                .OrderByDescending(p => p.LastUpdated)
                .ToListAsync();
            });
        }

        /// <summary>
        /// Updates an existing project's information.
        /// </summary>
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            }); 
        }

        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        public async Task DeleteProjectAsync(int id)
        {
            await ExecuteInContextAsync(async _context =>
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
            }); 
        }

        /// <summary>
        /// Gets or creates the default project for unassigned pairings.
        /// 
        /// Purpose:
        /// - Provides a fallback project for breeding pairs
        /// - Ensures all pairings have a project association
        /// - Maintains data integrity
        /// 
        /// Process:
        /// 1. Looks for existing default project
        /// 2. If not found:
        ///    - Creates/gets default line
        ///    - Creates default project
        ///    - Sets standard metadata
        /// 
        /// Note: The default project is protected from deletion
        /// and should be used sparingly, mainly as a temporary
        /// assignment until a proper project is created.
        /// </summary>
        public async Task<Project> GetDefaultProjectAsync()
        {
            return await ExecuteInContextAsync(async _context =>
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
            });
        }

        private async Task<bool> IsDefaultProject(int projectId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var defaultProject = await GetDefaultProjectAsync();
                return defaultProject.Id == projectId;
            }); 
        }
    }
}
