using Mimic.Actors;

namespace Minimap.API
{
    internal class ActorAPI
    {
        public static ProtoActor GetLocalPlayer()
        {
            return FindActorWhere(a => a.AmIAvatar());
        }
        public static ProtoActor GetActorByID(int? actorID)
        {
            if (actorID == null) return null;
            return FindActorWhere(a => a.ActorID == actorID);
        }
        public static ProtoActor[] GetAlivePlayers()
        {
            return FindActorsWhere(a => a.dead == false && a.ActorType == ReluProtocol.Enum.ActorType.Player);
        }
        public static ProtoActor[] FindActorsWhere(Func<ProtoActor, bool> predicate)
        {
            try
            {
                ProtoActor[] allActors = UnityObjectAPI.FindObjectsOfType<ProtoActor>();
                return [.. allActors.Where(predicate)];
            }
            catch
            {
                return null;
            }
        }
        public static ProtoActor FindActorWhere(Func<ProtoActor, bool> predicate)
        {
            try
            {
                ProtoActor[] allActors = UnityObjectAPI.FindObjectsOfType<ProtoActor>();
                return allActors.FirstOrDefault(predicate);
            }
            catch
            {
                return null;
            }
        }

        public static string GetActorName(ProtoActor? actor)
        {
            if (actor == null) return "Unknown";
            return actor.nickName;
        }
    }
}
