using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VOST.TwitterWatcher.Pages
{
    /// <summary>
    /// Error page
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// The request id that this error refers.
        /// </summary>
        /// <value>The request id.</value>
        public string RequestId { get; set; }

        /// <summary>
        /// Whether to show the request id.
        /// </summary>
        /// <returns><c>true</c> if it should show the request id, <c>false</c> otherwise.</returns>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// OnGet page event
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
