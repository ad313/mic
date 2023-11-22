using System;

namespace Mic.Helpers
{
    /// <summary>
    /// Id帮助类
    /// </summary>
    public class IdHelper
    {
        /// <summary>
        /// 获取雪花Id
        /// </summary>
        /// <returns></returns>
        public static string GetSnowflakeId()
        {
            return Snowflake.Instance.NextId().ToString();
        }

        /// <summary>
        /// 获取雪花Id
        /// </summary>
        /// <returns></returns>
        public static long GetSnowflakeIdLong()
        {
            return Snowflake.Instance.NextId();
        }

        public static void SetWorkId(long workId)
        {
            Snowflake.Instance.SetWorkId(workId);
        }

        public static void SetWorkerIdBits(int workerIdBits)
        {
            Snowflake.WorkerIdBits = workerIdBits;
        }

        public static int GetWorkerIdBits()
        {
            return Snowflake.WorkerIdBits;
        }
    }

    internal class Snowflake
    {
        //机器ID 不同的机器存放不同的id  最大现在就是 -1L ^ -1L << workerIdBits;
        private static long _nodeId;
        private static readonly long Twepoch = 687888001020L; //唯一时间，这是一个避免重复的随机量，自行设定不要大于当前时间戳
        private static long _sequence = 0L;
        public static int WorkerIdBits = 6; //机器码字节数。4个字节用来保存机器码(定义为Long类型会出现，最大偏移64位，所以左移64位没有意义)
        internal static long MaxWorkerId = -1L ^ -1L << WorkerIdBits; //最大机器ID
        private static int sequenceBits = 10; //计数器字节数，10个字节用来保存计数码
        private static readonly int WorkerIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private static readonly int TimestampLeftShift = sequenceBits + WorkerIdBits; //时间戳左移动位数就是机器码和计数器总字节数
        internal static long SequenceMask = -1L ^ -1L << sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
        private long _lastTimestamp = -1L;

        internal static readonly Snowflake Instance = new Snowflake(1);

        /// <summary>
        /// 机器码
        /// </summary>
        /// <param name="workerId"></param>
        internal Snowflake(long workerId)
        {
            if (workerId > MaxWorkerId || workerId < 0)
                throw new Exception($"节点id 不能大于 {workerId} 或者 小于 0 ");
            _nodeId = workerId;
        }

        internal void SetWorkId(long workId)
        {
            _nodeId = workId;
        }

        internal long NextId()
        {
            lock (this)
            {
                long timestamp = TimeGen();
                if (this._lastTimestamp == timestamp)
                {
                    //同一微妙中生成ID
                    _sequence = (_sequence + 1) & SequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                    if (_sequence == 0)
                    {
                        //一微妙内产生的ID计数已达上限，等待下一微妙
                        timestamp = TillNextMillis(this._lastTimestamp);
                    }
                }
                else
                {
                    //不同微秒生成ID
                    _sequence = 0; //计数清0
                }
                if (timestamp < _lastTimestamp)
                {
                    //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常，因为不能保证现在生成的ID之前没有生成过
                    throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds",
                        this._lastTimestamp - timestamp));
                }

                this._lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long nextId = (timestamp - Twepoch << TimestampLeftShift) | _nodeId << WorkerIdShift | _sequence;
                return nextId;
            }
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private long TillNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns></returns>
        private long TimeGen()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
