using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MlaWebApi.Models
{
    public class EncryptionForFriends
    {
        public int friendId { get; set; }
        public int userId { get; set; }
        public string groupKey { get; set; }
    }
}