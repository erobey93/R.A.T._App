using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Research;

namespace RATAPPLibrary.Services
{
    public class ResearchService
    {
        private readonly RatAppDbContext _context;

        public ResearchService(RatAppDbContext context)
        {
            _context = context;
        }

        // Study Management
        public async Task<Study> CreateStudyAsync(Study study)
        {
            _context.Studies.Add(study);
            await _context.SaveChangesAsync();
            return study;
        }

        public async Task<Study> GetStudyAsync(int id)
        {
            return await _context.Studies
                .Include(s => s.StudyGroups)
                .Include(s => s.DataPoints)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Study>> GetStudiesAsync()
        {
            return await _context.Studies
                .Include(s => s.Researcher)
                .ToListAsync();
        }

        public async Task UpdateStudyAsync(Study study)
        {
            _context.Studies.Update(study);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStudyAsync(int id)
        {
            var study = await _context.Studies.FindAsync(id);
            if (study != null)
            {
                _context.Studies.Remove(study);
                await _context.SaveChangesAsync();
            }
        }

        // Study Group Management
        public async Task<StudyGroup> AddStudyGroupAsync(StudyGroup group)
        {
            _context.StudyGroups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<StudyGroup> GetStudyGroupAsync(int id)
        {
            return await _context.StudyGroups
                .Include(sg => sg.StudyAnimals)
                .ThenInclude(sa => sa.Animal)
                .FirstOrDefaultAsync(sg => sg.Id == id);
        }

        // Animal Management in Studies
        public async Task<StudyAnimal> AddAnimalToStudyGroupAsync(StudyAnimal studyAnimal)
        {
            _context.StudyAnimals.Add(studyAnimal);
            await _context.SaveChangesAsync();
            return studyAnimal;
        }

        public async Task RemoveAnimalFromStudyGroupAsync(int studyAnimalId)
        {
            var studyAnimal = await _context.StudyAnimals.FindAsync(studyAnimalId);
            if (studyAnimal != null)
            {
                _context.StudyAnimals.Remove(studyAnimal);
                await _context.SaveChangesAsync();
            }
        }

        // Data Point Management
        public async Task<DataPoint> AddDataPointAsync(DataPoint dataPoint)
        {
            _context.DataPoints.Add(dataPoint);
            await _context.SaveChangesAsync();
            return dataPoint;
        }

        public async Task<List<DataPoint>> GetStudyDataPointsAsync(int studyId)
        {
            return await _context.DataPoints
                .Where(dp => dp.StudyId == studyId)
                .ToListAsync();
        }

        // Observation Data Management
        public async Task<ObservationData> AddObservationAsync(ObservationData observation)
        {
            _context.ObservationData.Add(observation);
            await _context.SaveChangesAsync();
            return observation;
        }

        public async Task<List<ObservationData>> GetAnimalObservationsAsync(int studyAnimalId)
        {
            return await _context.ObservationData
                .Include(od => od.DataPoint)
                .Where(od => od.StudyAnimalId == studyAnimalId)
                .OrderByDescending(od => od.Timestamp)
                .ToListAsync();
        }

        // Analysis Methods
        public async Task<Dictionary<string, StudyStats>> GetStudyStatsAsync(int studyId, int dataPointId)
        {
            var observations = await _context.ObservationData
                .Where(od => od.DataPoint.StudyId == studyId && od.DataPointId == dataPointId)
                .ToListAsync();

            var groupedStats = new Dictionary<string, StudyStats>();

            // Get the data point to determine its type
            var dataPoint = await _context.DataPoints.FindAsync(dataPointId);
            if (dataPoint?.Type == DataType.Number)
            {
                var numericValues = observations
                    .Select(o => double.TryParse(o.Value, out double val) ? val : double.NaN)
                    .Where(v => !double.IsNaN(v))
                    .ToList();

                if (numericValues.Any())
                {
                    groupedStats["overall"] = new StudyStats
                    {
                        Average = numericValues.Average(),
                        Minimum = numericValues.Min(),
                        Maximum = numericValues.Max(),
                        Count = numericValues.Count
                    };
                }
            }
            else
            {
                groupedStats["overall"] = new StudyStats
                {
                    Count = observations.Count
                };
            }

            return groupedStats;
        }
    }
}
