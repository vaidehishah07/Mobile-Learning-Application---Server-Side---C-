//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MlaWebApi
{
    using System;
    using System.Collections.Generic;
    
    public partial class student
    {
        public string idStudent { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailId { get; set; }
        public string telephone { get; set; }
        public string address { get; set; }
        public long userId { get; set; }
        public string aliasMailId { get; set; }
        public string skypeId { get; set; }
    
        public virtual register register { get; set; }
    }
}
