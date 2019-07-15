using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleAzureADAuthenticationWithOpenIdConnect.Helpers;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Pages
{
    [AuthorizeRoles(Roles.ROLE_GROUP3)]
    public class Group3Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}