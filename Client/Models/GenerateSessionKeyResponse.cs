using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class GenerateSessionKeyResponse
    {
        public byte[] PublicEcdfKey { get; set; }
        public byte[] IV { get; set; }
    }
}
