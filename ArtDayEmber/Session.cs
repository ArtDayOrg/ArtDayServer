//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArtDayEmber
{
    using System;
    using System.Collections.Generic;
    
    public partial class Session
    {
        public Session()
        {
            this.Preferences = new HashSet<Preference>();
            this.Enrollments = new HashSet<Enrollment>();
        }
    
        public int id { get; set; }
        public string sessionName { get; set; }
        public string instructorName { get; set; }
        public int capacity { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public string imageUrl { get; set; }
        public string instructions { get; set; }
    
        public virtual ICollection<Preference> Preferences { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
