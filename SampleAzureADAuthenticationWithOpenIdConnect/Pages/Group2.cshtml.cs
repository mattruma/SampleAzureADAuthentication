using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleAzureADAuthenticationWithOpenIdConnect.Helpers;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Pages
{
    [AuthorizeRoles(Roles.ROLE_GROUP2)]
    public class Group2Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}