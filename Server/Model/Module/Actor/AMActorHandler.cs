﻿using System;
using System.Threading.Tasks;

namespace ETModel
{
	public abstract class AMActorHandler<E, Message>: IMActorHandler where E: Entity where Message : class 
	{
		protected abstract Task Run(E entity, Message message);

		public async Task Handle(Session session, Entity entity, ActorRequest actorRequest, object message)
		{
			Message msg = message as Message;
			if (msg == null)
			{
				Log.Error($"消息类型转换错误: {message.GetType().FullName} to {typeof (Message).Name}");
				return;
			}
			E e = entity as E;
			if (e == null)
			{
				Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
				return;
			}

			await this.Run(e, msg);

			// 等回调回来,session可以已经断开了,所以需要判断session id是否为0
			if (session.IsDisposed)
			{
				return;
			}
			ActorResponse response = new ActorResponse();
			session.Reply(response);
		}

		public Type GetMessageType()
		{
			return typeof (Message);
		}
	}

	public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E: Entity where Request: class, IActorRequest where Response : class, IActorResponse
	{
		protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e.ToString());
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}

		protected abstract Task Run(E unit, Request message, Action<Response> reply);

		public async Task Handle(Session session, Entity entity, ActorRequest actorRequest, object message)
		{
			try
			{
				Request request = message as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {message.GetType().FullName} to {typeof (Request).Name}");
					return;
				}
				E e = entity as E;
				if (e == null)
				{
					Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
					return;
				}

				int rpcId = request.RpcId;
				await this.Run(e, request, response =>
				{
					// 等回调回来,session可以已经断开了,所以需要判断session id是否为0
					if (session.IsDisposed)
					{
						return;
					}

					response.RpcId = rpcId;

					OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
					ushort opcode = opcodeTypeComponent.GetOpcode(response.GetType());
					byte[] repsponseBytes = session.Network.MessagePacker.SerializeToByteArray(response);

					ActorResponse actorResponse = new ActorResponse
					{
						Flag = 0x01,
						Op = opcode,
						AMessage = repsponseBytes
					};
					actorResponse.RpcId = actorRequest.RpcId;
					session.Reply(actorResponse);
				});
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}