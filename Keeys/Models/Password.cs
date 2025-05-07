using System;

namespace Keeys.Models
{
    public class Password
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string EncryptedPassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}