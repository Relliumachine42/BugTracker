using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BugTracker.Models
{
    public class Ticket
    {
        private DateTime _created;
        private DateTime? _updated;
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }
        public DateTime? Updated
        {
            get => _updated;
            set
            {
                if (value.HasValue)
                {
                    _updated = value.Value.ToUniversalTime();
                }
                else
                {
                    _updated = null;
                }
            }
        }
        public bool Archived { get; set; }
        public bool ArchivedByProject { get; set;}
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityId { get; set; }
        public string? DeveloperUserId { get; set; }
        [Required]
        public string? SubmitterUserId { get; set; }

        // Navigation Properties
        public virtual Project? Project { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();

    }
}
