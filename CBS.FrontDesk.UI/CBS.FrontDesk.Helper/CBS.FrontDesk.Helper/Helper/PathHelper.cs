using Microsoft.Extensions.Configuration;


namespace CBS.FrontDesk.Helper
{
    public class PathHelper
    {
        
        public IConfiguration _configuration;

        public PathHelper(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
    
        public string SMSAPIBaseEdnPoint
        {
            get
            {
                return _configuration["SMSSettings:SMSAPIBaseEdnPoint"];
            }
        }
     
        public string ComapanyName
        {
            get
            {
                return _configuration["SMSSettings:ComapanyName"];
            }
        }
        public string SpUserName
        {
            get
            {
                return _configuration["BankSettings:SpUserName"];
            }
        }
     
        public string SpPassword
        {
            get
            {
                return _configuration["BankSettings:SpPassword"];
            }
        }
        public string SendSMSURL
        {
            get
            {
                return _configuration["SMSSettings:SendSMS"];
            }
        }
        public string SMSSenderName
        {
            get
            {
                return _configuration["SMSSettings:SMSSenderName"];
            }
        }

    
        public string OpenAPIInsuranceSavingURL
        {
            get
            {
                return _configuration["OpenAPISettings:OpenAPIInsuranceSavingURL"];
            }
        }
        public string OpenAPIServiceType
        {
            get
            {
                return _configuration["OpenAPISettings:OpenAPIServiceType"];
            }
        }
        public string OpenAPIBaseEdnPoint
        {
            get
            {
                return _configuration["OpenAPISettings:OpenAPIBaseEdnPoint"];
            }
        }
        public string APIGatewayTechCareBaseEdnPoint
        {
            get
            {
                return _configuration["APIGatewayTechCareSettings:APIGatewayTechCareBaseEdnPoint"];
            }
        }
        public string GetClientByBeneficialCodeURL
        {
            get
            {
                return _configuration["APIGatewayTechCareSettings:GetClientByBeneficialCodeURL"];
            }
        }
        public string GetClientContextURL
        {
            get
            {
                return _configuration["APIGatewayTechCareSettings:GetClientContextURL"];
            }
        }
        //GetClientByBeneficialCodeURL
        public string OpenAPIInsuranceDisbustmentURL
        {
            get
            {
                return _configuration["OpenAPISettings:OpenAPIInsuranceDisbustmentURL"];
            }
        }
        public string OpenAPIInsuranceSavingCallBackURL
        {
            get
            {
                return _configuration["OpenAPISettings:OpenAPIInsuranceSavingCallBackURL"];
            }
        }
        public string OpenAPIBankID
        {
            get
            {
                return _configuration["OpenAPISettings:OpenAPIBankID"];
            }
        }
        public string UserName
        {
            get
            {
                return _configuration["Authentication:UserName"];
            }
        }
        public string Password
        {
            get
            {
                return _configuration["Authentication:Password"];
            }
        }
        public string IdentityServerBaseUrl
        {
            get
            {
                return _configuration["Authentication:IdentityServerBaseUrl"];
            }
        }
        public string AuthenthicationUrl
        {
            get
            {
                return _configuration["Authentication:AuthenthicationUrl"];
            }
        }
    }
}
