using BugTracker.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        private DateTimeOffset _created;
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }
        public int TicketId { get; set; }
        [Required]
        public string? BTUserId { get; set; }
        public string? FileName { get; set; }

        //File Properites
        public byte[]? FileData { get; set; }
        public string? FileType { get; set; }
        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? BTUser { get; set; }
    }
}
