using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleAzureADAuthenticationWithAzureAd.Helpers;

namespace SampleAzureADAuthenticationWithAzureAd.Pages
{
    [AuthorizeGroups(Roles.ROLE_GROUP3)]
    public class Group3Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}