using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Common
{
    public abstract class BaseEntity  
    {
        public Guid Id { get; set; } = Guid.NewGuid();
         
        public bool IsDeleted { get; set; } = false;
    }
}