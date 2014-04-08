using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ReFind.BusinessLayer
{
    public abstract class Document
    {
        /// <summary>
        /// Document Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique document hash tag
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Document page collection see <seealso cref="Page" />
        /// </summary>
        private List<Page> _pages = new List<Page>();

        /// <summary>
        /// Contains document metadata
        /// </summary>
        private Metadata _metadata;

        /// <summary>
        /// CTOR
        /// </summary>
        public Document()
        {
            _metadata = new Metadata();
        }

        /// <summary>
        /// Add a page to the document
        /// </summary>
        /// <param name="page"></param>
        public void AddPage(Page page)
        {
            _pages.Add(page);
        }

        /// <summary>
        /// Get a specific page from the document
        /// </summary>
        /// <param name="pageNumber">Specify page number</param>
        /// <returns>returns Page</returns>
        public Page GetPage(int pageNumber)
        {
            return 
                _pages.First(p => p.Number == pageNumber);
        }

        /// <summary>
        /// Get a range of Pages from the document
        /// </summary>
        /// <param name="startPage">Start page number</param>
        /// <param name="endPage">End page number</param>
        /// <returns>Returns list of pages</returns>
        public IEnumerable<Page> GetPageRange(int startPage, int endPage)
        {
            return
                _pages.FindAll(p => p.Number >= startPage && p.Number <= endPage);
        }

        /// <summary>
        /// Returns all pages in the document
        /// </summary>
        public IEnumerable<Page> Pages
        {
            get { return _pages; }
        }

        /// <summary>
        /// Returns number of pages loaded in the document.
        /// </summary>
        public int TotalPages
        {
            get { return _pages.Count; }
        }
    }
}
