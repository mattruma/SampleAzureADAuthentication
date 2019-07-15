using Microsoft.AspNetCore.Mvc;

namespace SampleAzureADAuthenticationWithAzureAd.Helpers
{
    public class AuthorizeGroupsAttribute : TypeFilterAttribute
    {
        public AuthorizeGroupsAttribute(params string[] groups) : base(typeof(AuthorizeGroupsFilter))
        {
            Arguments = new object[] { groups };
        }
    }
}