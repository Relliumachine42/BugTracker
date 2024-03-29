﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using BugTracker.Models.Enums;
using BugTracker.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using BugTracker.Helpers;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTFileService fileService, IBTTicketService ticketService, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _ticketService = ticketService;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        // GET: Projects
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 9;
            int page = pageNum ?? 1;

            IPagedList<Project> projects = await (await _projectService.GetAllProjectsByCompanyIdAsync(_companyId)).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(Index);

            return View(projects);

        }

        public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<Project> projects = await _projectService.SearchProjects(searchString).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(SearchIndex);
            ViewData["SearchString"] = searchString;

            return View(nameof(Index), projects);
        }

        // GET: Projects/AssignPM
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            // Get the list of project managers
            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), _companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);

            AssignPMViewModel viewModel = new()
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id,
            };

            return View(viewModel);
        }

        // POST: Projects/AssignPM
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                if (await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.ProjectId))
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.ProjectId });
                }
                else
                {
                    ModelState.AddModelError("PMId", "Error assigning PM.");

                }
            //ModelState.AddModelError("PMId", "No Project Manager chosen. Please select a PM.");

            }


            // Get the list of project managers
            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), _companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.ProjectId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);

            return View(viewModel);

        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        { 
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), _companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);
            List<BTUser> usersList = submitters.Concat(developers).ToList();

            IEnumerable<string> currentMembers = project.Members.Select(u => u.Id);

            ProjectMembersViewModel viewModel = new()
            {
                Project = project,
                UsersList = new MultiSelectList(usersList, "Id", "FullName", currentMembers)
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            if (viewModel.SelectedMembers != null)
            {
                await _projectService.RemoveMembersFromProjectAsync(viewModel.Project?.Id, _companyId);

                await _projectService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel.Project?.Id, _companyId);

                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            }

            //Handle the error
            ModelState.AddModelError("SelectedMembers", "No Users chosen. Please select Users.");

            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project?.Id, _companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), _companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);
            List<BTUser> usersList = submitters.Concat(developers).ToList();

            IEnumerable<string> currentMembers = viewModel.Project!.Members.Select(u => u.Id);
            viewModel.UsersList = new MultiSelectList(usersList, "Id", "FullName", currentMembers);


            return View(viewModel);
        }





        // GET: Projects/Details/5
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectAsync(slug, _companyId);

            if (project == null)
            {
                return NotFound($"No project found with Name {slug}");
            }

            return View(project);
        }






        // GET: Projects/Create
        // [Authorize(Policy = "Admin|PM")]     ----> same as [Authorize(Roles="Admin, ProjectManager")]
		[Authorize(Roles = "Admin, ProjectManager")]
		public async Task<IActionResult> Create()
        {
            //ViewData["ProjectPriority"] = new SelectList(Enum.GetValues(typeof(BTProjectPriorities)).Cast<BTProjectPriorities>());
            //ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");

            ViewData["ProjectPriority"] = new SelectList((await _projectService.GetProjectPrioritiesAsync()), "Id", "Name");

            return View(new Project() { StartDate = DateTime.Now, EndDate = DateTime.Now });

        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ProjectPriorityId,Archived,ImageFile")] Project project)
        {
            ModelState.Remove("CompanyId");
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                string? newSlug = StringHelper.BTSlug(project.Name);

                if(!await _projectService.ValidSlugAsync(newSlug, project.Id))
                {
                    ModelState.AddModelError("Name", "A similar Name is already in use.");

                    ViewData["ProjectPriority"] = new SelectList((await _projectService.GetProjectPrioritiesAsync()), "Id", "Name");
                    return View(project);
                }

                project.Slug = newSlug;
                project.CompanyId = _companyId;
                project.Created = DateTime.Now;

                if (project.ImageFile != null)
                {
                    project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFile);
                    project.ImageType = project.ImageFile.ContentType;
                }

                await _projectService.AddProjectAsync(project);

                return RedirectToAction(nameof(Index));
            }


            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Slug,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,Archived,ImageData,ImageType,ImageFile")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string? newSlug = StringHelper.BTSlug(project.Name);

                    if (newSlug != project.Slug)
                    {
                        if (!await _projectService.ValidSlugAsync(newSlug, project.Id))
                        {
                            ModelState.AddModelError("Name", "A similar Name/Slug is already in use.");

                            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
                            return View(project);
                        }

                        project.Slug = newSlug;
                    }


                    if (project.ImageFile != null)
                    {
                        project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFile);
                        project.ImageType = project.ImageFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(project);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("Name", "ModelState is invalid.");
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            project.Archived = true;

            await _projectService.UpdateProjectAsync(project);

            foreach (Ticket ticket in project.Tickets)
            {
                ticket.ArchivedByProject = true;
                await _ticketService.UpdateTicketAsync(ticket);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Undelete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Undelete(int? id)
		{
			if (id == null || _context.Projects == null)
			{
				return NotFound();
			}

			Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

			if (project == null)
			{
				return NotFound();
			}

			project.Archived = false;

			await _projectService.UpdateProjectAsync(project);

			foreach (Ticket ticket in project.Tickets)
			{
				ticket.ArchivedByProject = false;
				await _ticketService.UpdateTicketAsync(ticket);
			}

			return RedirectToAction(nameof(Index));
		}

		//// POST: Projects/Delete/5
		//[HttpPost, ActionName("Delete")]
  //      [ValidateAntiForgeryToken]
  //      public async Task<IActionResult> DeleteConfirmed(int id)
  //      {
  //          if (_context.Projects == null)
  //          {
  //              return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
  //          }
  //          var project = await _context.Projects.FindAsync(id);
  //          if (project != null)
  //          {
  //              _context.Projects.Remove(project);
  //          }

  //          await _context.SaveChangesAsync();
  //          return RedirectToAction(nameof(Index));
  //      }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
