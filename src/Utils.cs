using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;

namespace MSBuildTasks
{
    public class Utils
    {
        /// <summary>
        /// Checks whether the string supplied credential is a valid windows credential
        /// </summary>
        /// <param name="credential">The credential to check if can be impersonated IE DOMAIN\User</param>
        /// <returns>Boolean</returns>
        public Boolean IsImpersonator(String credential)
        {
            return credential != null && new NCredential().Exist(credential);
        }
        
    }
}
