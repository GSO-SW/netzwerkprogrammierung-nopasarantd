using System.Collections.Generic;

namespace NoPasaranMS.Model
{
    public class Lobby
    {
        public List<User> Users { get; }
        public User HostUser { get; set; }

        public Lobby(User host)
        {
            Users = new List<User>
            {
                (HostUser = host)
            };
        }
    }
}
