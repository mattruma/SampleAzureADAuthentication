using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleAzureADAuthenticationWithAzureAd.Helpers;

namespace SampleAzureADAuthenticationWithAzureAd.Pages
{
    [AuthorizeGroups(Roles.ROLE_GROUP1)]
    public class Group1Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}