using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Diagnostics;

namespace Signicat.Basic.Example.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Index()
        {
            // For more information about the Signicat URL format, 
            // see https://labs.signicat.com/cms/blog/understanding-the-signicat-pipeline

            string target = Url.Action("Verify", "Auth", null, Request.Url.Scheme);
            string targetUrlEncoded = Url.Encode(target);
            string authenticationUrl = "https://preprod.signicat.com/std/method/shared/?id=nbid2:myprofile:&target=" + targetUrlEncoded;
            return Redirect(authenticationUrl);
        }

        public ActionResult Verify(string SAMLResponse)
        {
            try
            {
                string recipient = Url.Action("Verify", "Auth", null, Request.Url.Scheme);
                IEnumerable<Signicat.Basic.Attribute> authAttributes;
                authAttributes = Signicat.Basic.Saml.Verify(SAMLResponse, Infrastructure.Test, 10, recipient);

                // The attributes will vary between different id methods.
                // This is an example for Norwegian BankID.
                // Choose your attributes according to your requirements.
                string nationalId = authAttributes.First(a => a.Namespace == "national-id" && a.Name == "no.fnr").Value;
                string plainName = authAttributes.First(a => a.Name == "plain-name").Value;
                FormsAuthentication.SetAuthCookie(nationalId, false);
                return RedirectToAction("Granted", new { name = plainName });
            }
            catch (SignicatException x)
            {
                // Could not verify SAML response
                Log(x);
                return Redirect("Denied");
            }
            catch (ArgumentException x)
            {
                // The input to the Verify method is invalid
                Log(x);
                return Redirect("Denied");
            }
        }

        [Authorize]
        public ActionResult Granted(string name)
        {
            ViewBag.Name = name;
            ViewBag.UniqueId = User.Identity.Name;
            return View();
        }

        public ActionResult Denied()
        {
            return View();
        }

        private void Log(Exception x)
        {
            Trace.WriteLine(x.Message + "\n\n" + x.StackTrace);
        }
    }
}
