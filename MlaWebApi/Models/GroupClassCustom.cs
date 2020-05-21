using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MlaWebApi.Models
{
    public class GroupClassCustom
    {
        public string groupName { get; set; }
        public int OwnerId { get; set; }
        public int userId { get; set; }
        public string groupKey { get; set; }
        public bool staus { get; set; }
        public int groupType { get; set; }
        public int groupId { get; set; }
    }
}