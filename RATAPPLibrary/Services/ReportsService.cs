﻿using RATAPPLibrary.Data.DbContexts;

//TODO - currently just a mock service to mess around with page a bit 
namespace RATAPPLibrary.Services
{
    public class ReportsService
    {
        private readonly RatAppDbContext _context;
        //ctor 
        public ReportsService(RatAppDbContext context)
        {
            _context = context;
        }
    }
}
