# Getting Started

This is a sample application that demonstrates how to leverage Active Directory for authentication and user security groups for authorization.

There are two applications, one that leverages `AzureAd` as the default authentication scheme, and the other that leverages `OpenIdConnect`.

## Setup

Register your application with Azure Active Directory.

<https://docs.microsoft.com/en-us/skype-sdk/trusted-application-api/docs/registrationinazureactivedirectory>

Configure your application to send the groups as claims.

<https://github.com/Azure-Samples/active-directory-dotnet-webapp-groupclaims#step-3-configure-your-application-to-receive-group-claims>

## Notes

The security groups are returned with your GUID values, and NOT their name, which is one of the reasons I created constants, made my code more readable.
