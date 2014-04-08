using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReFind.BusinessLayer
{
    /// <summary>
    /// Page class
    /// </summary>
    public class Page
    {
        public int Id { get; set; }

        /// <summary>
        /// Page Number
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Page Content
        /// </summary>
        public string Content { get; set; }
    }
}
