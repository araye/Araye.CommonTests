using Araye.Code.Extensions;
using Araye.Code.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Araye.CommonTests.Security
{
    /// <summary>
    /// collection of general test methods for security attributes
    /// </summary>
    public class Tests
    {
        #region CSRF Tests
        // more inforamation about CSRF https://www.owasp.org/index.php/Cross-Site_Request_Forgery_(CSRF)

        [TestMethod]
        public static void Area_Controllers_Methods_Must_Have_CSRF_Token(Type testController, string areaNamespace)
        {
            var areaControllers = from t in Assembly.GetAssembly(testController).GetTypes()
                                  where t.IsClass && t.Namespace == areaNamespace
                                  && t.IsSubclassOf(typeof(Controller))
                                  select t;
            foreach (Type controller in areaControllers)
            {
                Verify_All_Post_ActionMethods_Have_CSRF_Tokens(controller);
            }
        }

        [TestMethod]
        public static void Verify_All_Post_ActionMethods_Have_CSRF_Tokens(Type testController)
        {

            var actionMethods = testController.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                              .Where(method => typeof(ActionResult).IsAssignableFrom(method.ReturnType))
                                              .ToList();

            foreach (var method in from method in actionMethods let httpPostAttributes = method.GetCustomAttributes(typeof(HttpPostAttribute), true) where httpPostAttributes != null && httpPostAttributes.Any() let csrfTokens = method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true) where csrfTokens == null || !csrfTokens.Any() select method)
            {
                Trace.WriteLine($"no [ValidateAntiForgeryToken] found for method {method.Name} in controller {testController.FullName}");
                Assert.Fail();
            }
        }

        #endregion

        #region ASP.NET Authorize Attributes Tests

        [TestMethod]
        public static void Certain_Roles_Have_Access_To_Area_Controllers(Type testController, string areaNamespace, string[] roles)
        {
            var areaControllers = from t in Assembly.GetAssembly(testController).GetTypes()
                                  where t.IsClass && t.Namespace == areaNamespace
                                  && t.IsSubclassOf(typeof(Controller))
                                  select t;
            foreach (Type controller in areaControllers)
            {
                Must_Have_Require_Authorization(controller, roles);
            }
        }

        [TestMethod]
        public static AuthorizeAttribute Controller_Must_Have_AuthorizationAttribute(Type testController)
        {
            var attributes = testController.GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.IsTrue(attributes.Any(), $"No [AuthorizeAttribute] found for controller {testController.FullName}");
            return attributes.Any() ? attributes[0] as AuthorizeAttribute : null;
        }

        [TestMethod]
        public static AuthorizeAttribute Action_Must_Have_AuthorizationAttribute(Type testController, string methodName, Type[] parameters)
        {
            var methodInfo = testController.GetMethod(methodName, parameters);
            var attributes = methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.IsTrue(attributes.Any(), $"No [AuthorizeAttribute] found for {methodInfo.Name} in controller {testController.FullName}");
            return attributes.Any() ? attributes[0] as AuthorizeAttribute : null;
        }

        [TestMethod]
        public static void Must_Have_Require_Authorization(Type controller, string[] roles)
        {
            var authorizeAttribute = Controller_Must_Have_AuthorizationAttribute(controller);
            if (!roles.Any())
            {
                return;
            }

            if (authorizeAttribute == null)
            {
                return;
            }
            var firstOrdered = authorizeAttribute.Roles.Split(',')
                                          .Select(t => t.Trim())
                                          .OrderBy(t => t);
            var secondOrdered = roles.Select(t => t.Trim())
                                            .OrderBy(t => t);
            bool stringsAreEqual = firstOrdered.SequenceEqual(secondOrdered);
            Assert.IsTrue(stringsAreEqual);
        }

        [TestMethod]
        public static void Must_Require_Authorization(Type controller, string methodName, Type[] methodParameters, string[] roles)
        {
            var authorizeAttribute = Action_Must_Have_AuthorizationAttribute(controller, methodName, methodParameters);
            if (!roles.Any())
            {
                return;
            }

            if (authorizeAttribute == null)
            {
                return;
            }

            bool all = authorizeAttribute.Roles.Split(',').All(r => roles.Contains(r.Trim()));
            Assert.IsTrue(all);
        }

        #endregion

        #region Araye Authorize Attrbites Tests

        [TestMethod]
        public static AuthorizeAttribute Controller_Must_Have_ArayePermissionAttribute(Type testController)
        {
            var attributes = testController.GetCustomAttributes(typeof(ArayePermissionAttribute), true);
            Assert.IsTrue(attributes.Any(), $"No [ArayePermissionAttribute] found for controller {testController.FullName}");
            return attributes.Any() ? attributes[0] as AuthorizeAttribute : null;
        }

        [TestMethod]
        public static void Verify_All_ActionMethods_Have_ArayeActionPermission_Attribute(Type testController)
        {

            var actionMethods = testController.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                              .Where(method => typeof(ActionResult).IsAssignableFrom(method.ReturnType))
                                              .ToList();

            foreach (var method in from method in actionMethods where method.GetCustomAttributes(typeof(ArayeActionPermissionAttribute), true).None() select method)
            {
                if (method.GetCustomAttributes(typeof(NonActionAttribute), true).None())
                {
                    Trace.WriteLine($"no [ArayeActionPermission] found for method {method.Name} in controller {testController.FullName}");
                    Assert.Fail();
                }
            }
        }

        #endregion
        
    }
}
