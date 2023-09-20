using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Build.Evaluation;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        private DateTime _inviteDate;
        private DateTime _joinDate;
        public int Id { get; set; }
        public DateTime InviteDate
        {
            get { return _inviteDate; }
            set { _inviteDate = value.ToUniversalTime(); }
        }
        public DateTime JoinDate
        {
            get { return _joinDate; }
            set { _joinDate = value.ToUniversalTime(); }
        }
        public Guid CompanyToken { get; set; }
        public int CompanyId { get; set; }
        public int? ProjectId { get; set; }
        [Required]
        public string? InvitorId { get; set; }
        public string? InviteeId { get; set; }
        [Required]
        public string? InviteeEmail { get; set; }
        [Required]
        public string? InviteeFirstName { get; set; }
        [Required]
        public string? InviteeLastName { get; set; }
        public string? Message { get; set; }
        public bool IsValid { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Invitor { get; set; }
        public virtual BTUser? Invitee { get; set; }

    }
}
