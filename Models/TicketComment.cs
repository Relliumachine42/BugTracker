using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketComment
    {
        private DateTime _created;
        public int Id { get; set; }
        [Required]
        public string? Comment { get; set; }
        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }
        public int TicketId { get; set; }
        public string? UserId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
