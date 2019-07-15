using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleAzureADAuthenticationWithAzureAd.Helpers;

namespace SampleAzureADAuthenticationWithAzureAd.Pages
{
    [AuthorizeGroups(Roles.ROLE_GROUP2)]
    public class Group2Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}