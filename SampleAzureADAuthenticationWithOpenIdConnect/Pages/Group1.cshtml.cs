using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleAzureADAuthenticationWithOpenIdConnect.Helpers;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Pages
{
    [AuthorizeRoles(Roles.ROLE_GROUP1)]
    public class Group1Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}