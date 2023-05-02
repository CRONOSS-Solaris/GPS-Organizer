using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace GPS_Organizer
{
    [Category("GPS_Organizer")]
    public class GPS_OrganizerCommands : CommandModule
    {

        public GPS_Organizer Plugin => (GPS_Organizer)Context.Plugin;

        [Command("test", "This is a Test Command.")]
        [Permission(MyPromoteLevel.Moderator)]
        public void Test()
        {
            Context.Respond("This is a Test from " + Context.Player);
        }

        [Command("testWithCommands", "This is a Test Command.")]
        [Permission(MyPromoteLevel.None)]
        public void TestWithArgs(string foo, string bar = null)
        {
            Context.Respond("This is a Test " + foo + ", " + bar);
        }
    }
}
