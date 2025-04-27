using System;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Animal_Management;
using RATAPPLibrary.Data.Models.Breeding;

namespace RATAPPLibraryUT
{
    public class TestBase
    {
        protected RatAppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RatAppDbContext(options);

            // Seed with test data
            SeedTestData(context);

            return context;
        }

        private void SeedTestData(RatAppDbContext context)
        {
            // Add test species
            var rat = new Species { Id = 1, CommonName = "Rat", ScientificName = "Rattus norvegicus" };
            var mouse = new Species { Id = 2, CommonName = "Mouse", ScientificName = "Mus musculus" };
            context.Species.AddRange(rat, mouse);

            // Add test projects
            var project1 = new Project { Id = 1, Name = "Test Project 1" };
            var project2 = new Project { Id = 2, Name = "Test Project 2" };
            context.Projects.AddRange(project1, project2);

            // Add test animals
            var dam1 = new Animal 
            { 
                Id = "DAM001", 
                Name = "Test Dam 1",
                Sex = "Female",
                SpeciesId = rat.Id,
                DateOfBirth = DateTime.Now.AddYears(-1)
            };
            var dam2 = new Animal 
            { 
                Id = "DAM002", 
                Name = "Test Dam 2",
                Sex = "Female",
                SpeciesId = rat.Id,
                DateOfBirth = DateTime.Now.AddYears(-1)
            };
            var sire1 = new Animal 
            { 
                Id = "SIRE001", 
                Name = "Test Sire 1",
                Sex = "Male",
                SpeciesId = rat.Id,
                DateOfBirth = DateTime.Now.AddYears(-1)
            };
            var sire2 = new Animal 
            { 
                Id = "SIRE002", 
                Name = "Test Sire 2",
                Sex = "Male",
                SpeciesId = rat.Id,
                DateOfBirth = DateTime.Now.AddYears(-1)
            };

            context.Animals.AddRange(dam1, dam2, sire1, sire2);

            // Add test pairings
            var pairing1 = new Pairing
            {
                Id = "PAIR001",
                DamId = dam1.Id,
                SireId = sire1.Id,
                ProjectId = project1.Id,
                PairingStartDate = DateTime.Now.AddDays(-30)
            };
            var pairing2 = new Pairing
            {
                Id = "PAIR002",
                DamId = dam2.Id,
                SireId = sire2.Id,
                ProjectId = project2.Id,
                PairingStartDate = DateTime.Now.AddDays(-20)
            };

            context.Pairings.AddRange(pairing1, pairing2);

            context.SaveChanges();
        }

        protected void CleanupContext(RatAppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
