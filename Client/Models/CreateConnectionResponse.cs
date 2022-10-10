using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    internal class CreateConnectionResponse
    {
        public string Message { get; set; }
        public GenerateSessionKeyResponse? SessionKeyResponse { get; set; }
    }
}
