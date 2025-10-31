using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

}
