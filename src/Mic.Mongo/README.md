## MongoDB

### Init
#### 1、注入 HostModel
    MgClient.Init(new HostModel()
            {
                Host = "192.168.1.120",
                Port = 31575
            });

#### 2、定义实体
    [Table(Name = "AccessControlSystemLog", Database = "ArcFaceStore")] //指定数据库和表
    public class AccessControlSystemLog : EntityBase
    {
        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Height { get; set; }

        /// <summary>
        /// 创建时间 指定本地时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationTime { get; set; }

        public float Threshold { get; set; }
    }
        
#### 3、使用
     MgClient.InsertAsync(model);//insert
     MgClient.GetCollection<AccessControlSystemLog>().Where(xxxxx=>xxxxxxxx);
    