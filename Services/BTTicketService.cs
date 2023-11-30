using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTicketAsync(Ticket? ticket)
        {
            try
            {

                if(ticket != null)
                {
                ticket!.Created = DateTime.Now;
                ticket!.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == "New"))!.Id;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                }

                return;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
				await _context.AddAsync(ticketAttachment);
				await _context.SaveChangesAsync();
			}
            catch (Exception)
            {

                throw;
            }
        }

        public Task AddTicketCommentAsync(TicketComment? ticketComment)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveTicketAsync(Ticket? ticket)
        {
            throw new NotImplementedException();
        }

        public async Task AssignTicketAsync(int? ticketId, string? userId)
        {
            try
            {
                if (ticketId != null && !string.IsNullOrEmpty(userId))
                {
                    BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    Ticket? ticket = await GetTicketByIdAsync(ticketId, btUser!.CompanyId);

                    if (ticket != null && btUser != null) 
                    {
                        ticket!.DeveloperUserId = userId;
                        // TODO: Set ticket Status to "Development" with LookupService

                        await UpdateTicketAsync(ticket);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {

                List<Ticket> tickets = await _context.Tickets
                                                            .Where(t => t.Project!.CompanyId == companyId)
                                                            .Include(t => t.Project)
                                                            .Include(t => t.TicketPriority)
                                                            .Include(t => t.TicketType)
                                                            .Include(t => t.TicketStatus)
                                                            .Include(t => t.DeveloperUser)
                                                            .Include(t => t.SubmitterUser)
                                                            .Include(t => t.Comments)
                                                            .Include(t => t.Attachments)
                                                            .Include(t => t.History)
                                                            .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = new();
                if (ticketId != null && companyId != null)
                {
                    ticket = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                        .Include(t => t.Project)
                            .ThenInclude(p => p!.Company)
                        .Include(t => t.Attachments)
                        .Include(t => t.Comments)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.History)
                        .Include(t => t.SubmitterUser)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.TicketStatus)
                        .Include(t => t.TicketType)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == ticketId);

                }
                return ticket!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int? ticketAttachmentId)
        {
            {
                try
                {
                    TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                      .Include(t => t.BTUser)
                                                                      .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                    return ticketAttachment;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = new();

                if (ticketId != null && companyId != null)
                {
                    ticket = await _context.Tickets
                                                    .Where(t => t.Project!.CompanyId == companyId)
                                                    .Include(t => t.Project)
                                                        .ThenInclude(p => p!.Company)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.History)
                                                    .Include(t => t.SubmitterUser)
                                                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                }
                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<BTUser?> GetTicketDeveloperAsync(int? ticketId, int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                IEnumerable<TicketPriority> ticketPriorities = new List<TicketPriority>();


                ticketPriorities = await _context.TicketPriorities.ToListAsync();

                return ticketPriorities;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public Task<List<Ticket>> GetTicketsByUserIdAsync(string? userId, int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                IEnumerable<TicketType> ticketTypes = new List<TicketType>();


                ticketTypes = await _context.TicketTypes.ToListAsync();

                return ticketTypes;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task RestoreTicketAsync(Ticket? ticket)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateTicketAsync(Ticket? ticket)
        {
            try
            {
                if (ticket != null)
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
