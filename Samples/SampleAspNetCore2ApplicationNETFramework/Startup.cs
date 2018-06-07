using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleAspNetCore2ApplicationNETFramework.Data;
using SampleAspNetCore2ApplicationNETFramework.Services;
using System.IdentityModel.Metadata;
using Sustainsys.Saml2;
using System.Security.Cryptography.X509Certificates;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using Microsoft.AspNetCore.Hosting.Internal;
using Sustainsys.Saml2.WebSso;
using System.IO;

namespace SampleAspNetCore2ApplicationNETFramework
{
    public class Startup
    {
        IHostingEnvironment _hostingEnvironment;
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            _hostingEnvironment = environment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();


            //services.AddAuthentication("saml1").AddScheme<SimpleOptions, SimpleAuthHandler>("saml1", op =>
            // {
            //     op.options.SPOptions.EntityId = new EntityId("https://localhost:44342/Saml2");
            //     op.options.IdentityProviders.Add(
            //         new IdentityProvider(
            //             new EntityId("http://localhost:52071/Metadata"), op.options.SPOptions)
            //         {
            //             LoadMetadata = true
            //         });

            //     op.options.SPOptions.ServiceCertificates.Add(new X509Certificate2("Sustainsys.Saml2.Tests.pfx"));
            // });

           

            services.AddAuthentication()
                .AddSaml2("pharmalab","pharmalab",options =>
                {                    
                    options.SPOptions = CreateSPOptions("pharmalab");

                    var idp = new IdentityProvider(new EntityId("http://www.okta.com/exkez48ebtXNSGr3g0h7"), options.SPOptions)
                    {
                        AllowUnsolicitedAuthnResponse = true,
                        Binding = Saml2BindingType.HttpRedirect,
                        SingleSignOnServiceUrl = new Uri("https://dev-871818.oktapreview.com/app/beldev871818_arxspansaml_1/exkez48ebtXNSGr3g0h7/sso/saml")
                    };
                    idp.SigningKeys.AddConfiguredKey(
                        new X509Certificate2("okta.cert"));
                    new Federation("https://localhost:44342/Federation", true, options);

                    options.IdentityProviders.Add(idp);

                    //options.SPOptions.ServiceCertificates.Add(new X509Certificate2("Sustainsys.Saml2.Tests.pfx"));
                });

            //services.AddAuthentication()
            //    .AddSaml2("pfizer", "pfizer", options =>
            //    {
            //        options.SPOptions.EntityId = new EntityId("https://localhost:44342/Saml2");
            //        options.IdentityProviders.Add(
            //            new IdentityProvider(
            //                new EntityId("http://localhost:52071/Metadata"), options.SPOptions)
            //            {
            //                LoadMetadata = true
            //            });

            //        options.SPOptions.ServiceCertificates.Add(new X509Certificate2("Sustainsys.Saml2.Tests.pfx"));
            //    });

        }

        private static SPOptions CreateSPOptions(string optionName)
        {
            var spOptions = new SPOptions
            {
                EntityId = new EntityId("https://localhost:44342" + "/Saml2"),
                ReturnUrl = new Uri("https://localhost:44342/" + "/Account/ExternalLoginCallback"),
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

            spOptions.ServiceCertificates.Add(new X509Certificate2("Sustainsys.Saml2.Tests.pfx"));

            return spOptions;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
