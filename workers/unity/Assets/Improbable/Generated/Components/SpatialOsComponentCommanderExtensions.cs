// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using System;
using Improbable.Unity.CodeGeneration;
using Improbable.Unity.Core;
using Improbable.Worker;
using Improbable;
using Improbable.Core;

namespace Improbable.GeneratedCode
{
    public static class SpatialOsCommanderExtensions
    {
        public static void Goto(this IComponentCommander commander, 
            EntityId entityId, global::Improbable.Core.GotoRequest request, 
            CommandCallback<global::Improbable.Core.Nothing> callback,
            TimeSpan? timeout = null)
        {
            var rawRequest = new Improbable.Core.Character.Commands.Goto.Request(request);
            commander.SendCommand<Improbable.Core.Character.Commands.Goto,
                global::Improbable.Core.Nothing>(entityId, rawRequest, ExtractResponse_Goto, callback, timeout);
        }
        
        public static ICommandResponseHandler<global::Improbable.Core.Nothing> Goto(this IComponentCommander commander, 
            EntityId entityId, global::Improbable.Core.GotoRequest request, 
            TimeSpan? timeout = null)
        {
            var rawRequest = new Improbable.Core.Character.Commands.Goto.Request(request);
            var resultHandler = new CommandResponseHandler<global::Improbable.Core.Nothing>();
            commander.SendCommand<Improbable.Core.Character.Commands.Goto,
                global::Improbable.Core.Nothing>(entityId, rawRequest, ExtractResponse_Goto, resultHandler.Trigger, timeout);
            return resultHandler;
        }

        private static global::Improbable.Core.Nothing ExtractResponse_Goto(
            ICommandResponse<Improbable.Core.Character.Commands.Goto> rawResponse)
        {
            return rawResponse.Get().Value;
        }

        public static void CreatePlayer(this IComponentCommander commander, 
            EntityId entityId, global::Improbable.Core.CreatePlayerRequest request, 
            CommandCallback<global::Improbable.Core.CreatePlayerResponse> callback,
            TimeSpan? timeout = null)
        {
            var rawRequest = new Improbable.Core.PlayerCreator.Commands.CreatePlayer.Request(request);
            commander.SendCommand<Improbable.Core.PlayerCreator.Commands.CreatePlayer,
                global::Improbable.Core.CreatePlayerResponse>(entityId, rawRequest, ExtractResponse_CreatePlayer, callback, timeout);
        }
        
        public static ICommandResponseHandler<global::Improbable.Core.CreatePlayerResponse> CreatePlayer(this IComponentCommander commander, 
            EntityId entityId, global::Improbable.Core.CreatePlayerRequest request, 
            TimeSpan? timeout = null)
        {
            var rawRequest = new Improbable.Core.PlayerCreator.Commands.CreatePlayer.Request(request);
            var resultHandler = new CommandResponseHandler<global::Improbable.Core.CreatePlayerResponse>();
            commander.SendCommand<Improbable.Core.PlayerCreator.Commands.CreatePlayer,
                global::Improbable.Core.CreatePlayerResponse>(entityId, rawRequest, ExtractResponse_CreatePlayer, resultHandler.Trigger, timeout);
            return resultHandler;
        }

        private static global::Improbable.Core.CreatePlayerResponse ExtractResponse_CreatePlayer(
            ICommandResponse<Improbable.Core.PlayerCreator.Commands.CreatePlayer> rawResponse)
        {
            return rawResponse.Get().Value;
        }

    }
}