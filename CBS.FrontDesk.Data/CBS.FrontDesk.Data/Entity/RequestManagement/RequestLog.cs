//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CBS.FrontDesk.Data.Entity.RequestManagement
//{
//    public class RequestLog
//    {
//        [BsonId]
//        [BsonRepresentation(BsonType.ObjectId)]
//        public string Id { get; set; }

//        [BsonElement("IPAddress")]
//        public string IPAddress { get; set; }

//        [BsonElement("RequestUrl")]
//        public string RequestUrl { get; set; }

//        [BsonElement("RequestTime")]
//        public DateTime RequestTime { get; set; }

//        [BsonElement("IsBlocked")]
//        public bool IsBlocked { get; set; } = false;
//    }
//}
