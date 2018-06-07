using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using SampleOwinApplication.Models;
using Sustainsys.Saml2.Owin;
using Sustainsys.Saml2.Configuration;
using System.IdentityModel.Metadata;
using System.Globalization;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2;
using Sustainsys.Saml2.WebSso;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Configuration;

namespace SampleOwinApplication
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseSaml2Authentication(CreateSaml2Options());
        }

        private static Saml2AuthenticationOptions CreateSaml2Options()
        {
            var spOptions = CreateSPOptions("arxspan");
            var Saml2Options = new Saml2AuthenticationOptions(false)
            {
                SPOptions = spOptions,
                AuthenticationType = "arxspan",
                Caption = "arxspan"
            };

            var idp = new IdentityProvider(new EntityId("http://www.okta.com/exkez48ebtXNSGr3g0h7"), spOptions)
                {
                    AllowUnsolicitedAuthnResponse = true,
                    Binding = Saml2BindingType.HttpRedirect,
                    SingleSignOnServiceUrl = new Uri("https://dev-871818.oktapreview.com/app/beldev871818_arxspansaml_1/exkez48ebtXNSGr3g0h7/sso/saml")
                };

            idp.SigningKeys.AddConfiguredKey(
                new X509Certificate2(
                    HostingEnvironment.MapPath(
                        "~/secure/okta.cert")));
            new Federation("http://localhost:52071/Federation", true, Saml2Options);

            Saml2Options.IdentityProviders.Add(idp);            

            var spOptions2 = CreateSPOptions("belatrix");
            var Saml2Options2 = new Saml2AuthenticationOptions(false)
            {
                SPOptions = spOptions2,
                AuthenticationType = "belatrix",
                Caption = "belatrix"
            };

            var idp2 = new IdentityProvider(new EntityId("https://aax0038.my.centrify.com/ce0d8092-49bf-4e73-8306-5a5b2c2eb39c"), spOptions)
            {
                AllowUnsolicitedAuthnResponse = true,
                Binding = Saml2BindingType.HttpRedirect,
                SingleSignOnServiceUrl = new Uri("https://aax0038.my.centrify.com/applogin/appKey/ce0d8092-49bf-4e73-8306-5a5b2c2eb39c/customerId/AAX0038")
            };

            idp2.SigningKeys.AddConfiguredKey(
                new X509Certificate2(
                    HostingEnvironment.MapPath(
                        "~/App_Data/centrify.cert")));

            Saml2Options2.IdentityProviders.Add(idp2);

            // It's enough to just create the federation and associate it
            // with the options. The federation will load the metadata and
            // update the options with any identity providers found.
            new Federation("http://localhost:52071/ Federation", true, Saml2Options2);

            return Saml2Options;
        }

        private static SPOptions CreateSPOptions(string optionName)
        {
            var spOptions = new SPOptions
            {
                EntityId = new EntityId(ConfigurationManager.AppSettings["saml:baseurl"] + "/Saml2"),
                ReturnUrl = new Uri(ConfigurationManager.AppSettings["saml:baseurl"] + "/Account/ExternalLoginCallback"),
                ModulePath = string.Format("/{0}", optionName)
            };

            var techContact = new ContactPerson
            {
                Type = ContactType.Technical
            };
            techContact.EmailAddresses.Add("arxspantestmail@gmail.com");
            spOptions.Contacts.Add(techContact);

            var supportContact = new ContactPerson
            {
                Type = ContactType.Support
            };
            supportContact.EmailAddresses.Add("support@arxspan.com");
            spOptions.Contacts.Add(supportContact);

            var attributeConsumingService = new AttributeConsumingService(optionName)
            {
                IsDefault = true,
            };

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("urn:Email")
                {
                    FriendlyName = "Email",
                    IsRequired = true,
                    NameFormat = RequestedAttribute.AttributeNameFormatUri
                });

            spOptions.AttributeConsumingServices.Add(attributeConsumingService);

            spOptions.ServiceCertificates.Add(new X509Certificate2(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/App_Data/Sustainsys.Saml2.Tests.pfx", string.Empty,
                                X509KeyStorageFlags.MachineKeySet));

            var adsa = "fdsafdsa";

            return spOptions;
        }


    }
}