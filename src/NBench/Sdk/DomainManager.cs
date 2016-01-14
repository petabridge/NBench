using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NBench.Sdk
{
    /// <summary>
    /// Handles the creation and unloading of AppDomains used for each test file
    /// </summary>
    public class DomainManager
    {
        /// <summary>
        /// Creates a new AppDomain for running the test package
        /// </summary>
        /// <param name="package">The test package to be run</param>
        /// <returns></returns>
        public static AppDomain CreateDomain(TestPackage package)
        {
            AppDomainSetup setup = CreateAppDomainSetup(package);

            string domainName = "test-domain-" + package.Name;           

            return AppDomain.CreateDomain(domainName, null, setup);
        }

        /// <summary>
        /// Unloads the test runner app domain.
        /// </summary>
        /// <param name="domain">The domain to unload.</param>
        public static void UnloadDomain(AppDomain domain)
        {
            AppDomain.Unload(domain);
        }

        /// <summary>
        /// Creates a new AppDomainSetup
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private static AppDomainSetup CreateAppDomainSetup(TestPackage package)
        {
            AppDomainSetup setup = new AppDomainSetup();

            // We need to use distinct application name, when runnin tests in parallel
            setup.ApplicationName = string.Format("Tests_{0}", Environment.TickCount);
     
            setup.ApplicationBase = package.GetBasePath();
            setup.ConfigurationFile = package.ConfigurationFile;

            return setup;
        }
    }
}
