﻿using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;
        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new();

                if(companyId != null)
                {
                    company = await _context.Companies
                                            .Include(c => c.Projects)
                                            .Include(c => c.Invites)
                                            .Include(c => c.Members)
                                            .FirstOrDefaultAsync(c => c.Id == companyId);
                }

                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<Invite>> GetInvitesAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                List<BTUser>? members = new();
                members = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
                return members;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Project>> GetProjectsAsync(int? companyId)
        {
            try
            {
                List<Project>? projects = new();
                projects = await _context.Projects.Where(p => p.CompanyId == companyId).ToListAsync();
                return projects;
            }
            catch 
            {
                throw;
            }
        }
    }
}
