//using CBS.FrontDesk.Data.Entity.RequestManagement;
//using CBS.FrontDesk.Data.LocalContext;
//using MongoDB.Driver;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Timers;

//namespace CBS.BusinessService.RequestLoggerServicesP
//{
//    public class UnblockIpService
//    {
//        private readonly MongoDbContext _dbContext;
//        private readonly Timer _timer;

//        public UnblockIpService()
//        {
//            _dbContext = new MongoDbContext();
//            _timer = new Timer(1800000); // 30 minutes
//            _timer.Elapsed += UnblockIps;
//            _timer.Start();
//        }

//        private async void UnblockIps(object sender, ElapsedEventArgs e)
//        {
//            var filter = Builders<RequestLog>.Filter.And(
//                Builders<RequestLog>.Filter.Eq(x => x.IsBlocked, true),
//                Builders<RequestLog>.Filter.Lt(x => x.RequestTime, DateTime.UtcNow.AddMinutes(-30))
//            );

//            var update = Builders<RequestLog>.Update.Set(x => x.IsBlocked, false);
//            await _dbContext.RequestLogs.UpdateManyAsync(filter, update);
//        }
//    }
//}
