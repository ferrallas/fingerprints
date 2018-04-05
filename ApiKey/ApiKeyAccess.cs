using System;

namespace ApiKey
{
    public class ApiKeyAccess
    {
        public string Id { get; set; }

        public string User { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ServerSecret { get; set; }

        public ulong Use { get; set; }
    }
}
