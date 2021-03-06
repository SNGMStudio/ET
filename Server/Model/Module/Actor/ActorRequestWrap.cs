﻿using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ETModel
{
	/// <summary>
	/// 用来包装actor消息
	/// </summary>
	[Message(Opcode.ActorRequest)]
	[ProtoContract]
	public partial class ActorRequest : IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Id { get; set; }

		[ProtoMember(2, IsRequired = true)]
		public ushort Op { get; set; }

		[ProtoMember(3, IsRequired = true)]
		public byte[] AMessage { get; set; }
	}

	/// <summary>
	/// actor RPC消息响应
	/// </summary>
	[Message(Opcode.ActorResponse)]
	[ProtoContract]
	public partial class ActorResponse : IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public byte Flag { get; set; }

		[ProtoMember(2, IsRequired = true)]
		public ushort Op { get; set; }

		[ProtoMember(3, IsRequired = true)]
		public byte[] AMessage { get; set; }
	}
}
