using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow.Auctioner.DataStorage
{
    [Serializable]
    public class Attendee
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}
