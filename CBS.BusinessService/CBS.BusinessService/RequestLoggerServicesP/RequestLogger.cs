//using CBS.FrontDesk.Data.Entity.RequestManagement;
//using CBS.FrontDesk.Data.LocalContext;
//using MongoDB.Driver;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CBS.BusinessService.RequestLoggerServicesP
//{
   
//    public class RequestLogger
//    {
//        private readonly MongoDbContext _dbContext;

//        public RequestLogger()
//        {
//            _dbContext = new MongoDbContext();
//        }

//        public async Task LogRequestAsync(string ipAddress, string url)
//        {
//            var log = new RequestLog
//            {
//                IPAddress = ipAddress,
//                RequestUrl = url,
//                RequestTime = DateTime.UtcNow
//            };

//            await _dbContext.RequestLogs.InsertOneAsync(log);
//        }

//        public async Task<bool> IsBlockedAsync(string ipAddress)
//        {
//            var filter = Builders<RequestLog>.Filter.And(
//                Builders<RequestLog>.Filter.Eq(x => x.IPAddress, ipAddress),
//                Builders<RequestLog>.Filter.Eq(x => x.IsBlocked, true)
//            );

//            return await _dbContext.RequestLogs.Find(filter).AnyAsync();
//        }

//        public async Task<int> GetRequestCountAsync(string ipAddress, TimeSpan timeWindow)
//        {
//            var filter = Builders<RequestLog>.Filter.And(
//                Builders<RequestLog>.Filter.Eq(x => x.IPAddress, ipAddress),
//                Builders<RequestLog>.Filter.Gte(x => x.RequestTime, DateTime.UtcNow - timeWindow)
//            );

//            return await _dbContext.RequestLogs.CountDocumentsAsync(filter);
//        }

//        public async Task BlockIpAsync(string ipAddress)
//        {
//            var filter = Builders<RequestLog>.Filter.Eq(x => x.IPAddress, ipAddress);
//            var update = Builders<RequestLog>.Update.Set(x => x.IsBlocked, true);

//            await _dbContext.RequestLogs.UpdateManyAsync(filter, update);
//        }
//    }
//}
