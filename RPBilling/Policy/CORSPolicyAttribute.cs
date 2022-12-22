using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace RackPeople.BillingAPI.Policy
{
    public class CORSPolicyAttribute : Attribute, ICorsPolicyProvider
    {
        private CorsPolicy _policy;

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request) {
            return Task.FromResult(_policy);
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return Task.FromResult(_policy);
        }

        public CORSPolicyAttribute(params string[] origins) {
            this._policy = new CorsPolicy() {
                AllowAnyMethod = true,
                AllowAnyHeader = true,
            };

            // Add the custom domains
            foreach (var origin in origins) {
                this._policy.Origins.Add(origin);
            }
        }
    }
}