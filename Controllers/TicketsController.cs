using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services.Interfaces;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using BugTracker.Models.Enums;
using BugTracker.Services;
using System.IO;
using BugTracker.Extensions;
using X.PagedList;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;


        public TicketsController(ApplicationDbContext context, IBTProjectService projectService, IBTRolesService rolesService, IBTTicketService ticketService, IBTFileService fileService, UserManager<BTUser> userManager, IBTTicketHistoryService historyService, IBTNotificationService notificationService)
        {
            _context = context;
            _projectService = projectService;
            _rolesService = rolesService;
            _ticketService = ticketService;
            _fileService = fileService;
            _userManager = userManager;
            _historyService = historyService;
            _notificationService = notificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 9;
            int page = pageNum ?? 1;

            IPagedList<Ticket> tickets = await (await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId)).ToPagedListAsync(page, pageSize);
            ViewData["ActionName"] = nameof(Index);


            return View(tickets);

        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignTicket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AssignTicketViewModel viewModel = new();

            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            string? currentDeveloper = viewModel.Ticket?.DeveloperUserId;
            viewModel.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(viewModel.Ticket?.ProjectId, nameof(BTRoles.Developer), _companyId), "Id", "FullName", currentDeveloper);


            return View(viewModel);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(AssignTicketViewModel viewModel)
        {
            string? currentUserId = _userManager.GetUserId(User);

            if (viewModel.DeveloperId != null && viewModel.Ticket?.Id != null)
            {

                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId);

                try
                {
                    await _ticketService.AssignTicketAsync(viewModel.Ticket?.Id, viewModel.DeveloperId);
                }
                catch (Exception)
                {

                    throw;
                }

                //TODO: Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUserId);


                //TODO: Add Notification
                await _notificationService.NewDeveloperNotificationAsync(viewModel.Ticket?.Id, viewModel.DeveloperId, currentUserId);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ProjectId"] = new SelectList((await _projectService.GetAllProjectsByCompanyIdAsync(_companyId)), "Id", "Name");
            ViewData["TicketPriority"] = new SelectList((await _ticketService.GetTicketPrioritiesAsync()), "Id", "Name");
            ViewData["TicketType"] = new SelectList((await _ticketService.GetTicketTypesAsync()), "Id", "Name");
            
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId")] Ticket ticket)
        {
            string? currentUserId = _userManager.GetUserId(User);
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                ticket.SubmitterUserId = currentUserId;

                await _ticketService.AddTicketAsync(ticket);

                //Add History Record
                int companyId = User.Identity!.GetCompanyId();
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null!, newTicket, currentUserId);

                //Notify
                await _notificationService.NewTicketNotificationAsync(ticket.Id, currentUserId);

                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriority"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketType"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/AddTicketAttachment
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			string statusMessage;

            ModelState.Remove("BTUserId");

			if (ModelState.IsValid && ticketAttachment.FormFile != null)
			{
				ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
				ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
				ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.Created = DateTimeOffset.Now;
				ticketAttachment.BTUserId = _userManager.GetUserId(User);

				await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
				statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}

			return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
		}
        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string? fileName = ticketAttachment?.FileName;
            byte[]? fileData = ticketAttachment?.FileData;
            string? ext = Path.GetExtension(fileName)?.Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Archived = true;

            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Undelete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Undelete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Archived = false;

            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }

        //// POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Tickets == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
        //    }
        //    var ticket = await _context.Tickets.FindAsync(id);
        //    if (ticket != null)
        //    {
        //        _context.Tickets.Remove(ticket);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
