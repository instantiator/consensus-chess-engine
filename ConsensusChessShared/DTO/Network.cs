using System;
namespace ConensusChessShared.DTO
{
	public class Network : AbstractDTO
	{
		public NetworkType Type { get; set; }
		public string NetworkServer { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string AppToken { get; set; }
        public string AccountName { get; set; }

        public string Descriptor => $"{Type}:{NetworkServer}:{AccountName}";
    }
}

