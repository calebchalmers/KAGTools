using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Helpers
{
    public static class ApiHelper
    {
        private static readonly HttpClient httpClient;

        static ApiHelper()
        {
            httpClient = new HttpClient();
        }
        }
    }
}
