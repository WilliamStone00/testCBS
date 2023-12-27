using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.API.Helper.LoginModel.Authenthication
{
    public class TokenRefresher
    {
        public string accessTokenBearer { get; set; }
        public string refreshTokenBearer { get; set; }
    }
}
