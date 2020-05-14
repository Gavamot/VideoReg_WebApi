using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApi.Core
{
    public class UrlParameterBuilder
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        readonly string url;

        public UrlParameterBuilder(string url)
        {
            this.url = url;
        }


        public void AddParameter(string key, object value)
        {
            parameters.Add(key, value.ToString());
        }

        public string Build()
        {
            return AddParametersToUrl(parameters);
        }

        string AddParametersToUrl(Dictionary<string, string> parameters)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var p in parameters)
            {
                query[p.Key] = p.Value;
            }
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        string AddParametersToUrl(params object[] parameters)
        {
            if (parameters.Length % 2 != 0) throw new ArgumentException("Parameters count must to even");
            var dictionary = new Dictionary<string, string>();
            for (int i = 0; i < parameters.Length; i += 2)
            {
                dictionary.Add(parameters[i].ToString(), parameters[i + 1].ToString());
            }
            return AddParametersToUrl(url, dictionary);
        }
    }
}
